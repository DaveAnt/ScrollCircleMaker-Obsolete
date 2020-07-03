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
    /// 单行不规则长度滑动循环
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SingleRectCircleHelper<T> : BaseCircleHelper<T>
    {
        private HorizontalOrVerticalLayoutGroup _singleLayoutGroup;
        private BaseItem<T> _workItem;
        private Vector2 _tmpContentPos;
        private SizeInt _wholeSize;
        private bool _lockSlide, _firstRun;
        public short _slideDir = 1;
        private float _timer = 0;
        /// <summary>
        /// 锚点
        /// </summary>
        private int _contentSite
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        return _singleLayoutGroup.padding.top;
                    case ScrollDir.BottomToTop:
                        return _singleLayoutGroup.padding.bottom;
                    case ScrollDir.LeftToRight:
                        return _singleLayoutGroup.padding.left;
                    default:
                        return _singleLayoutGroup.padding.right;
                }
            }
            set
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        _singleLayoutGroup.padding.top = value;
                        break;
                    case ScrollDir.BottomToTop:
                        _singleLayoutGroup.padding.bottom = value;
                        break;
                    case ScrollDir.LeftToRight:
                        _singleLayoutGroup.padding.left = value;
                        break;
                    default:
                        _singleLayoutGroup.padding.right = value;
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
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        _tmpContentPos.y = value;
                        break;
                    case ScrollDir.BottomToTop:
                        _tmpContentPos.y = -value;
                        break;
                    case ScrollDir.RightToLeft:
                        _tmpContentPos.x = value;
                        break;
                    default:
                        _tmpContentPos.x = -value;
                        break;
                }
                _contentRect.anchoredPosition = _tmpContentPos;
            }
        }
        /// <summary>
        /// 边界距离值
        /// </summary>
        private int _contentPadding
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        if (_sProperty.isCircleEnable && !_lockSlide)
                            return (int)_viewRect.rect.height + _sProperty.WidthExt;
                        return _sProperty.TopExt;
                    case ScrollDir.BottomToTop:
                        if (_sProperty.isCircleEnable && !_lockSlide)
                            return (int)_viewRect.rect.height + _sProperty.WidthExt;
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
        /// 数据底部所在位置
        /// </summary>
        private int _maxExtent
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        if (!_lockSlide && _sProperty.isCircleEnable)
                            return (int)(_contentRect.rect.height - ContentBorder(true) + _sProperty.WidthExt);
                        return (int)(_contentRect.rect.height - _viewRect.rect.height);
                    default:
                        if (!_lockSlide && _sProperty.isCircleEnable)
                            return (int)(_contentRect.rect.width - ContentBorder(true) + _sProperty.WidthExt);
                        return (int)(_contentRect.rect.width - _viewRect.rect.width);
                }
            }
        }
        /// <summary>
        /// 首个Item实例高宽
        /// </summary>
        private RectTransform _itemFirstSize
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        return _contentRect.GetChild(0).transform as RectTransform;
                    case ScrollDir.BottomToTop:
                        return _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                    case ScrollDir.RightToLeft:
                        return _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                    default:
                        return _contentRect.GetChild(0).transform as RectTransform;
                }
            }
        }
        /// <summary>
        /// 不规则单行滑动构造函数
        /// </summary>
        /// <param name="contentTrans">content的transform组件</param>
        /// <param name="createItemFunc">创建item函数</param>
        public SingleRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc)
        {
            _createItemFunc = createItemFunc;
            _contentRect = contentTrans as RectTransform;
            _viewRect = contentTrans.parent.GetComponent<RectTransform>();
            _scrollRect = _viewRect.parent.GetComponent<ScrollRect>();
            _sProperty = _contentRect.GetComponent<ScrollCircleComponent>();
            if (_sProperty == null) throw new Exception("Content must have ScrollCircleComponent!");
            _baseItem = _sProperty.baseItem;
            _itemRect = _baseItem.transform.GetComponent<RectTransform>();
            _scrollRect.onValueChanged.AddListener(OnRefreshHandler);
            _itemSet = new List<BaseItem<T>>();
            _dataSet = new List<T>();
            
            Transform workItem = GameObject.Instantiate(_baseItem.transform,_scrollRect.transform);
            workItem.name = "workItem";
            workItem.gameObject.SetActive(false);
            workItem.localScale = Vector3.zero;
            workItem.localPosition = Vector3.one * int.MaxValue;
            _workItem = _createItemFunc();
            _workItem.InitComponents(workItem);
            OnInit();
        }

        private void OnInit()
        {
            _wholeSize.Width = (ushort)(_itemRect.rect.width + _sProperty.WidthExt);
            _wholeSize.Height = (ushort)(_itemRect.rect.height + _sProperty.WidthExt);
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    _singleLayoutGroup = _contentRect.GetComponent<VerticalLayoutGroup>() ??
                         _contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
                    _sProperty.maxItems = (int)Math.Ceiling(_viewRect.rect.height / _wholeSize.Height) + 1;
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _singleLayoutGroup = _contentRect.GetComponent<HorizontalLayoutGroup>() ??
                        _contentRect.gameObject.AddComponent<HorizontalLayoutGroup>();
                    _sProperty.maxItems = (int)Math.Ceiling(_viewRect.rect.width / _wholeSize.Width) + 1;
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    break;
            }
            if (_sProperty.maxItems <= 0) Debug.LogError("自动计算后，最大实例数为空！");

            _singleLayoutGroup.childControlHeight = false;
            _singleLayoutGroup.childControlWidth = false;
            _singleLayoutGroup.childForceExpandHeight = false;
            _singleLayoutGroup.childForceExpandWidth = false;
            _singleLayoutGroup.spacing = _sProperty.WidthExt;
            _singleLayoutGroup.padding.top = _sProperty.TopExt;
            _singleLayoutGroup.padding.bottom = _sProperty.BottomExt;
            _singleLayoutGroup.padding.right = _sProperty.RightExt;
            _singleLayoutGroup.padding.left = _sProperty.LeftExt;
            OnResolveGroupEnum();
        }

        private void OnResolveGroupEnum()
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    _singleLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    break;
                case ScrollDir.BottomToTop:
                    _slideDir = -1;
                    _singleLayoutGroup.childAlignment = TextAnchor.LowerLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    break;
                case ScrollDir.LeftToRight:
                    _slideDir = -1;
                    _singleLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    break;
                case ScrollDir.RightToLeft:
                    _singleLayoutGroup.childAlignment = TextAnchor.UpperRight;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    break;
            }
        }

        protected override void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            if (_lockSlide) return;
            if (_timer < _sProperty.refreshRatio)
            {
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
                        _tmpContentPos.y + _wholeSize.Height * _sProperty.limitNum * _slideDir :
                        _tmpContentPos.y - _wholeSize.Height * _sProperty.limitNum * _slideDir;
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
                        _tmpContentPos.x + _wholeSize.Width * _sProperty.limitNum * _slideDir:
                        _tmpContentPos.x - _wholeSize.Width * _sProperty.limitNum * _slideDir;
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

        public void InitItem(int itemIdx)
        {
            BaseItem<T> baseItem = _createItemFunc();
            baseItem.InitComponents(GameObject.Instantiate(_baseItem, _contentRect).transform);
            baseItem.InitEvents();
            baseItem.UpdateView(_dataSet[itemIdx],itemIdx);
            baseItem.gameObject.name = _baseItem.name + itemIdx;
            _itemSet.Add(baseItem);
        }

        public override void OnStart(List<T> _tmpDataSet = null)
        {
            if (_tmpDataSet != null)
            {
                if (_sProperty.scrollSort == ScrollSort.BackDir ||
                    _sProperty.scrollSort == ScrollSort.BackZDir)
                    _tmpDataSet.Reverse();
                _dataSet.AddRange(_tmpDataSet);
            }
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                if (i >= _dataSet.Count) break;//表示没有数据
                InitItem(i);
            }
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.BottomToTop:
                case ScrollDir.RightToLeft:
                    for (int i = _itemSet.Count - 2; i >= 0; --i)
                        _itemSet[i].transform.SetAsLastSibling();
                    break;
            }
            OnAnchorSet();
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
            ToAdaptionContent(default, itemIdx, 0);
        }

        public override void DelItem(Func<T, T, bool> seekFunc, T data)
        {
            for (int i = _dataSet.Count - 1; i >= 0; ++i)
            {
                if (seekFunc(data, _dataSet[i]))
                {
                    _dataSet.RemoveAt(i);
                    ToAdaptionContent(default, i, 0);
                    break;
                }
            }
        }
        public override void AddItem(T data, int itemIdx = 0)
        {
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count);
            switch (_sProperty.scrollSort)
            {
                case ScrollSort.FrontDir:
                case ScrollSort.FrontZDir:
                    itemIdx = _dataSet.Count - itemIdx;
                    break;
            }
            _dataSet.Insert(itemIdx, data);
            ToAdaptionContent(data, itemIdx, 1);
        }
        public override void UpdateItem(T data, int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= _dataSet.Count)
            {
                Debug.LogError("UpdateItem超范围！");
                return;
            }
            ToAdaptionContent(data, itemIdx, 2);
            _dataSet[itemIdx] = data;
            int tmpOffset = _sProperty.dataIdx > itemIdx ? _dataSet.Count -
                    _sProperty.dataIdx + itemIdx : itemIdx - _sProperty.dataIdx;
            if (tmpOffset < _sProperty.maxItems)
            {
                int tmpItemIdx = (_sProperty.itemIdx + tmpOffset) % _sProperty.maxItems;
                if (tmpItemIdx < _itemSet.Count)
                    _itemSet[tmpItemIdx].UpdateView(data);
            }

        }
        public override void ResetItems()
        {
            _firstRun = false;
            foreach (var baseItem in _itemSet)
                baseItem.OnDestroy();
            _contentSite = _contentPadding;
            _contentSize = 0;
            _contentPos = 0;
            _dataSet.Clear();
            _itemSet.Clear();
        }
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
        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {
            switch (_scrollRect.vertical)
            {
                case true:
                    if (_sProperty.isCircleEnable && isDrawEnable)
                        _sProperty.StartCoroutine(ToAutoMoveVSeat(toSeat));
                    else if (_sProperty.isCircleEnable)
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
        /// <summary>
        /// 上定位
        /// </summary>
        /// <param name="isDrawEnable">是否需要动画</param>
        public override void ToTop(bool isDrawEnable = true)
        {
           ToLocation(_contentPadding, isDrawEnable);
        }
        /// <summary>
        /// 下定位
        /// </summary>
        /// <param name="isDrawEnable">是否需要动画</param>
        public override void ToBottom(bool isDrawEnable = true)
        {
            ToLocation(_maxExtent, isDrawEnable);
        }

        #region//---------------------------内置函数-------------------------------//
        /// <summary>
        /// 计算删除0、添加1、更新2导致的位移
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="globalSeat">位置</param>
        /// <param name="signHandle">标志位</param>
        /// <returns></returns>
        private void ToAdaptionContent(T data,int globalSeat,byte signHandle)
        {
            if (!_firstRun || globalSeat == -1) return;
            bool toTurn = false;
            switch (signHandle)
            {
                case 0://删除
                    _workItem.UpdateView(_dataSet[globalSeat], globalSeat);
                    float delSize = _sProperty.WidthExt + (_scrollRect.vertical ?
                        _workItem.rectTrans.rect.height:_workItem.rectTrans.rect.width );
                    if (_dataSet.Count < _itemSet.Count)
                    {
                        GameObject.Destroy(_itemSet[_itemSet.Count - 1].gameObject);
                        _itemSet.RemoveAt(_itemSet.Count - 1);
                    }
                    if (!_lockSlide && _sProperty.maxItems >= _dataSet.Count)
                    {
                        toTurn = _sProperty.isCircleEnable;
                        _lockSlide = true;
                    }

                    if (toTurn)
                    {
                        _contentSize = _contentSize + ContentBorder(false) - ContentBorder(true) - delSize;
                        _contentSite = (int)(_contentSite + _contentPadding - ContentBorder(true)/2);
                        if (_sProperty.dataIdx > globalSeat)
                        {
                            _contentSite -= (int)delSize;
                            OnRefreshItemUp();
                        } 
                        if (_contentSite < _contentPadding)
                            _contentSite = _contentPadding;         
                    }
                    else
                    {
                        _contentSize -= delSize;
                        if (_sProperty.dataIdx > globalSeat)
                        {
                            _contentSite -= (int)delSize;
                            OnRefreshItemUp();
                        }         
                    }
                    RefreshItems();
                    break;
                case 1://添加
                    _workItem.UpdateView(data, globalSeat);
                    float addSize = _sProperty.WidthExt + (_scrollRect.vertical ? 
                        _workItem.rectTrans.rect.height:_workItem.rectTrans.rect.width);
                    if (_itemSet.Count < _sProperty.maxItems)
                        InitItem(_itemSet.Count);
                    if (_lockSlide == true && _sProperty.maxItems < _dataSet.Count)
                    {
                        toTurn = _sProperty.isCircleEnable;
                        _lockSlide = false;
                    }

                    _contentSize += addSize;
                    if (toTurn){
                        int tmpPadding = (int)ContentBorder(true) / 2;
                        _contentSize = _contentSize + ContentBorder(true) - ContentBorder(false);
                        _contentSite = (int)(_contentSite + tmpPadding - _contentPadding);
                    }
                    RefreshItems();
                    break;
                case 2://更新
                    _workItem.UpdateView(_dataSet[globalSeat], globalSeat);
                    float oldSize = _scrollRect.vertical ? _workItem.rectTrans.rect.height
                        : _workItem.rectTrans.rect.width;
                    _workItem.UpdateView(data, globalSeat);
                    float newSize = _scrollRect.vertical ? _workItem.rectTrans.rect.height
                        : _workItem.rectTrans.rect.width;
                    float updateSize = newSize - oldSize;
                    _contentSize += updateSize;
                    if (_sProperty.dataIdx > globalSeat)
                        _contentSite += (int)updateSize;
                    break;
                default:
                    Debug.LogError("ToAdaptionContent signHandle error:" + signHandle);
                    break;
            }
        }
        /// <summary>
        /// 扩展边距
        /// </summary>
        private float ContentBorder(bool isCircleEnable)
        {
            switch (_scrollRect.vertical)
            {
                case true:
                    if (isCircleEnable)
                        return 2 * (_viewRect.rect.height + _sProperty.WidthExt);
                    return _sProperty.TopExt + _sProperty.BottomExt;
                default:
                    if(isCircleEnable)
                        return 2 * (_viewRect.rect.width + _sProperty.WidthExt);
                    return _sProperty.LeftExt + _sProperty.RightExt;
            }
        }
        /// <summary>
        /// 对齐偏移
        /// </summary>
        /// <param name="tmpItemIdx">对齐位置</param>
        private void ToItemOffset(int tmpItemIdx)
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.LeftToRight:
                    for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                        _itemSet[i].transform.SetAsLastSibling();
                    for (int i = tmpItemIdx - 1; i >= _sProperty.itemIdx; --i)
                        _itemSet[i].transform.SetAsFirstSibling();
                    break;
                default:
                    for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                        _itemSet[i].transform.SetAsFirstSibling();
                    for (int i = tmpItemIdx - 1; i >= _sProperty.itemIdx; --i)
                        _itemSet[i].transform.SetAsLastSibling();
                    break;
            }
        }
        /// <summary>
        /// Item循环位置自适应
        /// </summary>
        private void ToItemCircle()
        {
            int tmpItemIdx; Vector2 tmpForce;
            if (_highDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.itemIdx = _sProperty.dataIdx = 0;
                _contentSite = _contentPos = _contentPadding;
                _singleLayoutGroup.SetLayoutVertical();
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
                _sProperty.dataIdx = _dataSet.Count - _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.maxItems;
                _contentSite = _contentPadding + _maxExtent;
                for (int i = _dataSet.Count - 1; i >= _dataSet.Count - _sProperty.maxItems; --i)
                {
                    _workItem.UpdateView(_dataSet[i], i);//模拟变化
                    _contentSite -= _sProperty.WidthExt + (_scrollRect.vertical ?
                         (int)(_workItem.rectTrans.rect.height):
                         (int)(_workItem.rectTrans.rect.width));
                }
                _contentPos = _maxExtent;
                _singleLayoutGroup.SetLayoutVertical();
                ToItemOffset(tmpItemIdx);
                RefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
        }
        /// <summary>
        /// 刷新Items
        /// </summary>
        private void RefreshItems()
        {
            int tmpItemIdx, tmpDataIdx;
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                if (i > _itemSet.Count - 1) return;
                tmpDataIdx = (_sProperty.dataIdx + i) % _dataSet.Count;
                tmpItemIdx = (_sProperty.itemIdx + i) % _sProperty.maxItems;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx],tmpDataIdx);
            }
        }
        /// <summary>
        /// 设置排序
        /// </summary>
        /// <param name="tmpItemIdx">设置索引</param>
        /// <param name="tmpSlideDir">滑动方向</param>
        private void SetItemSibling(int tmpItemIdx,bool tmpSlideDir)
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.LeftToRight:
                    if(tmpSlideDir)
                        _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                    else
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                    break;
                default:
                    if (tmpSlideDir)
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                    else
                        _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                    break;
            }
        }
        /// <summary>
        /// 下滑动时刷新接口
        /// </summary>
        private void OnRefreshItemDown()
        {
            int tmpDataIdx = (_sProperty.dataIdx + _sProperty.maxItems) % _dataSet.Count;
            _contentSite += _scrollRect.vertical ? (int)_itemFirstSize.rect.height
                + _sProperty.WidthExt: (int)_itemFirstSize.rect.width + _sProperty.WidthExt;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + (tmpDataIdx);
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
            SetItemSibling(_sProperty.itemIdx,true);
            _sProperty.itemIdx = (_sProperty.itemIdx + 1) % _sProperty.maxItems;
            _sProperty.dataIdx = (_sProperty.dataIdx + 1) % _dataSet.Count;
        }
        /// <summary>
        /// 上滑动时刷新接口
        /// </summary>
        private void OnRefreshItemUp()
        {         
            _sProperty.itemIdx = _sProperty.itemIdx - 1 < 0 ? _sProperty.maxItems - 1 : _sProperty.itemIdx - 1;
            _sProperty.dataIdx = _sProperty.dataIdx - 1 < 0 ? _dataSet.Count - 1 : _sProperty.dataIdx - 1;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + _sProperty.dataIdx;
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[_sProperty.dataIdx],_sProperty.dataIdx);
            SetItemSibling(_sProperty.itemIdx,false);
            _contentSite -= _scrollRect.vertical ? (int)_itemFirstSize.rect.height
                + _sProperty.WidthExt : (int)_itemFirstSize.rect.width + _sProperty.WidthExt;
        }
        /// <summary>
        /// 自适应高度
        /// </summary>
        private void OnAnchorSet()
        {
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;

            float tmpSize = 0;
            bool isCirCleEnable = !_lockSlide && _sProperty.isCircleEnable;
            switch (_scrollRect.vertical)
            {
                case true:
                    for (int i = 0; i < _dataSet.Count; ++i)
                    {
                        _workItem.UpdateView(_dataSet[i], i);//模拟变化
                        tmpSize += _workItem.rectTrans.rect.height + _sProperty.WidthExt;
                    }
                    break;
                default:
                    for (int i = 0; i < _dataSet.Count; ++i)
                    {
                        _workItem.UpdateView(_dataSet[i], i);//模拟变化
                        tmpSize += _workItem.rectTrans.rect.width + _sProperty.WidthExt;
                    }
                    break;
            }
            if (isCirCleEnable) _contentSite = _contentPos = _contentPadding;
            tmpSize = tmpSize + ContentBorder(isCirCleEnable) - _sProperty.WidthExt;
            _tmpContentPos = _contentRect.anchoredPosition;
            _contentSize = tmpSize;
        }
        #endregion
        #region//---------------------------循环滑动方式-------------------------------//
        private void OnCircleVertical()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y);
            while (_tmpContentPos.y > _contentSite + _itemFirstSize.rect.height + _sProperty.WidthExt)//向下
                OnRefreshItemDown();
            while (_tmpContentPos.y < _contentSite - _sProperty.WidthExt)
                OnRefreshItemUp();
            ToItemCircle();
        }

        private void OnCircleHorizontal()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = Mathf.Abs(_tmpContentPos.x);
            while (_tmpContentPos.x > _contentSite + _itemFirstSize.rect.width + _sProperty.WidthExt)
                OnRefreshItemDown();
            while (_tmpContentPos.x < _contentSite - _sProperty.WidthExt)
                OnRefreshItemUp();
            ToItemCircle();
        }

        /// <summary>
        /// 垂直动画定位
        /// </summary>
        /// <param name="toSeat">位置</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveVSeat(int toSeat)
        {
            int tmpToSeat = toSeat;
            toSeat = Mathf.Clamp(toSeat, 0, _maxExtent);
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _itemFirstSize.rect.height + _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _slideDir < toSeat)
                {
                    _tmpContentPos.y = _tmpContentPos.y + _slideDir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.y = toSeat * _slideDir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _itemFirstSize.rect.height + _sProperty.WidthExt)
                {
                    OnRefreshItemDown();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _slideDir > toSeat)
                {
                    _tmpContentPos.y = _tmpContentPos.y - _slideDir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.y = toSeat * _slideDir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (_tmpContentPos.y * _slideDir < _contentSite - _sProperty.WidthExt)
                {
                    OnRefreshItemUp();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.y = toSeat * _slideDir;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
            _toLocationEvent?.Invoke();

            if (_sProperty.scrollType == ScrollType.Limit)
            {
                OnCircleVertical();
                _tmpContentPos = _contentRect.anchoredPosition;
                if (tmpToSeat < 0)
                {
                    _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y) + tmpToSeat;
                    ToLocation((int)_tmpContentPos.y);
                }
                else if (tmpToSeat != toSeat)
                {
                    _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y) + tmpToSeat - toSeat;
                    ToLocation((int)_tmpContentPos.y);
                }
            }
        }
        /// <summary>
        /// 水平动画定位
        /// </summary>
        /// <param name="toSeat">位置</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveHSeat(int toSeat)
        {
            int tmpToSeat = toSeat;
            toSeat = Mathf.Clamp(toSeat, 0, _maxExtent);
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _itemFirstSize.rect.width + _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _slideDir < toSeat)
                {
                    _tmpContentPos.x = _tmpContentPos.x + _slideDir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.x = toSeat * _slideDir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _itemFirstSize.rect.width + _sProperty.WidthExt)
                {
                    OnRefreshItemDown();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _slideDir > toSeat)
                {
                    _tmpContentPos.x = _tmpContentPos.x - _slideDir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.x = toSeat * _slideDir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (_tmpContentPos.x * _slideDir < _contentSite - _sProperty.WidthExt)
                {
                    OnRefreshItemUp();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.x = toSeat * _slideDir;
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
        /// 强制垂直定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectVSeat(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _maxExtent);
            _contentPos = toSeat;
            OnCircleVertical();
        }
        /// <summary>
        /// 强制水平定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectHSeat(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _maxExtent);
            _contentPos = toSeat;
            OnCircleHorizontal();
        }
        #endregion
        #region//---------------------------普通滑动方式-------------------------------//
        private void OnCircleVerticalNo()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y);
            _tmpContentPos.y = Mathf.Clamp(_tmpContentPos.y , 0, _maxExtent);
            while (_tmpContentPos.y > _contentSite + _itemFirstSize.rect.height + _sProperty.WidthExt)//向下
            {
                if (_sProperty.dataIdx + _sProperty.maxItems >= _dataSet.Count) break;
                OnRefreshItemDown();
            }
            while (_tmpContentPos.y < _contentSite - _sProperty.WidthExt)
            {
                if (_sProperty.dataIdx <= 0) break;
                OnRefreshItemUp();
            }
        }

        private void OnCircleHorizontalNo()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = Mathf.Abs(_tmpContentPos.x);
            _tmpContentPos.x = Mathf.Clamp(_tmpContentPos.x , 0, _maxExtent);
            while (_tmpContentPos.x > _contentSite + _itemFirstSize.rect.width + _sProperty.WidthExt)
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
            toSeat = Mathf.Clamp(toSeat, 0, _maxExtent);
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _itemFirstSize.rect.height + _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _slideDir < toSeat)
                {
                    _tmpContentPos.y = _tmpContentPos.y + _slideDir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.y = toSeat * _slideDir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _itemFirstSize.rect.height + _sProperty.WidthExt
                    && _sProperty.dataIdx + _sProperty.maxItems < _dataSet.Count)
                {
                    OnRefreshItemDown();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _slideDir > toSeat)
                {
                    _tmpContentPos.y = _tmpContentPos.y - _slideDir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.y = toSeat * _slideDir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (_tmpContentPos.y * _slideDir < _contentSite 
                    - _sProperty.WidthExt && _sProperty.dataIdx > 0)
                {
                    OnRefreshItemUp();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.y = toSeat * _slideDir;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
            _toLocationEvent?.Invoke();
        }
        /// <summary>
        /// 水平动画定位
        /// </summary>
        /// <param name="toSeat">位置</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveHSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _maxExtent);
            _scrollRect.enabled = false;    
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _itemFirstSize.rect.width + _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _slideDir < toSeat)
                {
                    _tmpContentPos.x = _tmpContentPos.x + _slideDir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.x = toSeat * _slideDir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _itemFirstSize.rect.width + _sProperty.WidthExt
                    && _sProperty.dataIdx + _sProperty.maxItems < _dataSet.Count)
                {
                    OnRefreshItemDown();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.WidthExt)
            {
                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _slideDir > toSeat)
                {
                    _tmpContentPos.x = _tmpContentPos.x - _slideDir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    _tmpContentPos.x = toSeat * _slideDir;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    _scrollRect.enabled = true;
                    _toLocationEvent?.Invoke();
                    yield break;
                }

                if (_tmpContentPos.x * _slideDir < _contentSite 
                    - _sProperty.WidthExt && _sProperty.dataIdx > 0)
                {
                    OnRefreshItemUp();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.x = toSeat * _slideDir;
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
            toSeat = Mathf.Clamp(toSeat, 0, _maxExtent);
            _contentPos = toSeat;
            OnCircleVerticalNo();
        }
        /// <summary>
        /// 强制水平定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectHSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _maxExtent);
            _contentPos = toSeat;
            OnCircleHorizontalNo();
        }
        #endregion
    }
}
