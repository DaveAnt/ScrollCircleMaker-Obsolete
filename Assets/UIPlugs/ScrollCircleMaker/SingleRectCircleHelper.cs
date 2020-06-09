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
    public class SingleRectCircleHelper<T> : BaseCircleHelper<T>
    {
        struct ContentExtra
        {
            public short dir;
            public int area;
        }

        private HorizontalOrVerticalLayoutGroup _singleLayoutGroup;
        private ContentExtra _cExtra;
        private Vector2 _tmpContentPos;
        private SizeInt _wholeSize;
        private bool _lockSlide, _firstRun;
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
                    case ScrollDir.BottomToTop:
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

        private float _itemFirstSize//不规则长度
        {
            get
            {
                RectTransform _tmpItem;
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        _tmpItem = _contentRect.GetChild(0).transform as RectTransform;
                        return _tmpItem.rect.height + _sProperty.WidthExt;
                    case ScrollDir.BottomToTop:
                        _tmpItem = _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                        return _tmpItem.rect.height + _sProperty.WidthExt;
                    case ScrollDir.RightToLeft:
                        _tmpItem = _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                        return _tmpItem.rect.width + _sProperty.WidthExt;
                    default:
                        _tmpItem = _contentRect.GetChild(0).transform as RectTransform;
                        return _tmpItem.rect.width + _sProperty.WidthExt;
                }
            }
        }

        private float _itemFinalSize//不规则长度
        {
            get
            {
                RectTransform _tmpItem;
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        _tmpItem = _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                        return _tmpItem.rect.height + _sProperty.WidthExt;
                    case ScrollDir.BottomToTop:
                        _tmpItem = _contentRect.GetChild(0).transform as RectTransform;
                        return _tmpItem.rect.height + _sProperty.WidthExt;
                    case ScrollDir.RightToLeft:
                        _tmpItem = _contentRect.GetChild(0).transform as RectTransform;
                        return _tmpItem.rect.width + _sProperty.WidthExt;
                    default:
                        _tmpItem = _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                        return _tmpItem.rect.width + _sProperty.WidthExt;
                }
            }
        }

        public SingleRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc)
        {
            _createItemFunc = createItemFunc;
            _contentRect = contentTrans as RectTransform;
            _viewRect = contentTrans.parent.GetComponent<RectTransform>();
            _scrollRect = _viewRect.parent.GetComponent<ScrollRect>();
            _sProperty = _contentRect.GetComponent<ScrollCircleComponent>();
            if (_sProperty == null) Debug.LogError("Content must have ScrollCircleComponent!");
            _baseItem = _sProperty.baseItem;
            _itemRect = _baseItem.transform.GetComponent<RectTransform>();
            _scrollRect.onValueChanged.AddListener(OnRefreshHandler);
            _itemSet = new List<BaseItem<T>>();
            _dataSet = new List<T>();
            OnInit();
        }

        private void OnInit()
        {
            _wholeSize.Width = (int)_itemRect.rect.width + _sProperty.WidthExt;
            _wholeSize.Height = (int)_itemRect.rect.height + _sProperty.WidthExt;
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
                    _cExtra.dir = 1;
                    _singleLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    break;
                case ScrollDir.BottomToTop:
                    _cExtra.dir = -1;
                    _singleLayoutGroup.childAlignment = TextAnchor.LowerLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    break;
                case ScrollDir.LeftToRight:
                    _cExtra.dir = -1;
                    _singleLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    break;
                case ScrollDir.RightToLeft:
                    _cExtra.dir = 1;
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
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    OnCircleVerticalNo();
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    OnCircleHorizontalNo();
                    break;
            }
            _timer = 0;
        }

        public void InitItem(int itemIdx)
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

        public override void OnStart(List<T> _tmpDataSet = null)
        {
            _firstRun = true;
            _scrollRect.inertia = _sProperty.scrollType != ScrollType.Drag;
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
            OnAnchorSetNo();
        }
        public override void DelItem(int itemIdx)
        {

        }

        public override void DelItem(Func<T, T, bool> seekFunc, T data)
        {

        }
        public override void AddItem(T data, int itemIdx = -1)
        {
            if (itemIdx != -1) itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count - 1);

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

            //扩展边距
            if (_firstRun)//表示已经初始化，需要计算偏移
            {
                if (_itemSet.Count < _sProperty.maxItems)
                    InitItem(_itemSet.Count);
                //if (_sProperty.isCircleEnable)
                //    OnAnchorSet();
                //else
                //    OnAnchorSetNo();
                //RefreshItems();
            }
        }
        public override void UpdateItem(T data, int itemIdx)
        {

        }
        public override void ResetItems()
        {
            foreach (var baseItem in _itemSet)
                baseItem.OnDestroy();
            Vector2 contentSize = _scrollRect.vertical ?
                new Vector2(_contentRect.sizeDelta.x, 0) :
                new Vector2(0, _contentRect.sizeDelta.y);
            _contentRect.sizeDelta = contentSize;
            _contentSite = _contentPadding;
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
        /// 对齐偏移
        /// </summary>
        /// <param name="tmpItemIdx"></param>
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
                _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
            }
        }
        /// <summary>
        /// 设置排序
        /// </summary>
        /// <param name="tmpItemIdx">设置索引</param>
        /// <param name="tmpSlideDir">滑动方向</param>
        private void SetItemSibling(int tmpItemIdx,bool tmpSlideDir)
        {
            if (tmpSlideDir)
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.LeftToRight:
                        _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                        break;
                    default:
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                        break;
                }
            }
            else
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.LeftToRight:
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                        break;
                    default:
                        _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                        break;
                }
            }

        }
        /// <summary>
        /// 下滑动时刷新接口
        /// </summary>
        private void OnRefreshItemDown()
        {
            _contentSite += (int)_itemFirstSize;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + (_sProperty.dataIdx + _sProperty.maxItems);
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[_sProperty.dataIdx + _sProperty.maxItems]);
            SetItemSibling(_sProperty.itemIdx,true);
            _sProperty.itemIdx = (_sProperty.itemIdx + 1) % _sProperty.maxItems;
            _sProperty.dataIdx = _sProperty.dataIdx + 1;
        }
        /// <summary>
        /// 上滑动时刷新接口
        /// </summary>
        private void OnRefreshItemUp()
        {
            _contentSite -= (int)_itemFinalSize;
            _sProperty.itemIdx = _sProperty.itemIdx - 1 < 0 ? _sProperty.maxItems - 1 : _sProperty.itemIdx - 1;
            _sProperty.dataIdx = _sProperty.dataIdx - 1;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + _sProperty.dataIdx;
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[_sProperty.dataIdx]);
            SetItemSibling(_sProperty.itemIdx,false);
        }

        #region//---------------------------循环滑动方式-------------------------------//
        private void OnCircleVertical()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Clamp(_tmpContentPos.y *
                _cExtra.dir, 0, _cExtra.area);
            while (_tmpContentPos.y > _contentSite + _itemFirstSize)//向下
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

        private void OnCircleHorizontal()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = Mathf.Clamp(_tmpContentPos.x *
                _cExtra.dir, 0, _cExtra.area);
            while (_tmpContentPos.x > _contentSite + _itemFirstSize)
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
        /// <param name="toSeat">位置</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveVSeat(int toSeat)
        {
            yield break;
        }
        /// <summary>
        /// 水平动画定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveHSeat(int toSeat)
        {
            yield break;
        }
        /// <summary>
        /// 强制垂直定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectVSeat(int toSeat)
        {

        }
        /// <summary>
        /// 强制水平定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectHSeat(int toSeat)
        {

        }

        private void OnAnchorSet()
        {
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;
            Vector2 contentSize = _contentRect.sizeDelta;
            _tmpContentPos = _contentRect.anchoredPosition;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:

                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:

                    break;
            }
            _contentRect.sizeDelta = contentSize;
        }
        #endregion

        #region//---------------------------普通滑动方式-------------------------------//
        private void OnCircleVerticalNo()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Clamp(_tmpContentPos.y *
                _cExtra.dir, 0, _cExtra.area);
            while (_tmpContentPos.y > _contentSite + _itemFirstSize)//向下
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
            _tmpContentPos.x = Mathf.Clamp(_tmpContentPos.x *
                _cExtra.dir, 0, _cExtra.area);
            while (_tmpContentPos.x > _contentSite + _itemFirstSize)
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
            while (toSeat > _contentSite + _itemFirstSize)
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

                if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _itemFirstSize &&
                    _sProperty.dataIdx + _sProperty.maxItems < _dataSet.Count)
                {
                    OnRefreshItemDown();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.WidthExt)
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

                if (_tmpContentPos.y * _cExtra.dir < _contentSite 
                    - _sProperty.WidthExt && _sProperty.dataIdx > 0)
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
            while (toSeat > _contentSite + _itemFirstSize)
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

                if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _itemFirstSize &&
                    _sProperty.dataIdx + _sProperty.maxItems < _dataSet.Count)
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

                if (_tmpContentPos.x * _cExtra.dir < _contentSite 
                    - _sProperty.WidthExt && _sProperty.dataIdx > 0)
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
            _contentPos = toSeat;
            OnCircleVerticalNo();
        }
        /// <summary>
        /// 强制水平定位
        /// </summary>
        /// <param name="toSeat">位置参数</param>
        private void ToDirectHSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area);
            _contentPos = toSeat;
            OnCircleHorizontalNo();
        }

        private void OnAnchorSetNo()
        {
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;
            Vector2 contentSize = _contentRect.sizeDelta;
            _tmpContentPos = _contentRect.anchoredPosition;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    _cExtra.area = (int)(_sProperty.TopExt + _sProperty.BottomExt + _dataSet.Count
                        * _wholeSize.Height - _sProperty.WidthExt - _viewRect.rect.height);
                    contentSize.y = _cExtra.area + _viewRect.rect.height;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _cExtra.area = (int)(_sProperty.LeftExt + _sProperty.RightExt + _dataSet.Count
                        * _wholeSize.Width - _sProperty.WidthExt - _viewRect.rect.width);
                    contentSize.y = _cExtra.area + _viewRect.rect.height;
                    break;
            }
            _contentRect.sizeDelta = contentSize;
        }
#endregion

    }
}
