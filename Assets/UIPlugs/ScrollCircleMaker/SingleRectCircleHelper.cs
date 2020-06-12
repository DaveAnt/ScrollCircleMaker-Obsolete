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
        private BaseItem<T> _tempItem;
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
        private float _contentSize
        {
            get {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return _contentRect.sizeDelta.y;
                    default:
                        return _contentRect.sizeDelta.x;
                }
            }
            set {
                switch (_scrollRect.vertical)
                {
                    case true:
                        _contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x,value);
                        break;
                    default:
                        _contentRect.sizeDelta = new Vector2(value,_contentRect.sizeDelta.y);
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

        private RectTransform _itemFirstSize//不规则长度
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

        private RectTransform _itemFinalSize//不规则长度
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        return _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                    case ScrollDir.BottomToTop:
                        return _contentRect.GetChild(0).transform as RectTransform;
                    case ScrollDir.RightToLeft:
                        return _contentRect.GetChild(0).transform as RectTransform;
                    default:
                        return _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
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
            _tempItem = createItemFunc();
            _tempItem.SetTransform(_baseItem.transform);
            _tempItem.InitComponents();
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
            baseItem.SetTransform(UnityEngine.Object.Instantiate(_baseItem, _contentRect).transform);
            baseItem.InitComponents();
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
            OnAnchorSetNo();
            _firstRun = true;
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;
            _scrollRect.inertia = _sProperty.scrollType != ScrollType.Drag;
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
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
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
            if (_sProperty.isCircleEnable)
            {
                switch (_scrollRect.vertical)
                {
                    case true:
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
            ToLocation(int.MaxValue, isDrawEnable);
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
            _contentSite += _scrollRect.vertical ? (int)_itemFirstSize.rect.height
                + _sProperty.WidthExt: (int)_itemFirstSize.rect.width + _sProperty.WidthExt;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + (_sProperty.dataIdx + _sProperty.maxItems);
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[_sProperty.dataIdx + _sProperty.maxItems], _sProperty.dataIdx + _sProperty.maxItems);
            SetItemSibling(_sProperty.itemIdx,true);
            _sProperty.itemIdx = (_sProperty.itemIdx + 1) % _sProperty.maxItems;
            _sProperty.dataIdx = _sProperty.dataIdx + 1;
        }
        /// <summary>
        /// 上滑动时刷新接口
        /// </summary>
        private void OnRefreshItemUp()
        {
            _contentSite -= _scrollRect.vertical ? (int)_itemFinalSize.rect.height
                + _sProperty.WidthExt : (int)_itemFinalSize.rect.width + _sProperty.WidthExt;
            _sProperty.itemIdx = _sProperty.itemIdx - 1 < 0 ? _sProperty.maxItems - 1 : _sProperty.itemIdx - 1;
            _sProperty.dataIdx = _sProperty.dataIdx - 1;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + _sProperty.dataIdx;
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[_sProperty.dataIdx],_sProperty.dataIdx);
            SetItemSibling(_sProperty.itemIdx,false);
        }
        #region//---------------------------循环滑动方式-------------------------------//
        private void OnCircleVertical()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
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

        private void OnCircleHorizontal()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
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
        /// <param name="toSeat">位置</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveVSeat(int toSeat)
        {
            yield break;
        }
        /// <summary>
        /// 水平动画定位
        /// </summary>
        /// <param name="toSeat">位置</param>
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
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area);
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _itemFirstSize.rect.height + _sProperty.WidthExt)
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
        /// <param name="toSeat">位置</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveHSeatNo(int toSeat)
        {
            toSeat = Mathf.Clamp(toSeat, 0, _cExtra.area);
            _scrollRect.enabled = false;    
            yield return new WaitForEndOfFrame();
            while (toSeat > _contentSite + _itemFirstSize.rect.width + _sProperty.WidthExt)
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
        /// <summary>
        /// 自适应高度
        /// </summary>
        private void OnAnchorSetNo()
        {
            Vector2 contentSize = _contentRect.sizeDelta;
            switch (_scrollRect.vertical)
            {
                case true:
                    contentSize.y = _sProperty.TopExt + _sProperty.BottomExt;
                    for (int i = 0; i < _dataSet.Count; ++i)
                    {
                        _tempItem.UpdateView(_dataSet[i], i);//模拟变化
                        contentSize.y += _tempItem.rectTrans.sizeDelta.y + _sProperty.WidthExt;                            
                    }
                    contentSize.y -= _sProperty.WidthExt;
                    _cExtra.area = (int)(contentSize.y - _viewRect.rect.height);
                    break;
                default:
                    contentSize.x = _sProperty.RightExt + _sProperty.LeftExt;
                    for (int i = 0; i < _dataSet.Count; ++i)
                    {
                        _tempItem.UpdateView(_dataSet[i], i);//模拟变化
                        contentSize.x += _tempItem.rectTrans.sizeDelta.x + _sProperty.WidthExt;
                    }
                    contentSize.x -= _sProperty.WidthExt;
                    _cExtra.area = (int)(contentSize.x - _viewRect.rect.width);
                    break;
            }
            _tmpContentPos = _contentRect.anchoredPosition;
            _contentRect.sizeDelta = contentSize;
        }
        #endregion

    }
}
