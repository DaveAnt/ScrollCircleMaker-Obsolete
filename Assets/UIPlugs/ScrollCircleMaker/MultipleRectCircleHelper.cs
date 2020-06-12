//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker       
{
    /// <summary>
    /// 多行规则长度滑动循环
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class MultipleRectCircleHelper<T> : BaseCircleHelper<T>
    {
        struct ContentExtra
        {
            public short dir;
            public int area;
            public int length;
        }

        private GridLayoutGroup _gridLayoutGroup;
        private ContentExtra _cExtra;
        private Vector2 _tmpContentPos;
        private SizeInt _wholeSize, _maxRanks;
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
        /// Content位置
        /// </summary>
        private int _contentPos
        {
            set
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                switch (_scrollRect.vertical)
                {
                    case true:
                        _tmpContentPos.y = value * _cExtra.dir;
                        _contentRect.anchoredPosition = _tmpContentPos;
                        break;
                    default:
                        _tmpContentPos.x = value * _cExtra.dir;
                        _contentRect.anchoredPosition = _tmpContentPos;
                        break;
                }
            }
        }
        /// <summary>
        /// 边界值
        /// </summary>
        private int _contentPadding
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        if (_sProperty.isCircleEnable && !_lockSlide)
                            return (int)_viewRect.rect.height + _sProperty.HeightExt;
                        return _sProperty.TopExt;
                    case ScrollDir.BottomToTop:
                        if (_sProperty.isCircleEnable && !_lockSlide)
                            return (int)_viewRect.rect.height + _sProperty.HeightExt;
                        return _sProperty.BottomExt;
                    case ScrollDir.LeftToRight:
                        if (_sProperty.isCircleEnable && !_lockSlide)
                            return (int)_viewRect.rect.width + _sProperty.WidthExt;
                        return _sProperty.LeftExt;
                    default:
                        if (_sProperty.isCircleEnable && !_lockSlide)
                            return (int)_viewRect.rect.width + _sProperty.WidthExt;
                        return _sProperty.RightExt;
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
                switch (_scrollRect.vertical)
                {
                    case true:
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
            switch (_scrollRect.vertical)
            {
                case true:
                    _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                    float tmpWidth = _contentRect.rect.width - _sProperty.LeftExt - _sProperty.RightExt;
                    _maxRanks.Width = (int)((tmpWidth + _sProperty.WidthExt) / _wholeSize.Width);
                    _maxRanks.Height = (int)(Math.Ceiling(_viewRect.rect.height / _wholeSize.Height) + 1);
                    _sProperty.maxItems = _maxRanks.Width * _maxRanks.Height;
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                default:
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
        /// 解析GridLayoutGroup排版
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

        protected override void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            if (_lockSlide) return;
            if (_timer < _sProperty.refreshRatio){
                _timer += Time.deltaTime;
                return;
            }

            switch (_scrollRect.vertical)
            {
                case true:
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
                default:
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

            _firstRun = true;
            _scrollRect.inertia = _sProperty.scrollType != ScrollType.Drag;
            
        }

        public override void DelItem(int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= _dataSet.Count)
            {
                Debug.LogError("DelItem超范围！");
                return;
            }
                
            _dataSet.RemoveAt(itemIdx);
            if (_firstRun)  ToAutoSite(false);
        }

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

            if (_firstRun && seekSwitch) ToAutoSite(false);
        }
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
            if(_firstRun) ToAutoSite(true);
        }
        public override void UpdateItem(T data, int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= _dataSet.Count)
            {
                Debug.LogError("UpdateItem超范围！");
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
        public override void ResetItems()
        {
            foreach (var baseItem in _itemSet)
                baseItem.OnDestroy();
            Vector2 contentSize = _scrollRect.vertical ? 
                new Vector2(_contentRect.sizeDelta.x, 0):
                new Vector2(0, _contentRect.sizeDelta.y);
            _contentRect.sizeDelta = contentSize;
            _contentSite = _contentPadding;
            _contentPos = 0;
            _dataSet.Clear();
            _itemSet.Clear();
        }
        public override int GetLocation()
        {
            switch (_scrollRect.vertical)
            {
                case true:
                    return (int)Mathf.Abs(_contentRect.rect.y);
                default:
                    return (int)Mathf.Abs(_contentRect.rect.x);
            }
        }
        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {
            switch (_scrollRect.vertical)
            {
                case true:
                    if (_sProperty.isCircleEnable&& isDrawEnable)
                        _sProperty.StartCoroutine(ToAutoMoveVSeat(toSeat));
                    else if(_sProperty.isCircleEnable)
                        ToDirectVSeat(toSeat);
                    else if (isDrawEnable)
                        _sProperty.StartCoroutine(ToAutoMoveVSeatNo(toSeat));
                    else
                        ToDirectVSeatNo(toSeat);
                    break;
                default:
                    if (_sProperty.isCircleEnable && isDrawEnable)
                        _sProperty.StartCoroutine(ToAutoMoveHSeat(toSeat));
                    else if (_sProperty.isCircleEnable)
                        ToDirectHSeat(toSeat);
                    else if (isDrawEnable)
                        _sProperty.StartCoroutine(ToAutoMoveHSeatNo(toSeat));
                    else
                        ToDirectHSeatNo(toSeat);
                    break;
            }
        }
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
        public override void ToBottom(bool isDrawEnable = true)
        {
            ToLocation(_cExtra.area, isDrawEnable);
        }
        /// <summary>
        /// 初始化item
        /// </summary>
        /// <param name="itemIdx">位置</param>
        private void InitItem(int itemIdx)
        {
            BaseItem<T> baseItem = _createItemFunc();
            baseItem.SetTransform(UnityEngine.Object.Instantiate(_baseItem, _contentRect).transform);
            baseItem.InitComponents();
            baseItem.InitEvents();
            baseItem.UpdateView(_dataSet[itemIdx]);
            baseItem.gameObject.name = _baseItem.name + itemIdx;
            _itemSet.Add(baseItem);
        }
        /// <summary>
        /// 自适应改变
        /// </summary>
        /// <param name="state">true：添加，false移除</param>
        private void ToAutoSite(bool state)
        {
            bool lockSilde = _lockSlide;
            int maxSize = _scrollRect.vertical ? _maxRanks.Width : _maxRanks.Height;
            if (state && _itemSet.Count < _sProperty.maxItems)
                InitItem(_itemSet.Count);
            else if (!state && _dataSet.Count < _itemSet.Count)
            {
                GameObject.Destroy(_itemSet[_itemSet.Count - 1].gameObject);
                _itemSet.RemoveAt(_itemSet.Count - 1);
            }
            _sProperty.initItems = _itemSet.Count;
            if (_sProperty.isCircleEnable)
                OnAnchorSet();
            else
                OnAnchorSetNo();

            if (_sProperty.isCircleEnable)
            {
                if (_lockSlide && !lockSilde)//锁定
                {
                    ToItemOffset(0);
                    _sProperty.dataIdx = 0;
                    _sProperty.itemIdx = 0;
                    _contentSite = _contentPadding;
                    _contentPos = 0;                       
                }
                else if (!_lockSlide && lockSilde)//变成循环
                {
                    ToItemOffset(0);
                    _sProperty.dataIdx = 0;
                    _sProperty.itemIdx = 0;
                    _contentSite = _contentPadding;
                    _contentPos = _contentPadding;
                }
            }
            else if (_sProperty.dataIdx + _itemSet.Count >= _dataSet.Count + maxSize)
                OnRefreshItemUp();
            RefreshItems();
        }
        /// <summary>
        /// 对齐偏移
        /// </summary>
        /// <param name="tmpItemIdx"></param>
        private void ToItemOffset(int tmpItemIdx)
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
        /// 垂直循环式滑动
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
                _contentSite = _contentPos = _contentPadding;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemOffset(tmpItemIdx);
                RefreshItems();              
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
            else if (_lowerDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _tmpTotalItems - _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.maxItems;
                _contentSite = _contentPadding + _sProperty.dataIdx * _wholeSize.Height / _maxRanks.Width;
                _contentPos = _cExtra.area;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemOffset(tmpItemIdx);
                RefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
        }
        /// <summary>
        /// 水平循环式滑动
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
                _contentSite = _contentPos = _contentPadding;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemOffset(tmpItemIdx);
                RefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
            else if (_lowerDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _tmpTotalItems - _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.maxItems;
                _contentSite = _contentPadding + _sProperty.dataIdx * _wholeSize.Width / _maxRanks.Height;
                _contentPos = _cExtra.area;           
                _gridLayoutGroup.SetLayoutVertical();
                ToItemOffset(tmpItemIdx);
                RefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
        }
        /// <summary>
        /// 垂直动画过程定位
        /// </summary>
        /// <param name="toSeat">位置</param>
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
        /// 水平动画过程定位
        /// </summary>
        /// <param name="toSeat">位置</param>
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
        /// 无过程垂直定位
        /// </summary>
        /// <param name="toSeat">位置</param>
        private void ToDirectVSeat(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, (int)(_contentRect.rect.height - _viewRect.rect.height));
            _contentPos = toSeat;

            int tmpItemIdx = _sProperty.itemIdx, tmpRow;
            if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _wholeSize.Height)//向下
            {
                tmpRow = (int)(Mathf.Abs(_tmpContentPos.y) - _contentSite) / _wholeSize.Height;
                _contentSite += _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = (_sProperty.dataIdx + _maxRanks.Width * tmpRow) % _tmpTotalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + _maxRanks.Width * tmpRow) % _sProperty.maxItems;
                ToItemOffset(tmpItemIdx);
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
                ToItemOffset(tmpItemIdx);
            }
            RefreshItems();          
        }
        /// <summary>
        /// 无过程水平定位
        /// </summary>
        /// <param name="toSeat">位置</param>
        private void ToDirectHSeat(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, (int)(_contentRect.rect.width - _viewRect.rect.width));
            _contentPos = toSeat;

            int tmpItemIdx = _sProperty.itemIdx, tmpColumn;
            if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _wholeSize.Width)//向下
            {
                tmpColumn = (int)(Mathf.Abs(_tmpContentPos.x) - _contentSite) / _wholeSize.Width;
                _contentSite += _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = (_sProperty.dataIdx + _maxRanks.Height * tmpColumn) % _tmpTotalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + _maxRanks.Height * tmpColumn) % _sProperty.maxItems;
                ToItemOffset(tmpItemIdx);
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
                ToItemOffset(tmpItemIdx);
            }
            RefreshItems();
        }
        /// <summary>
        /// 自适应高宽
        /// </summary>
        private void OnAnchorSet()
        {
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;
            Vector2 contentSize = _contentRect.sizeDelta;
            switch (_scrollRect.vertical)
            {
                case true:
                    _cExtra.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width);         
                    _cExtra.area = _cExtra.length * _wholeSize.Height;
                    _tmpTotalItems = _cExtra.length * _maxRanks.Width;
                    contentSize.y = _cExtra.area - _sProperty.HeightExt;
                    contentSize.y += _lockSlide ? _sProperty.TopExt + _sProperty.BottomExt :
                        2 * (_viewRect.rect.height + _sProperty.HeightExt);
                    break;
                default:
                    _cExtra.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height);
                    _cExtra.area = _cExtra.length * _wholeSize.Width;
                    _tmpTotalItems = _cExtra.length * _maxRanks.Height;
                    contentSize.x = _cExtra.area - _sProperty.WidthExt;
                    contentSize.x += _lockSlide ? _sProperty.LeftExt + _sProperty.RightExt :
                        2 * (_viewRect.rect.width + _sProperty.WidthExt);
                    break;
            }
            if (!_lockSlide && !_firstRun) _contentSite = _contentPos = _contentPadding;
            _tmpContentPos = _contentRect.anchoredPosition;
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
        /// 垂直过程定位
        /// </summary>
        /// <param name="toSeat">位置</param>
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
        /// 水平过程定位
        /// </summary>
        /// <param name="toSeat">位置</param>
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
        /// <param name="toSeat">位置</param>
        private void ToDirectVSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area);
            _contentPos = toSeat;

            int tmpItemIdx = _sProperty.itemIdx, tmpRow;
            if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _wholeSize.Height)//向下
            {
                tmpRow = Mathf.Min((int)(Mathf.Abs(_tmpContentPos.y) - _contentSite) / _wholeSize.Height
                    , _cExtra.length - (_sProperty.dataIdx + _sProperty.maxItems) / _maxRanks.Width);
                _contentSite += _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = (_sProperty.dataIdx + _maxRanks.Width * tmpRow) % _tmpTotalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + _maxRanks.Width * tmpRow) % _sProperty.maxItems;
                ToItemOffset(tmpItemIdx);
            }
            else if (Mathf.Abs(_tmpContentPos.y) < _contentSite - _sProperty.HeightExt)
            {
                tmpRow = Mathf.Min((int)Math.Ceiling((_contentSite - Mathf.Abs(_tmpContentPos.y)
                    - _sProperty.HeightExt) / _wholeSize.Height), _sProperty.dataIdx / _maxRanks.Width);
                _contentSite -= _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width * tmpRow % _tmpTotalItems;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width * tmpRow % _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemOffset(tmpItemIdx);
            }
            RefreshItems();
        }
        /// <summary>
        /// 强制水平定位
        /// </summary>
        /// <param name="toSeat">位置</param>
        private void ToDirectHSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area);
            _contentPos = toSeat;

            int tmpItemIdx = _sProperty.itemIdx, tmpColumn;
            if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _wholeSize.Width)//向下
            {
                tmpColumn = Mathf.Min((int)(Mathf.Abs(_tmpContentPos.x) - _contentSite) / _wholeSize.Width,
                  _cExtra.length - (_sProperty.dataIdx + _sProperty.maxItems) / _maxRanks.Height);
                _contentSite += _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = (_sProperty.dataIdx + _maxRanks.Height * tmpColumn) % _tmpTotalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + _maxRanks.Height * tmpColumn) % _sProperty.maxItems;
                ToItemOffset(tmpItemIdx);
            }
            else if (Mathf.Abs(_tmpContentPos.x) < _contentSite - _sProperty.WidthExt)
            {
                tmpColumn = Mathf.Min((int)Math.Ceiling((_contentSite - Mathf.Abs(_tmpContentPos.x)
                  - _sProperty.WidthExt) / _wholeSize.Width), _sProperty.dataIdx / _maxRanks.Height);
                _contentSite -= _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height * tmpColumn % _tmpTotalItems;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height * tmpColumn % _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemOffset(tmpItemIdx);
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
            Vector2 contentSize = _contentRect.sizeDelta;
            switch (_scrollRect.vertical)
            {
                case true:
                    _cExtra.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width);              
                    _cExtra.area = (int)(_sProperty.TopExt + _sProperty.BottomExt + _cExtra.length 
                        * _wholeSize.Height - _sProperty.HeightExt - _viewRect.rect.height);
                    contentSize.y = _cExtra.area + _viewRect.rect.height;
                    _tmpTotalItems = _cExtra.length * _maxRanks.Width;
                    break;
                default:
                    _cExtra.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height);
                    _cExtra.area = (int)(_sProperty.LeftExt + _sProperty.RightExt + _cExtra.length 
                        * _wholeSize.Width - _sProperty.WidthExt - _viewRect.rect.width);
                    contentSize.x = _cExtra.area + _viewRect.rect.width;
                    _tmpTotalItems = _cExtra.length * _maxRanks.Height;
                    break;
            }
            _tmpContentPos = _contentRect.anchoredPosition;
            _contentRect.sizeDelta = contentSize;
        }
#endregion
    }
}