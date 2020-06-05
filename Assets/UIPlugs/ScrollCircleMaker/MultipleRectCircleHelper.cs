//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker       //多行矩形滑动循环
{

    public sealed class MultipleRectCircleHelper<T> : BaseCircleHelper<T>
    {
        private GridLayoutGroup _gridLayoutGroup;
        private SizeInt _wholeSize, _maxRanks;
        private BoundaryInt _cExtra;
        private Vector2 _tmpContentPos;
        private bool _lockSlide, _firstRun;
        private int _tmpTotalItems;
        private float _timer = 0;
        /// <summary>
        /// 偏移锚点
        /// </summary>
        private int _contentSite
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        return _gridLayoutGroup.padding.top;
                    case ScrollDir.BottomToTop:
                        return _gridLayoutGroup.padding.bottom;
                    case ScrollDir.LeftToRight:
                        return _gridLayoutGroup.padding.left;
                    default:
                        return _gridLayoutGroup.padding.right;
                }
            }
            set
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        _gridLayoutGroup.padding.top = value;
                        break;
                    case ScrollDir.BottomToTop:
                        _gridLayoutGroup.padding.bottom = value;
                        break;
                    case ScrollDir.LeftToRight:
                        _gridLayoutGroup.padding.left = value;
                        break;
                    default:
                        _gridLayoutGroup.padding.right = value;
                        break;
                }
            }
        }
        /// <summary>
        /// 上界限判断
        /// </summary>
        private bool _lowerDefine
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        return _contentRect.anchoredPosition.y <= _cExtra.dir;
                    case ScrollDir.BottomToTop:
                        return _contentRect.anchoredPosition.y >= _cExtra.dir;
                    case ScrollDir.LeftToRight:
                        return _contentRect.anchoredPosition.x >= _cExtra.dir;
                    default:
                        return _contentRect.anchoredPosition.x <= _cExtra.dir;
                }
            }
        }
        /// <summary>
        /// 下界限判断
        /// </summary>
        private bool _highDefine
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        return Mathf.Abs(_contentRect.anchoredPosition.y) >= (int)(_contentRect.rect.height - _viewRect.rect.height);
                    default:
                        return Mathf.Abs(_contentRect.anchoredPosition.x) >= (int)(_contentRect.rect.width - _viewRect.rect.width);
                }
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="contentTrans">content的transform组件</param>
        /// <param name="createItemFunc">创建item函数</param>
        public MultipleRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc)
        {
            _createItemFunc = createItemFunc;
            _contentRect = contentTrans as RectTransform;
            _viewRect = contentTrans.parent.GetComponent<RectTransform>();
            _scrollRect = _viewRect.parent.GetComponent<ScrollRect>(); 
            _sProperty = _contentRect.GetComponent<ScrollCircleComponent>();
            if (_sProperty == null) UnityEngine.Debug.LogError("Content must have ScrollCircleComponent!");

            _baseItem = _sProperty.baseItem;
            _gridLayoutGroup = _contentRect.GetComponent<GridLayoutGroup>() ?? _contentRect.gameObject.AddComponent<GridLayoutGroup>();
            _itemRect = _baseItem.transform.GetComponent<RectTransform>();
            _scrollRect.onValueChanged.AddListener(OnRefreshHandler);
            _itemSet = new List<BaseItem<T>>();
            _dataSet = new List<T>();
            OnInit();
        }
        /// <summary>
        /// 初始化配置
        /// </summary>
        private void OnInit()
        {
            _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
            _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                    float tmpWidth = _contentRect.rect.width - _sProperty.LeftExt - _sProperty.RightExt;
                    _maxRanks.Width = (int)((tmpWidth + _sProperty.WidthExt) / _wholeSize.Width);
                    _maxRanks.Height = (int)(Math.Ceiling(_viewRect.rect.height / _wholeSize.Height) + 1);
                    _sProperty.maxItems = _maxRanks.Width * _maxRanks.Height;
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                    float tmpHeight = _contentRect.rect.height - _sProperty.TopExt - _sProperty.BottomExt;
                    _maxRanks.Height = (int)((tmpHeight + _sProperty.HeightExt) / _wholeSize.Height);
                    _maxRanks.Width = (int)(Math.Ceiling(_viewRect.rect.width / _wholeSize.Width) + 1);
                    _sProperty.maxItems = _maxRanks.Width * _maxRanks.Height;
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    break;
            }
            _gridLayoutGroup.cellSize = _itemRect.rect.size;
            _gridLayoutGroup.padding.left = _sProperty.LeftExt;
            _gridLayoutGroup.padding.right = _sProperty.RightExt;
            _gridLayoutGroup.padding.top = _sProperty.TopExt;
            _gridLayoutGroup.padding.bottom = _sProperty.BottomExt;
            _gridLayoutGroup.spacing = new Vector2(_sProperty.WidthExt, _sProperty.HeightExt);
            OnResolveGroupEnum();
        }
        /// <summary>
        /// 解析排版
        /// </summary>
        private void OnResolveGroupEnum()
        {
            int sign = (short)_sProperty.scrollDir * 10 + (short)_sProperty.scrollSort;
            switch (sign)
            {
                case 0:case 2:case 20:case 22:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
                    break;
                case 1:case 3:case 30:case 32:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperRight;
                    break;
                case 10:case 12:case 21:case 23:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerLeft;
                    break;
                case 11:case 13:case 31:case 33:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerRight;
                    break;
            }
            switch (sign)
            {
                case 0:case 1:case 2:case 3:
                    _cExtra.dir = 1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    break;
                case 10:case 11: case 12:case 13:
                    _cExtra.dir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    _gridLayoutGroup.childAlignment = TextAnchor.LowerLeft;
                    break;
                case 20:case 21:case 22:case 23:
                    _cExtra.dir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    break;
                case 30: case 31:case 32:case 33:
                    _cExtra.dir = 1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperRight;
                    break;
            }
        }
        /// <summary>
        /// 刷新item定位
        /// </summary>
        /// <param name="v2">方向向量</param>
        protected override void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            if (_lockSlide) return;
            if (_timer < _sProperty.refreshRatio){
                _timer += Time.deltaTime;
                return;
            }

            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    if (_sProperty.scrollType == ScrollType.Limit)
                    {
                        _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y);
                        _tmpContentPos.y = _scrollRect.velocity.y > 0 ?
                        _tmpContentPos.y + _wholeSize.Height * _sProperty.limitNum *_cExtra.dir:
                        _tmpContentPos.y - _wholeSize.Height * _sProperty.limitNum * _cExtra.dir;
                        ToLocation((int)_tmpContentPos.y);
                    }
                    else if (_sProperty.isCircleEnable)
                        OnCircleVertical();
                    else
                        OnCircleVerticalNo();
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    if (_sProperty.scrollType == ScrollType.Limit)
                    {
                        _tmpContentPos.x = Mathf.Abs(_tmpContentPos.x);
                        _tmpContentPos.x = _scrollRect.velocity.x > 0 ?
                        _tmpContentPos.x + _wholeSize.Width * _sProperty.limitNum * _cExtra.dir :
                        _tmpContentPos.x - _wholeSize.Width * _sProperty.limitNum * _cExtra.dir;
                        ToLocation((int)_tmpContentPos.x);
                    }
                    else if (_sProperty.isCircleEnable)
                        OnCircleHorizontal();
                    else
                        OnCircleHorizontalNo();
                    break;
            }
            _timer = 0;
        }
        /// <summary>
        /// 启动滑动
        /// </summary>
        /// <param name="_tmpDataSet">数据集</param>
        public override void OnStart(List<T> _tmpDataSet = null)
        {                    
            if (_tmpDataSet != null)
            {
                if(_sProperty.scrollSort == ScrollSort.BackDir ||
                _sProperty.scrollSort == ScrollSort.BackZDir)
                    _tmpDataSet.Reverse();
                _dataSet.AddRange(_tmpDataSet);
            }
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                if (i >= _dataSet.Count) break;//表示没有数据
                InitItem(i);
            }
            if (_sProperty.isCircleEnable)
                OnAnchorSet();    
            else
                OnAnchorSetNo();
            _scrollRect.inertia = _sProperty.scrollType != ScrollType.Drag;
            _firstRun = true;
        }
        /// <summary>
        /// 移除Item by 索引
        /// </summary>
        /// <param name="itemIdx">索引</param>
        public override void DelItem(int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= _dataSet.Count)
            {
                UnityEngine.Debug.LogError("DelItem超范围！");
                return;
            }
                
            _dataSet.RemoveAt(itemIdx);
            if (_firstRun)//表示已经初始化，需要计算偏移
                ToAutoSite(false);
        }
        /// <summary>
        /// 移除Item by 匹配函数
        /// </summary>
        /// <param name="seekFunc">1是需要移除的数据，2是数据集里的数据</param>
        /// <param name="data">需要移除的参数</param>
        public override void DelItem(Func<T,T,bool> seekFunc,T data)
        {
            bool seekSwitch = false;
            for (int i = _dataSet.Count - 1; i >= 0; ++i)
            {
                seekSwitch = seekFunc(data, _dataSet[i]);
                if (seekSwitch)
                {
                    _dataSet.RemoveAt(i);
                    break;
                }
            }

            if (_firstRun && seekSwitch)//表示已经初始化，需要计算偏移
                ToAutoSite(false);
        }
        /// <summary>
        /// 添加item
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="itemIdx">位置</param>
        public override void AddItem(T data, int itemIdx = -1)
        {
            if (itemIdx != -1) 
                itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count - 1);

            switch (_sProperty.scrollSort)
            {
                case ScrollSort.FrontDir:
                case ScrollSort.FrontZDir:
                    if (itemIdx != -1)
                        _dataSet.Insert(itemIdx, data);
                    else
                        _dataSet.Add(data);
                    break;
                case ScrollSort.BackDir:
                case ScrollSort.BackZDir:
                    if (itemIdx != -1)
                        _dataSet.Insert(_dataSet.Count - itemIdx - 1, data);
                    else
                        _dataSet.Insert(0, data);
                    break;
            }
            if(_firstRun)
                ToAutoSite(true);
        }
        /// <summary>
        /// 更新item样式
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="itemIdx">位置</param>
        public override void UpdateItem(T data, int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= _dataSet.Count)
            {
                UnityEngine.Debug.LogError("UpdateItem超范围！");
                return;
            }
            _dataSet[itemIdx] = data;
            int tmpItemIdx,tmpOffset;
            tmpOffset = _sProperty.dataIdx > itemIdx ?_tmpTotalItems - 
                    _sProperty.dataIdx + itemIdx: itemIdx - _sProperty.dataIdx;
            if (tmpOffset < _sProperty.maxItems)
            {
                tmpItemIdx = (_sProperty.itemIdx + tmpOffset) % _sProperty.maxItems;
                if (tmpItemIdx < _itemSet.Count)
                    _itemSet[tmpItemIdx].UpdateView(data);
            }      
        }
        /// <summary>
        /// 重置启动器
        /// </summary>
        public override void ResetItems()
        {
            _dataSet.Clear();
            _itemSet.Clear();
            Vector2 contentSize = _contentRect.sizeDelta;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    contentSize.y = 0;
                    _contentRect.sizeDelta = contentSize;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    contentSize.x = 0;
                    _contentRect.sizeDelta = contentSize;
                    break;
            }
        }
        /// <summary>
        /// 获取当前定位信息
        /// </summary>
        /// <returns></returns>
        public override int GetLocation()
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    return (int)Mathf.Abs(_contentRect.rect.y);
                default:
                    return (int)Mathf.Abs(_contentRect.rect.x);
            }
        }
        /// <summary>
        /// 定位接口
        /// </summary>
        /// <param name="toSeat">位置</param>
        /// <param name="isDrawEnable">是否需要动画</param>
        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {
            if (_sProperty.isCircleEnable)
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        if (isDrawEnable)
                            _sProperty.StartCoroutine(ToAutoMoveVSeat(toSeat));
                        else
                            ToDirectVSeat(toSeat);
                        break;
                    default:
                        if (isDrawEnable)
                            _sProperty.StartCoroutine(ToAutoMoveHSeat(toSeat));
                        else
                            ToDirectHSeat(toSeat);
                        break;
                }
            }
            else
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        if (isDrawEnable)
                            _sProperty.StartCoroutine(ToAutoMoveVSeatNo(toSeat));
                        else
                            ToDirectVSeatNo(toSeat);
                        break;
                    default:
                        if (isDrawEnable)
                            _sProperty.StartCoroutine(ToAutoMoveHSeatNo(toSeat));
                        else
                            ToDirectHSeatNo(toSeat);
                        break;
                }
            }
        }
        /// <summary>
        /// 上定位
        /// </summary>
        /// <param name="isDrawEnable">是否需要动画</param>
        public override void ToTop(bool isDrawEnable = true)
        {
            if (_sProperty.isCircleEnable)
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        ToLocation((int)_viewRect.rect.height + _sProperty.HeightExt, isDrawEnable);
                        break;
                    default:
                        ToLocation((int)_viewRect.rect.width + _sProperty.WidthExt, isDrawEnable);
                        break;
                }
            }
            else
            {
                ToLocation(0, isDrawEnable);
            }
        }
        /// <summary>
        /// 下定位
        /// </summary>
        /// <param name="isDrawEnable">是否需要动画</param>
        public override void ToBottom(bool isDrawEnable = true)
        {
            ToLocation(_cExtra.area, isDrawEnable);
        }
        /// <summary>
        /// 销毁item
        /// </summary>
        /// <param name="itemIdx">位置</param>
        private void ClearItem(int itemIdx)
        {
            GameObject.Destroy(_itemSet[itemIdx - 1].gameObject);
            _itemSet.RemoveAt(itemIdx - 1);
        }
        /// <summary>
        /// 初始化item
        /// </summary>
        /// <param name="itemIdx">位置</param>
        private void InitItem(int itemIdx)
        {
            BaseItem<T> baseItem = _createItemFunc();
            RectTransform itemRect = UnityEngine.Object.Instantiate(_baseItem, _contentRect).transform as RectTransform;
            baseItem.transform = itemRect as Transform;
            baseItem.gameObject = itemRect.gameObject;
            baseItem.gameObject.name = _baseItem.name + itemIdx;
            baseItem.InitComponents();
            baseItem.InitEvents();
            baseItem.UpdateView(_dataSet[itemIdx]);
            _itemSet.Add(baseItem);
        }
        /// <summary>
        /// 自适应改变
        /// </summary>
        /// <param name="state">t添加f移除</param>
        private void ToAutoSite(bool state)
        {
            if (state && _itemSet.Count < _sProperty.maxItems)
                InitItem(_itemSet.Count);
            else if(!state && _dataSet.Count < _itemSet.Count)
                ClearItem(_itemSet.Count);

            if (_sProperty.isCircleEnable)
                OnAnchorSet();
            else
                OnAnchorSetNo();
            RefreshItems();
        }
        /// <summary>
        /// 对齐偏移
        /// </summary>
        /// <param name="tmpItemIdx"></param>
        private void ToItemAline(int tmpItemIdx)
        {
            for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                _itemSet[i].transform.SetAsLastSibling();
            for (int i = tmpItemIdx - 1; i >= _sProperty.itemIdx; --i)
                _itemSet[i].transform.SetAsFirstSibling();
        }
        /// <summary>
        /// 强制刷新所有items
        /// </summary>
        private void RefreshItems()
        {
            int tmpItemIdx, tmpDataIdx;
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                if (i >= _itemSet.Count) return;
                tmpDataIdx = (_sProperty.dataIdx + i) % _tmpTotalItems;
                tmpItemIdx = (_sProperty.itemIdx + i) % _sProperty.maxItems;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                if (tmpDataIdx >= 0 && tmpDataIdx < _dataSet.Count)
                {
                    if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                        _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                }
                else
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
            }
            _gridLayoutGroup.SetLayoutVertical();
        }
        /// <summary>
        /// 下滑动时刷新接口
        /// </summary>
        private void OnRefreshItemDown()
        {
            int tmpRow, tmpSize;
            tmpRow = _scrollRect.vertical ? _maxRanks.Width : _maxRanks.Height;
            tmpSize = _scrollRect.vertical ? _wholeSize.Height : _wholeSize.Width;

            int tmpItemIdx, tmpDataIdx;
            for (int i = 0; i < tmpRow; ++i)
            {
                tmpItemIdx = _sProperty.itemIdx + i;
                tmpDataIdx = (_sProperty.dataIdx + _sProperty.maxItems + i) % _tmpTotalItems;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                if (tmpDataIdx < _dataSet.Count)
                {
                    if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                        _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                }
                else
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
                _itemSet[tmpItemIdx].transform.SetAsLastSibling();
            }
            _sProperty.dataIdx = _sProperty.dataIdx + tmpRow >=
                _dataSet.Count ? 0 : _sProperty.dataIdx + tmpRow;
            _sProperty.itemIdx = _sProperty.itemIdx + tmpRow >=
                _sProperty.maxItems ? 0 : _sProperty.itemIdx + tmpRow;
            _contentSite += tmpSize;
            _gridLayoutGroup.SetLayoutVertical();
        }
        /// <summary>
        /// 上滑动时刷新接口
        /// </summary>
        private void OnRefreshItemUp() //true表示水平
        {
            int tmpRow, tmpSize;
            tmpRow = _scrollRect.vertical ? _maxRanks.Width : _maxRanks.Height;
            tmpSize = _scrollRect.vertical ? _wholeSize.Height : _wholeSize.Width;

            _sProperty.dataIdx = _sProperty.dataIdx - tmpRow < 0 ?
                _tmpTotalItems - tmpRow : _sProperty.dataIdx - tmpRow;
            _sProperty.itemIdx = _sProperty.itemIdx - tmpRow < 0 ?
                _sProperty.maxItems - tmpRow : _sProperty.itemIdx - tmpRow;
            int tmpItemIdx, tmpDataIdx;
            for (int i = tmpRow - 1; i >= 0; --i)
            {
                tmpItemIdx = _sProperty.itemIdx + i;
                tmpDataIdx = _sProperty.dataIdx + i;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                if (tmpDataIdx < _dataSet.Count)
                {
                    if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                        _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                }
                else
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
                _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
            }
            _contentSite -= tmpSize;
            _gridLayoutGroup.SetLayoutVertical();
        }

