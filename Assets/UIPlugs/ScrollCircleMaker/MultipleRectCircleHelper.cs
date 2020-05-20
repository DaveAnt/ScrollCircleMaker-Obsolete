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

namespace UIPlugs.ScrollCircleMaker       //多行矩形滑动循环
{
    public class MultipleRectCircleHelper<T> : BaseScrollCircleHelper<T>
    {
        private GridLayoutGroup _gridLayoutGroup;
        private SizeInt _wholeSize, _lookSize, _maxRanks;
        private RangeInt _boundaryArea;
        private Vector2 _tmpContentPos;
        private float _timer = 0;
        private bool _lockSlide;
        private bool _firstRun;

        private int contentSite//偏移锚点
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

        public MultipleRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc)
        {
            _createItemFunc = createItemFunc;
            _contentRect = contentTrans as RectTransform;
            _viewRect = contentTrans.parent.GetComponent<RectTransform>();
            _scrollRect = _viewRect.parent.GetComponent<ScrollRect>();
            _sProperty = _contentRect.GetComponent<ScrollCircleComponent>();
            if (_sProperty == null) Debug.LogError("content must have ScrollCircleComponent!");

            _baseItem = _sProperty.baseItem;
            _gridLayoutGroup = _contentRect.GetComponent<GridLayoutGroup>() ?? _contentRect.gameObject.AddComponent<GridLayoutGroup>();
            _itemRect = _baseItem.transform.GetComponent<RectTransform>();
            _scrollRect.onValueChanged.AddListener(OnRefreshHandler);
            _itemSet = new List<BaseItem<T>>();
            _dataSet = new List<T>();
            OnInit();
        }
        private void OnInit()
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                    float tmpWidth = _contentRect.rect.width - _sProperty.LeftExt - _sProperty.RightExt;
                    _maxRanks.Width = (int)((tmpWidth + _sProperty.WidthExt) / (_itemRect.rect.width + _sProperty.WidthExt));
                    _maxRanks.Height = (int)(Math.Ceiling(_viewRect.rect.height / (_itemRect.rect.height + _sProperty.HeightExt)) + 1);
                    _sProperty.maxItems = _maxRanks.Width * _maxRanks.Height;
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                    float tmpHeight = _contentRect.rect.height - _sProperty.TopExt - _sProperty.BottomExt;
                    _maxRanks.Height = (int)((tmpHeight + _sProperty.HeightExt) / (_itemRect.rect.height + _sProperty.HeightExt));
                    _maxRanks.Width = (int)(Math.Ceiling(_viewRect.rect.width / (_itemRect.rect.width + _sProperty.WidthExt)) + 1);
                    _sProperty.maxItems = _maxRanks.Width * _maxRanks.Height;
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    break;
            }
            OnResolveGroupEnum();
            _gridLayoutGroup.spacing = new Vector2(_sProperty.WidthExt, _sProperty.HeightExt);
            _gridLayoutGroup.cellSize = _itemRect.rect.size;
            _lookSize.Width = (int)(_maxRanks.Width * (_itemRect.rect.width + _sProperty.WidthExt) - _sProperty.WidthExt);
            _lookSize.Height = (int)(_maxRanks.Height * (_itemRect.rect.height + _sProperty.HeightExt) - _sProperty.HeightExt);

#if IsCircleEnable
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.BottomToTop:
                case ScrollDir.TopToBottom:
                    _gridLayoutGroup.padding.top = 0;
                    _gridLayoutGroup.padding.bottom = 0;
                    _gridLayoutGroup.padding.left = _sProperty.LeftExt;
                    _gridLayoutGroup.padding.right = _sProperty.RightExt;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _gridLayoutGroup.padding.left = 0;
                    _gridLayoutGroup.padding.right = 0;
                    _gridLayoutGroup.padding.top = _sProperty.TopExt;
                    _gridLayoutGroup.padding.bottom = _sProperty.BottomExt;
                    break;
            }
#else
            _gridLayoutGroup.padding.left = _sProperty.LeftExt;
            _gridLayoutGroup.padding.right = _sProperty.RightExt;
            _gridLayoutGroup.padding.top = _sProperty.TopExt;
            _gridLayoutGroup.padding.bottom = _sProperty.BottomExt;