#region//-------------------------------------循环滑动方式------------------------------------------//
        /// <summary>
        /// 垂直滑动循环式
        /// </summary>
        private void OnCircleVertical()
        {
            int tmpItemIdx;Vector2 tmpForce;
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y);
            while (_tmpContentPos.y  > _contentSite  + _wholeSize.Height)//向下
                OnRefreshItemDown();
            while (_tmpContentPos.y < _contentSite - _sProperty.HeightExt)
                OnRefreshItemUp();
    
            if (_highDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.itemIdx = _sProperty.dataIdx = 0;
                _contentSite = (int)_viewRect.rect.height + _sProperty.HeightExt;
                _tmpContentPos.y = (_viewRect.rect.height + _sProperty.HeightExt) * _cExtra.dir;
                _contentRect.anchoredPosition = _tmpContentPos;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemAline(tmpItemIdx);
                RefreshItems();              
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
            //强制下定位
            else if (_lowerDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _tmpTotalItems - _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.maxItems;
                _contentSite = (int)_viewRect.rect.height + _sProperty.HeightExt 
                    + _sProperty.dataIdx * _wholeSize.Height / _maxRanks.Width;
                _tmpContentPos.y = _cExtra.area * _cExtra.dir;
                _contentRect.anchoredPosition = _tmpContentPos;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemAline(tmpItemIdx);
                RefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
        }
        /// <summary>
        /// 水平滑动循环式
        /// </summary>
        private void OnCircleHorizontal()
        {
            int tmpItemIdx; Vector2 tmpForce;
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = Mathf.Abs(_tmpContentPos.x);
            while (_tmpContentPos.x > _contentSite + _wholeSize.Width)//向下
                OnRefreshItemDown();
            while (_tmpContentPos.x < _contentSite - _sProperty.WidthExt)
                OnRefreshItemUp();   
            if (_highDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.itemIdx = _sProperty.dataIdx = 0;
                _contentSite = (int)_viewRect.rect.width + _sProperty.WidthExt;
                _tmpContentPos.x = (_viewRect.rect.width + _sProperty.WidthExt) * _cExtra.dir;
                _contentRect.anchoredPosition = _tmpContentPos;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemAline(tmpItemIdx);
                RefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
            //强制下定位
            else if (_lowerDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _tmpTotalItems - _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.maxItems;
                _contentSite = (int)_viewRect.rect.width + _sProperty.WidthExt
                    + _sProperty.dataIdx * _wholeSize.Width / _maxRanks.Height;
                _tmpContentPos.x = _cExtra.area * _cExtra.dir;
                _contentRect.anchoredPosition = _tmpContentPos;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemAline(tmpItemIdx);
                RefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
        }
        /// <summary>
        /// 自动垂直定位循环式
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveVSeat(int toSeat)
        {
            int tmpToSeat = toSeat;
            toSeat = Mathf.Clamp(toSeat, 0, (int)(_contentRect.rect.height - _viewRect.rect.height));
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _wholeSize.Height)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _cExtra.dir < toSeat)
                {
                    _tmpContentPos.y = _tmpContentPos.y + _cExtra.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.y = toSeat * _cExtra.dir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _wholeSize.Height)
                {
                    OnRefreshItemDown();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.HeightExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _cExtra.dir > toSeat)
                {
                    _tmpContentPos.y = _tmpContentPos.y - _cExtra.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.y = toSeat * _cExtra.dir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (_tmpContentPos.y * _cExtra.dir < _contentSite - _sProperty.HeightExt)
                {
                    OnRefreshItemUp();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.y = toSeat * _cExtra.dir;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
            _toLocationEvent?.Invoke();

            //如果是限速滑动，需要成环
            if (_sProperty.scrollType == ScrollType.Limit)
            {              
                OnCircleVertical();
                _tmpContentPos = _contentRect.anchoredPosition;
                if (tmpToSeat < 0){
                    _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y) + tmpToSeat;
                    ToLocation((int)_tmpContentPos.y);
                }
                else if(tmpToSeat != toSeat){
                    _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y) + tmpToSeat - toSeat;
                    ToLocation((int)_tmpContentPos.y);
                }
            }
        }
        /// <summary>
        /// 自动水平定位循环式
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveHSeat(int toSeat)
        {
            int tmpToSeat = toSeat;
            toSeat = Mathf.Clamp(toSeat, 0, (int)(_contentRect.rect.width - _viewRect.rect.width));
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _wholeSize.Width)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _cExtra.dir < toSeat)
                {
                    _tmpContentPos.x = _tmpContentPos.x + _cExtra.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.x = toSeat * _cExtra.dir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _wholeSize.Width)
                {
                    OnRefreshItemDown();                  
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _cExtra.dir > toSeat)
                {
                    _tmpContentPos.x = _tmpContentPos.x - _cExtra.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.x = toSeat * _cExtra.dir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (_tmpContentPos.x * _cExtra.dir < _contentSite - _sProperty.WidthExt)
                {
                    OnRefreshItemUp();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.x = toSeat * _cExtra.dir;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
            _toLocationEvent?.Invoke();

            //如果是限速滑动，需要成环
            if (_sProperty.scrollType == ScrollType.Limit)
            {
                OnCircleHorizontal();
                _tmpContentPos = _contentRect.anchoredPosition;
                if (tmpToSeat < 0)
                {
                    _tmpContentPos.x = Mathf.Abs(_tmpContentPos.x) + tmpToSeat;
                    ToLocation((int)_tmpContentPos.x);
                }
                else if (tmpToSeat != toSeat)
                {
                    _tmpContentPos.x = Mathf.Abs(_tmpContentPos.x) + tmpToSeat - toSeat;
                    ToLocation((int)_tmpContentPos.x);
                }
            }
        }
        /// <summary>
        /// 强制垂直定位循环式
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectVSeat(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area + (int)_viewRect.rect.height);
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = toSeat * _cExtra.dir;
            _contentRect.anchoredPosition = _tmpContentPos;

            int tmpItemIdx = _sProperty.itemIdx, tmpRow;
            if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _wholeSize.Height)//向下
            {
                tmpRow = (int)(Mathf.Abs(_tmpContentPos.y) - _contentSite) / _wholeSize.Height;
                _contentSite += _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = (_sProperty.dataIdx + _maxRanks.Width * tmpRow) % _tmpTotalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + _maxRanks.Width * tmpRow) % _sProperty.maxItems;
                ToItemAline(tmpItemIdx);
            }
            else if (Mathf.Abs(_tmpContentPos.y) < _contentSite - _sProperty.HeightExt)
            {
                tmpRow = (int)Math.Ceiling((_contentSite - Mathf.Abs(_tmpContentPos.y)
                    - _sProperty.HeightExt) / _wholeSize.Height);
                _contentSite -= _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width * tmpRow % _tmpTotalItems;
                _sProperty.dataIdx = _sProperty.dataIdx < 0 ? _tmpTotalItems + _sProperty.dataIdx : _sProperty.dataIdx;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width * tmpRow % _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemAline(tmpItemIdx);
            }
            RefreshItems();          
        }
        /// <summary>
        /// 强制水平定位循环式
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectHSeat(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area + (int)_viewRect.rect.width);
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = toSeat * _cExtra.dir;
            _contentRect.anchoredPosition = _tmpContentPos;

            int tmpItemIdx = _sProperty.itemIdx, tmpColumn;
            if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _wholeSize.Width)//向下
            {
                tmpColumn = (int)(Mathf.Abs(_tmpContentPos.x) - _contentSite) / _wholeSize.Width;
                _contentSite += _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = (_sProperty.dataIdx + _maxRanks.Height * tmpColumn) % _tmpTotalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + _maxRanks.Height * tmpColumn) % _sProperty.maxItems;
                ToItemAline(tmpItemIdx);
            }
            else if (Mathf.Abs(_tmpContentPos.x) < _contentSite - _sProperty.WidthExt)
            {
                tmpColumn = (int)Math.Ceiling((_contentSite - Mathf.Abs(_tmpContentPos.x)
                    - _sProperty.WidthExt) / _wholeSize.Width);
                _contentSite -= _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height * tmpColumn % _tmpTotalItems;
                _sProperty.dataIdx = _sProperty.dataIdx < 0 ? _tmpTotalItems + _sProperty.dataIdx : _sProperty.dataIdx;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height * tmpColumn % _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemAline(tmpItemIdx);
            }
            RefreshItems();
        }
        /// <summary>
        /// 自适应高宽循环式
        /// </summary>
        private void OnAnchorSet()
        {           
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;
            _tmpContentPos = _contentRect.anchoredPosition;
            Vector2 contentSize = _contentRect.sizeDelta;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    _cExtra.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width);         
                    _cExtra.area = _cExtra.length * _wholeSize.Height;
                    _tmpTotalItems = _cExtra.length * _maxRanks.Width;
                    contentSize.y = _cExtra.area - _sProperty.HeightExt;
                    contentSize.y += _lockSlide ? _sProperty.TopExt + _sProperty.BottomExt :
                        2 * (_viewRect.rect.height + _sProperty.HeightExt);
                    if (!_lockSlide)
                    {
                        _tmpContentPos.y = (_viewRect.rect.height + _sProperty.HeightExt) * _cExtra.dir;
                        _contentSite = (int)_tmpContentPos.y * _cExtra.dir;
                        _contentRect.anchoredPosition = _tmpContentPos;
                    }
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _cExtra.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height);
                    _cExtra.area = _cExtra.length * _wholeSize.Width;
                    _tmpTotalItems = _cExtra.length * _maxRanks.Height;
                    contentSize.x = _cExtra.area - _sProperty.WidthExt;
                    contentSize.x += _lockSlide ? _sProperty.LeftExt + _sProperty.RightExt :
                        2 * (_viewRect.rect.width + _sProperty.WidthExt);
                    if (!_lockSlide)
                    {
                        _tmpContentPos.x = (_viewRect.rect.width + _sProperty.WidthExt) * _cExtra.dir;
                        _contentSite = (int)_tmpContentPos.x * _cExtra.dir;
                        _contentRect.anchoredPosition = _tmpContentPos;
                    }
                    break;
            }              
            _contentRect.sizeDelta = contentSize;
        }