#endif
        }

        private void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            if (!_sProperty.isSlideEnable || _lockSlide)
                return;
            if (_timer <= _sProperty.refreshRatio){
                _timer += Time.deltaTime;
                return;
            }

            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    OnCircleVertical();
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    OnCircleHorizontal();
                    break;
            }

            _timer = 0;
        }
        private void OnResolveGroupEnum()
        {
            //解析排版
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
            sign = sign * 10 + (_sProperty.isCircleEnable ? 1 : 0);
            switch (sign)
            {
                case 0:case 10:case 20:case 30:case 200:case 210:case 220:case 230:
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    break;
                case 1:case 11:case 21:case 31:case 101:case 111:case 121:case 131:
                    _gridLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
                    break;
                case 100:case 110: case 120:case 130:
                    _gridLayoutGroup.childAlignment = TextAnchor.LowerLeft;
                    break;
                case 201:case 211:case 221:case 231:case 301:case 311:case 321:case 331:
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                    break;
                case 300: case 310:case 320:case 330:
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperRight;
                    break;
            }
        }
#if IsCircleEnable
        private void OnCircleVertical()
        {
            
        }

        private void OnCircleHorizontal()
        {
            
        }

        private void RefreshItems()
        {
            int tmpItemIdx, tmpDataIdx;
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                tmpItemIdx = (_sProperty.itemIdx + i) % _sProperty.maxItems;
                tmpDataIdx = (_sProperty.dataIdx + i) % _dataSet.Count;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
            }
        }

        private void InitItem(int itemIdx)
        {
            int tmpItemIdx = itemIdx % _dataSet.Count;
            BaseItem<T> baseItem = _createItemFunc();
            RectTransform itemRect = UnityEngine.Object.Instantiate(_baseItem, _contentRect).transform as RectTransform;
            baseItem.transform = itemRect as Transform;
            baseItem.gameObject = itemRect.gameObject;
            baseItem.gameObject.name = _baseItem.name + tmpItemIdx;
            baseItem.InitComponents();
            baseItem.InitEvents();
            baseItem.UpdateView(_dataSet[tmpItemIdx]);
            _itemSet.Add(baseItem);
        }

        private void OnAnchorSet()
        {
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;

            Vector2 contentSize = _contentRect.sizeDelta;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width)
                        * _wholeSize.Height - _sProperty.HeightExt;
                    contentSize.y = contentSize.y < _viewRect.rect.height ?
                        3 * _viewRect.rect.height : contentSize.y + 2 * _viewRect.rect.height;
                    break;
                case ScrollDir.BottomToTop:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width)
                        * _wholeSize.Height - _sProperty.HeightExt;
                    contentSize.y = contentSize.y < _viewRect.rect.height ?
                        3 * _viewRect.rect.height : contentSize.y + 2 * _viewRect.rect.height;
                    break;
                case ScrollDir.LeftToRight:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height)
                        * _wholeSize.Width - _sProperty.WidthExt;
                    contentSize.x = contentSize.x < _viewRect.rect.width ?
                        3 * _viewRect.rect.width : contentSize.x + 2 * _viewRect.rect.width;
                    break;
                case ScrollDir.RightToLeft:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = contentSize.x < _viewRect.rect.width ?
                        3 * _viewRect.rect.width : contentSize.x + 2 * _viewRect.rect.width;
                    break;
            }
            Debug.LogError(contentSize);
            _contentRect.sizeDelta = contentSize;
        }

        public override void OnStart(List<T> _tmpDataSet = null)
        {
            _firstRun = true;
            if (_tmpDataSet != null)
            {
                switch (_sProperty.scrollSort)
                {
                    case ScrollSort.BackDir:
                    case ScrollSort.BackZDir:
                        _tmpDataSet.Reverse();
                        break;
                }
                _dataSet.AddRange(_tmpDataSet);
            }
            if (_dataSet.Count > 0)
            {
                for (int i = 0; i < _sProperty.maxItems; ++i)
                    InitItem(i);
            }
            OnAnchorSet();
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

        public override void ResetItems()
        {
            
        }

        public override int GetLocation()
        {
            throw new NotImplementedException();
        }

        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {
            
        }

        public override void ToTop(bool isDrawEnable = true)
        {
            
        }

        public override void ToBottom(bool isDrawEnable = true)
        {
            
        }
#else
        private void OnCircleVertical()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Clamp(_tmpContentPos.y *
                _boundaryArea.start, 0,  _boundaryArea.length);
            while (_tmpContentPos.y + _viewRect.rect.height >
                contentSite + _lookSize.Height + _sProperty.HeightExt)//向下
            {
                if (_contentRect.anchoredPosition.y * _boundaryArea.start < 0) return;
                if (_sProperty.dataIdx + _maxRanks.Width >= _dataSet.Count) return;//数据到底了

                for (int i = 0; i < _maxRanks.Width; ++i)
                {
                    int tmpItemIdx = _sProperty.itemIdx + i;
                    int tmpDataIdx = _sProperty.dataIdx + _sProperty.maxItems + i;
                    _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                    if (tmpDataIdx < _dataSet.Count)
                    {
                        if (_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                            _itemSet[tmpItemIdx].gameObject.SetActive(true);
                        _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                    }
                    else
                        _itemSet[tmpItemIdx].gameObject.SetActive(false);
                    _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                }

                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Width;
                _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Width >=
                    _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Width;
                contentSite += _wholeSize.Height;
                _gridLayoutGroup.SetLayoutVertical();
            }
            while (_tmpContentPos.y < contentSite - _sProperty.HeightExt)
            {
                if (_sProperty.dataIdx <= 0) return;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width < 0 ?
                    _sProperty.maxItems - _maxRanks.Width : _sProperty.itemIdx - _maxRanks.Width;

                for (int i = _maxRanks.Width - 1; i >= 0; --i)
                {
                    int tmpItemIdx = _sProperty.itemIdx + i;
                    int tmpDataIdx = _sProperty.dataIdx + i;
                    _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                    if (tmpDataIdx >= 0)
                    {
                        if (_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                            _itemSet[tmpItemIdx].gameObject.SetActive(true);
                        _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                    }
                    _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                }
                contentSite -= _wholeSize.Height;
                _gridLayoutGroup.SetLayoutVertical();
            }
        }

        private void OnCircleHorizontal()
        {
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = Mathf.Clamp(_tmpContentPos.x * 
                _boundaryArea.start,0, _boundaryArea.length);
            while (_tmpContentPos.x + _viewRect.rect.width >
                    contentSite + _lookSize.Width + _sProperty.WidthExt)
            {                
                if (_sProperty.dataIdx + _maxRanks.Height >= _dataSet.Count) return;

                for (int i = 0; i < _maxRanks.Height; ++i)
                {
                    int tmpItemIdx = _sProperty.itemIdx + i;
                    int tmpDataIdx = _sProperty.dataIdx + _sProperty.maxItems + i;
                    _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                    if (tmpDataIdx < _dataSet.Count)
                    {
                        if (_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                            _itemSet[tmpItemIdx].gameObject.SetActive(true);
                        _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                    }
                    else
                        _itemSet[tmpItemIdx].gameObject.SetActive(false);
                    _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                }

                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Height;
                _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Height >=
                    _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Height;
                contentSite += _wholeSize.Width;
                _gridLayoutGroup.SetLayoutHorizontal();
            }
            while (_tmpContentPos.x < contentSite - _sProperty.WidthExt)
            {
                if (_sProperty.dataIdx <= 0) return;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height < 0 ?
                    _sProperty.maxItems - _maxRanks.Height : _sProperty.itemIdx - _maxRanks.Height;

                for (int i = _maxRanks.Height - 1; i >= 0; --i)
                {
                    int tmpItemIdx = _sProperty.itemIdx + i;
                    int tmpDataIdx = _sProperty.dataIdx + i;
                    _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                    if (tmpDataIdx >= 0)
                    {
                        if (_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                            _itemSet[tmpItemIdx].gameObject.SetActive(true);
                        _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                    }
                    _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                }
                contentSite -= _wholeSize.Width;
                _gridLayoutGroup.SetLayoutHorizontal();
            }
        }

        private IEnumerator ToAutoMoveVSeat(int toSeat)
        {           
            if(_boundaryArea.length <= 0) yield break;
            toSeat = Mathf.Clamp(toSeat, 0, _boundaryArea.length);

            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat + _viewRect.rect.height >
                contentSite + _lookSize.Height + _sProperty.HeightExt)
            {
                if (_sProperty.dataIdx + _maxRanks.Width >= _dataSet.Count)
                {
                    _scrollRect.enabled = true;
                    yield break;
                }

                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _boundaryArea.start < toSeat)
                {         
                    _tmpContentPos.y = _tmpContentPos.y + _boundaryArea.start * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _scrollRect.enabled = true;
                    yield break;
                }

                if (_tmpContentPos.y * _boundaryArea.start + _viewRect.rect.height >
                    contentSite + _lookSize.Height + _sProperty.HeightExt)
                {
                    for (int i = 0; i < _maxRanks.Width; ++i)
                    {
                        int tmpItemIdx = _sProperty.itemIdx + i;
                        int tmpDataIdx = _sProperty.dataIdx + _sProperty.maxItems + i;
                        _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                        if (tmpDataIdx < _dataSet.Count)
                        {
                            if (_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                                _itemSet[tmpItemIdx].gameObject.SetActive(true);
                            _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                        }
                        else
                            _itemSet[tmpItemIdx].gameObject.SetActive(false);
                        _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                    }

                    _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Width;
                    _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Width >=
                        _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Width;
                    contentSite += _wholeSize.Height;
                    _gridLayoutGroup.SetLayoutVertical();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < contentSite - _sProperty.HeightExt)
            {
                if (_sProperty.dataIdx <= 0)
                {
                    _scrollRect.enabled = true;
                    yield break;
                }

                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _boundaryArea.start > toSeat)
                {            
                    _tmpContentPos.y =_tmpContentPos.y - _boundaryArea.start * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _scrollRect.enabled = true;
                    yield break;
                }

                if (_tmpContentPos.y * _boundaryArea.start < contentSite - _sProperty.HeightExt)
                {
                    _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width;
                    _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width < 0 ?
                        _sProperty.maxItems - _maxRanks.Width : _sProperty.itemIdx - _maxRanks.Width;

                    for (int i = _maxRanks.Width - 1; i >= 0; --i)
                    {
                        int tmpItemIdx = _sProperty.itemIdx + i;
                        int tmpDataIdx = _sProperty.dataIdx + i;
                        _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                        if (tmpDataIdx >= 0)
                        {
                            if (_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                                _itemSet[tmpItemIdx].gameObject.SetActive(true);
                            _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                        }
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                    }
                    contentSite -= _wholeSize.Height;
                    _gridLayoutGroup.SetLayoutVertical();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.y = toSeat * _boundaryArea.start;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
        }

        private IEnumerator ToAutoMoveHSeat(int toSeat)
        {
            if (_boundaryArea.length <= 0) yield break;
            toSeat = Mathf.Clamp(toSeat, 0, _boundaryArea.length);
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat + _viewRect.rect.width >
                    contentSite + _lookSize.Width + _sProperty.WidthExt)
            {
                if (_sProperty.dataIdx + _maxRanks.Height >= _dataSet.Count) {
                    _scrollRect.enabled = true;
                    yield break;
                }

                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _boundaryArea.start < toSeat){
                    _tmpContentPos.x = _tmpContentPos.x + _boundaryArea.start * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else {
                    _scrollRect.enabled = true;
                    yield break;
                }              

                if (_tmpContentPos.x * _boundaryArea.start + _viewRect.rect.width >
                    contentSite + _lookSize.Width + _sProperty.WidthExt)
                {
                    for (int i = 0; i < _maxRanks.Height; ++i)
                    {
                        int tmpItemIdx = _sProperty.itemIdx + i;
                        int tmpDataIdx = _sProperty.dataIdx + _sProperty.maxItems + i;
                        _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                        if (tmpDataIdx < _dataSet.Count)
                        {
                            if (_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                                _itemSet[tmpItemIdx].gameObject.SetActive(true);
                            _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                        }
                        else
                            _itemSet[tmpItemIdx].gameObject.SetActive(false);
                        _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                    }

                    _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Height;
                    _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Height >=
                        _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Height;
                    contentSite += _wholeSize.Width;
                    _gridLayoutGroup.SetLayoutHorizontal();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < contentSite - _sProperty.WidthExt)
            {
                if (_sProperty.dataIdx <= 0) {
                    _scrollRect.enabled = true;
                    yield break;
                }

                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _boundaryArea.start > toSeat){
                    _tmpContentPos.x = _tmpContentPos.x - _boundaryArea.start * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _scrollRect.enabled = true;
                    yield break;
                }

                if (_tmpContentPos.x * _boundaryArea.start < contentSite - _sProperty.WidthExt)
                {
                    _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height;
                    _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height < 0 ?
                        _sProperty.maxItems - _maxRanks.Height : _sProperty.itemIdx - _maxRanks.Height;

                    for (int i = _maxRanks.Height - 1; i >= 0; --i)
                    {
                        int tmpItemIdx = _sProperty.itemIdx + i;
                        int tmpDataIdx = _sProperty.dataIdx + i;
                        _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                        if (tmpDataIdx >= 0)
                        {
                            if (_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                                _itemSet[tmpItemIdx].gameObject.SetActive(true);
                            _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                        }
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                    }
                    contentSite -= _wholeSize.Width;
                    _gridLayoutGroup.SetLayoutHorizontal();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.x = toSeat * _boundaryArea.start;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
        }

        private void ToDirectVSeat(int toSeat)
        {
            if (_boundaryArea.length <= 0) return;
            toSeat = Mathf.Clamp(toSeat, 0, _boundaryArea.length);
   
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = toSeat * _boundaryArea.start;
            _contentRect.anchoredPosition = _tmpContentPos;

            int tmpItemIdx;
            if (Mathf.Abs(_tmpContentPos.y) + _viewRect.rect.height >
                    contentSite + _lookSize.Height + _sProperty.HeightExt)//向下
            {
                tmpItemIdx = _sProperty.itemIdx;
                int tmpRow = (int)Math.Ceiling((Mathf.Abs(_tmpContentPos.y) + _viewRect.rect.height
                        - contentSite - _lookSize.Height - _sProperty.HeightExt) / _wholeSize.Height);
                contentSite += _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Width * tmpRow;
                _sProperty.itemIdx = (tmpItemIdx + _maxRanks.Width * tmpRow) % _sProperty.maxItems;

                //对齐偏移
                for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                    _itemSet[i].transform.SetAsLastSibling();
                for (int i = tmpItemIdx; i > _sProperty.itemIdx; --i)
                    _itemSet[i].transform.SetAsFirstSibling();
            }
            else if (Mathf.Abs(_tmpContentPos.y) * _boundaryArea.start < contentSite - _sProperty.HeightExt)
            {
                tmpItemIdx = _sProperty.itemIdx;
                int tmpRow = (int)Math.Ceiling((contentSite
                    - _sProperty.HeightExt - Mathf.Abs(_tmpContentPos.y)) / _wholeSize.Height);
                contentSite -= _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width * tmpRow;
                _sProperty.itemIdx = tmpItemIdx - _maxRanks.Width * tmpRow;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems - _maxRanks.Width : _sProperty.itemIdx;

                //对齐偏移
                for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                    _itemSet[i].transform.SetAsLastSibling();
                for (int i = tmpItemIdx; i > _sProperty.itemIdx; --i)
                    _itemSet[i].transform.SetAsFirstSibling();
            }

            RefreshItems();
            _gridLayoutGroup.SetLayoutVertical();
        }

        private void ToDirectHSeat(int toSeat)
        {
            if (_boundaryArea.length <= 0) return;
            toSeat = Mathf.Clamp(toSeat, 0, _boundaryArea.length);
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = toSeat * _boundaryArea.start;
            _contentRect.anchoredPosition = _tmpContentPos;

            int tmpItemIdx;
            if (Mathf.Abs(_tmpContentPos.x) + _viewRect.rect.width >
                    contentSite + _lookSize.Width + _sProperty.WidthExt)//向下
            {
                tmpItemIdx = _sProperty.itemIdx;
                int tmpColumn = (int)Math.Ceiling((Mathf.Abs(_tmpContentPos.x) + _viewRect.rect.width
                        - contentSite - _lookSize.Width - _sProperty.WidthExt) / _wholeSize.Width);
                contentSite += _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Height * tmpColumn;
                _sProperty.itemIdx = (tmpItemIdx + _maxRanks.Height * tmpColumn) % _sProperty.maxItems;

                //对齐偏移
                for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                    _itemSet[i].transform.SetAsLastSibling();
                for (int i = tmpItemIdx; i > _sProperty.itemIdx; --i)
                    _itemSet[i].transform.SetAsFirstSibling();
            }
            else if (Mathf.Abs(_tmpContentPos.x) < contentSite - _sProperty.HeightExt)
            {
                tmpItemIdx = _sProperty.itemIdx;
                int tmpColumn = (int)Math.Ceiling((contentSite
                    - _sProperty.HeightExt - Mathf.Abs(_tmpContentPos.x)) / _wholeSize.Width);
                contentSite -= _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height * tmpColumn;
                _sProperty.itemIdx = tmpItemIdx - _maxRanks.Height * tmpColumn;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems - _maxRanks.Height : _sProperty.itemIdx;

                //对齐偏移
                for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                    _itemSet[i].transform.SetAsLastSibling();
                for (int i = tmpItemIdx; i > _sProperty.itemIdx; --i)
                    _itemSet[i].transform.SetAsFirstSibling();
            }

            RefreshItems();
            _gridLayoutGroup.SetLayoutHorizontal();
        }  

        private void RefreshItems()
        {
            int tmpItemIdx;
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                if (i > _itemSet.Count - 1) return;
                tmpItemIdx = (_sProperty.itemIdx + i) % _sProperty.maxItems;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + (_sProperty.dataIdx + i);
                if (_sProperty.dataIdx + i >= 0 &&
                    _sProperty.dataIdx + i < _dataSet.Count)
                {
                    if (!_itemSet[tmpItemIdx].gameObject.activeSelf)
                        _itemSet[tmpItemIdx].gameObject.SetActive(true);
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[_sProperty.dataIdx + i]);
                }
                else
                    _itemSet[tmpItemIdx].gameObject.SetActive(false);
            }
        }

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

        private void OnAnchorSet()
        {
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;

            Vector2 contentSize = _contentRect.sizeDelta;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = _sProperty.TopExt + _sProperty.BottomExt - _sProperty.HeightExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _wholeSize.Height;
                    _boundaryArea.start = 1;
                    _boundaryArea.length = (int)(contentSize.y - _viewRect.rect.height);
                    break;
                case ScrollDir.BottomToTop:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = _sProperty.TopExt + _sProperty.BottomExt - _sProperty.HeightExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _wholeSize.Height;
                    _boundaryArea.start = -1;
                    _boundaryArea.length = (int)(contentSize.y - _viewRect.rect.height);
                    break;
                case ScrollDir.LeftToRight:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt - _sProperty.WidthExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _wholeSize.Width;
                    _boundaryArea.start = -1;
                    _boundaryArea.length = (int)(contentSize.x - _viewRect.rect.width);
                    break;
                case ScrollDir.RightToLeft:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt - _sProperty.WidthExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _wholeSize.Width;
                    _boundaryArea.start = 1;
                    _boundaryArea.length = (int)(contentSize.x - _viewRect.rect.width);
                    break;
            }
            _contentRect.sizeDelta = contentSize;
        }

        public override void OnStart(List<T> _tmpDataSet = null)
        {
            _firstRun = true;
            if (_tmpDataSet != null)
            {
                switch (_sProperty.scrollSort)
                {
                    case ScrollSort.BackDir:
                    case ScrollSort.BackZDir:
                        _tmpDataSet.Reverse();
                        break;
                }
                _dataSet.AddRange(_tmpDataSet);
            }
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                if (i >= _dataSet.Count) break;//表示没有数据
                InitItem(i);
            }
            OnAnchorSet();
        }

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

        public override void AddItem(T data, int itemIdx = -1)
        {
            if(itemIdx != -1)  itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count-1);

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
                OnAnchorSet();
                RefreshItems();
            }
        }

        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {
            if (_sProperty.isCircleEnable) return;
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

        public override void ToTop(bool isDrawEnable = true)
        {
            ToLocation(0, isDrawEnable);
        }

        public override void ToBottom(bool isDrawEnable = true)
        {
            ToLocation(_boundaryArea.length, isDrawEnable);
        }
#endif
        //公有函数 不管是不是轮回
        public override void UpdateItem(T data, int itemIdx)
        {
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count - 1);
            _dataSet[itemIdx] = data;
            if (itemIdx >= _sProperty.dataIdx &&
                itemIdx < _sProperty.dataIdx + _sProperty.maxItems)
            {
                int tmpItemIdx = (itemIdx - _sProperty.dataIdx + _sProperty.itemIdx) % _sProperty.maxItems;
                _itemSet[tmpItemIdx].UpdateView(data);
            }
        }

        public override void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveListener(OnRefreshHandler);
            _dataSet.Clear();
            _itemSet.Clear();
            GC.Collect();
        }
    }
}