#endregion
#region//-------------------------------------普通滑动方式------------------------------------------//
        /// <summary>
        /// 垂直滑动
        /// </summary>
        private void OnCircleVerticalNo()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Clamp(_tmpContentPos.y *
                _cExtra.dir, 0,  _cExtra.area);
            while (_tmpContentPos.y  >  _contentSite + _wholeSize.Height)//向下
            {
                if (_sProperty.dataIdx + _sProperty.maxItems >= _dataSet.Count) break;
                OnRefreshItemDown();
            }
            while (_tmpContentPos.y < _contentSite - _sProperty.HeightExt)
            {
                if (_sProperty.dataIdx <= 0) break;
                OnRefreshItemUp();
            }
        }
        /// <summary>
        /// 水平滑动
        /// </summary>
        private void OnCircleHorizontalNo()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = Mathf.Clamp(_tmpContentPos.x * 
                _cExtra.dir,0, _cExtra.area);
            while (_tmpContentPos.x > _contentSite + _wholeSize.Width)
            {
                if (_sProperty.dataIdx + _sProperty.maxItems >= _dataSet.Count) break;
                OnRefreshItemDown();
            }
            while (_tmpContentPos.x < _contentSite - _sProperty.WidthExt)
            {
                if (_sProperty.dataIdx <= 0) break;
                OnRefreshItemUp();
            }
        }
        /// <summary>
        /// 垂直动画定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveVSeatNo(int toSeat)
        {           
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area);
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _wholeSize.Height)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _cExtra.dir < toSeat)
                {         
                    _tmpContentPos.y = _tmpContentPos.y + _cExtra.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _tmpContentPos.y = toSeat * _cExtra.dir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _wholeSize.Height &&
                    _sProperty.dataIdx + _sProperty.maxItems < _dataSet.Count)
                {
                    OnRefreshItemDown();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.HeightExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _cExtra.dir > toSeat)
                {            
                    _tmpContentPos.y =_tmpContentPos.y - _cExtra.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _tmpContentPos.y = toSeat * _cExtra.dir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (_tmpContentPos.y * _cExtra.dir < _contentSite - _sProperty.HeightExt && _sProperty.dataIdx > 0)
                {
                    OnRefreshItemUp();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.y = toSeat * _cExtra.dir;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
            _toLocationEvent?.Invoke();
        }
        /// <summary>
        /// 水平动画定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveHSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area);
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _wholeSize.Width)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _cExtra.dir < toSeat){
                    _tmpContentPos.x = _tmpContentPos.x + _cExtra.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else {
                    _tmpContentPos.x = toSeat * _cExtra.dir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }              

                if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _wholeSize.Width && 
                    _sProperty.dataIdx + _sProperty.maxItems < _dataSet.Count)
                {
                    OnRefreshItemDown();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _cExtra.dir > toSeat){
                    _tmpContentPos.x = _tmpContentPos.x - _cExtra.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _tmpContentPos.x = toSeat * _cExtra.dir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (_tmpContentPos.x * _cExtra.dir < _contentSite - _sProperty.WidthExt && _sProperty.dataIdx > 0)
                {
                    OnRefreshItemUp();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.x = toSeat * _cExtra.dir;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
            _toLocationEvent?.Invoke();
        }
        /// <summary>
        /// 强制垂直定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectVSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area); 
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = toSeat * _cExtra.dir;
            _contentRect.anchoredPosition = _tmpContentPos;

            int tmpItemIdx = _sProperty.itemIdx, tmpRow;
            if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _wholeSize.Height)//向下
            {
                tmpRow = Mathf.Min((int)(Mathf.Abs(_tmpContentPos.y) - _contentSite) / _wholeSize.Height
                    , _cExtra.length - (_sProperty.dataIdx + _sProperty.maxItems) / _maxRanks.Width);
                _contentSite += _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = (_sProperty.dataIdx + _maxRanks.Width * tmpRow) % _tmpTotalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + _maxRanks.Width * tmpRow) % _sProperty.maxItems;
                ToItemAline(tmpItemIdx);
            }
            else if (Mathf.Abs(_tmpContentPos.y) < _contentSite - _sProperty.HeightExt)
            {
                tmpRow = Mathf.Min((int)Math.Ceiling((_contentSite - Mathf.Abs(_tmpContentPos.y)
                    - _sProperty.HeightExt) / _wholeSize.Height), _sProperty.dataIdx / _maxRanks.Width);
                _contentSite -= _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width * tmpRow % _tmpTotalItems;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width * tmpRow % _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemAline(tmpItemIdx);
            }
            RefreshItems();
        }
        /// <summary>
        /// 强制水平定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectHSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area);
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = toSeat * _cExtra.dir;
            _contentRect.anchoredPosition = _tmpContentPos;

            int tmpItemIdx = _sProperty.itemIdx, tmpColumn;
            if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _wholeSize.Width)//向下
            {
                tmpColumn = Mathf.Min((int)(Mathf.Abs(_tmpContentPos.x) - _contentSite) / _wholeSize.Width,
                        _cExtra.length - (_sProperty.dataIdx + _sProperty.maxItems) / _maxRanks.Height);
                _contentSite += _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = (_sProperty.dataIdx + _maxRanks.Height * tmpColumn) % _tmpTotalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + _maxRanks.Height * tmpColumn) % _sProperty.maxItems;
                ToItemAline(tmpItemIdx);
            }
            else if (Mathf.Abs(_tmpContentPos.x) < _contentSite - _sProperty.WidthExt)
            {
                tmpColumn = Mathf.Min((int)Math.Ceiling((_contentSite - Mathf.Abs(_tmpContentPos.x)
                    - _sProperty.WidthExt) / _wholeSize.Width), _sProperty.dataIdx / _maxRanks.Height);
                _contentSite -= _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height * tmpColumn % _tmpTotalItems;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height * tmpColumn % _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemAline(tmpItemIdx);
            }
            RefreshItems();
        }  
        /// <summary>
        /// 自适应高宽
        /// </summary>
        private void OnAnchorSetNo()
        {
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;
            Vector2 _contentSize = _contentRect.sizeDelta;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    _cExtra.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width);              
                    _cExtra.area = _sProperty.TopExt + _sProperty.BottomExt + _cExtra.length 
                        * _wholeSize.Height - _sProperty.HeightExt - (int)_viewRect.rect.height;
                    _contentSize.y = _cExtra.area + (int)_viewRect.rect.height;
                    _tmpTotalItems = _cExtra.length * _maxRanks.Width;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _cExtra.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height);
                    _cExtra.area = _sProperty.LeftExt + _sProperty.RightExt + _cExtra.length 
                        * _wholeSize.Width - _sProperty.WidthExt - (int)_viewRect.rect.width;
                    _contentSize.x = _cExtra.area + (int)_viewRect.rect.width;
                    _tmpTotalItems = _cExtra.length * _maxRanks.Height;
                    break;
            }
            _tmpContentPos = _contentRect.anchoredPosition;
            _contentRect.sizeDelta = _contentSize;
        }
#endregion
    }
}