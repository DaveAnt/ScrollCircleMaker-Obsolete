//------------------------------------------------------------
// ScrollCircleMaker
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker       //多行矩形滑动循环
{
    public class MultipleRectCircleHelper<T> : BaseScrollCircleHelper<T>
    {
        private GridLayoutGroup _gridLayoutGroup;
        private SizeInt _wholeSize,_lookSize, _maxRanks;
        private RectOffset _boundaryArea;
        private Vector2 _tmpContentPos;
        private float _timer = 0;
        private bool _lockSlide;
        
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
            _boundaryArea = new RectOffset();
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
                    _maxRanks.Height = (int)(Math.Ceiling(_viewRect.rect.height/(_itemRect.rect.height + _sProperty.HeightExt))+1);
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
            _gridLayoutGroup.padding.left = _sProperty.LeftExt;
            _gridLayoutGroup.padding.right = _sProperty.RightExt;
            _gridLayoutGroup.padding.top = _sProperty.TopExt;
            _gridLayoutGroup.padding.bottom = _sProperty.BottomExt;
            _gridLayoutGroup.spacing = new Vector2(_sProperty.WidthExt,_sProperty.HeightExt);
            _gridLayoutGroup.cellSize = _itemRect.rect.size;
            _lookSize.Width = (int)(_maxRanks.Width * (_itemRect.rect.width + _sProperty.WidthExt) - _sProperty.WidthExt);
            _lookSize.Height = (int)(_maxRanks.Height * (_itemRect.rect.height + _sProperty.HeightExt) - _sProperty.HeightExt);
        }

        private void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            if (!_sProperty.isSlideEnable || _lockSlide)
                return;
            if (_timer <= _sProperty.refreshRatio)
            {
                _timer += Time.deltaTime;
                return;
            }
            _tmpContentPos = _contentRect.anchoredPosition;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:                       
                    while (_tmpContentPos.y + _viewRect.rect.height >
                        _gridLayoutGroup.padding.top + _lookSize.Height + _sProperty.HeightExt)//向下
                    {
                        if (_sProperty.dataIdx + _maxRanks.Width >= _dataSet.Count)return;//数据到底了

                        for (int i = 0; i < _maxRanks.Width; ++i)
                        {
                            int tmpItemIdx = _sProperty.itemIdx + i;
                            int tmpDataIdx = _sProperty.dataIdx +_sProperty.maxItems + i;
                            _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                            if (tmpDataIdx < _dataSet.Count)
                            {
                                if(_itemSet[tmpItemIdx].gameObject.activeSelf == false)
                                    _itemSet[tmpItemIdx].gameObject.SetActive(true);
                                _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                            }          
                            else
                                _itemSet[tmpItemIdx].gameObject.SetActive(false);
                            _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                        }

                        _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Width;
                        _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Width >= _sProperty.maxItems?0:_sProperty.itemIdx + _maxRanks.Width;
                        _gridLayoutGroup.padding.top += _wholeSize.Height;
                        _gridLayoutGroup.SetLayoutVertical();
                    }
                    while (_contentRect.anchoredPosition.y  <
                        _gridLayoutGroup.padding.top - _sProperty.HeightExt)
                    {
                        if (_sProperty.dataIdx <= 0)return;
                        _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width;
                        _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width < 0 ? _sProperty.maxItems - _maxRanks.Width : _sProperty.itemIdx - _maxRanks.Width;                
                        
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
                        _gridLayoutGroup.padding.top -= _wholeSize.Height;
                        _gridLayoutGroup.SetLayoutVertical();
                    }
                    break;
                case ScrollDir.BottomToTop:
                    while (-_tmpContentPos.y + _viewRect.rect.height >
                        _gridLayoutGroup.padding.bottom + _lookSize.Height + _sProperty.HeightExt)//向上
                    {
                        if (_sProperty.dataIdx + _maxRanks.Width >= _dataSet.Count) return;//数据到底

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
                        _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Width >= _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Width;
                        _gridLayoutGroup.padding.bottom += _wholeSize.Height;
                        _gridLayoutGroup.SetLayoutVertical();
                    }
                    while (-_contentRect.anchoredPosition.y <
                        _gridLayoutGroup.padding.bottom - _sProperty.HeightExt)
                    {
                        if (_sProperty.dataIdx <= 0) return;
                        _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width;
                        _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width < 0 ? _sProperty.maxItems - _maxRanks.Width : _sProperty.itemIdx - _maxRanks.Width;

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
                        _gridLayoutGroup.padding.bottom -= _wholeSize.Height;
                        _gridLayoutGroup.SetLayoutVertical();
                    }
                    break;
                case ScrollDir.LeftToRight:
                    while (-_tmpContentPos.x + _viewRect.rect.width >
                            _gridLayoutGroup.padding.left + _lookSize.Width + _sProperty.WidthExt)
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
                        _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Height >= _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Height;
                        _gridLayoutGroup.padding.left += _wholeSize.Width;
                        _gridLayoutGroup.SetLayoutHorizontal();
                    }
                    while (-_contentRect.anchoredPosition.x <
                        _gridLayoutGroup.padding.left - _sProperty.WidthExt)
                    {
                        if (_sProperty.dataIdx <= 0) return;
                        _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height;
                        _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height < 0 ? _sProperty.maxItems - _maxRanks.Height : _sProperty.itemIdx - _maxRanks.Height;

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
                        _gridLayoutGroup.padding.left -= _wholeSize.Width;
                        _gridLayoutGroup.SetLayoutHorizontal();
                    }
                    break;
                case ScrollDir.RightToLeft:
                    while (_tmpContentPos.x + _viewRect.rect.width >
                        _gridLayoutGroup.padding.right + _lookSize.Width + _sProperty.WidthExt)
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
                        _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Height >= _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Height;
                        _gridLayoutGroup.padding.right += _wholeSize.Width;
                        _gridLayoutGroup.SetLayoutHorizontal();
                    }
                    while (_contentRect.anchoredPosition.x <
                        _gridLayoutGroup.padding.right - _sProperty.WidthExt)
                    {
                        if (_sProperty.dataIdx <= 0) return;
                        _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height;
                        _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height < 0 ? _sProperty.maxItems - _maxRanks.Height : _sProperty.itemIdx - _maxRanks.Height;

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
                        _gridLayoutGroup.padding.right -= _wholeSize.Width;
                        _gridLayoutGroup.SetLayoutHorizontal();
                    }
                    break;
            }

            _timer = 0;
        }

        private void OnResolveGroupEnum()
        {
            //解析排版
            int sign=(short)_sProperty.scrollDir*10 + (short)_sProperty.scrollSort;
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
            sign = sign*10+(_sProperty.isCircleEnable ? 1 : 0);
            switch (sign)
            {
                case 0:case 10:case 20:case 30:case 200:case 210:case 220:case 230:
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    break;
                case 1:case 11:case 21:case 31:case 101:case 111:case 121:case 131:
                    _gridLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
                    break;
                case 100:case 110:case 120:case 130:
                    _gridLayoutGroup.childAlignment = TextAnchor.LowerLeft;
                    break;            
                case 201:case 211:case 221:case 231:case 301:case 311:case 321:case 331:
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                    break;
                case 300:case 310:case 320: case 330:
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperRight;
                    break;
            }
        }

        private void OnAnchorSet()
        {
            Vector2 contentSize = _contentRect.sizeDelta;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = _sProperty.TopExt + _sProperty.BottomExt - _sProperty.HeightExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _wholeSize.Height;
                    _boundaryArea.top = _sProperty.TopExt;
                    _boundaryArea.bottom = (int)(contentSize.y - _viewRect.rect.height - _sProperty.BottomExt);
                    break;
                case ScrollDir.BottomToTop:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = _sProperty.TopExt + _sProperty.BottomExt - _sProperty.HeightExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _wholeSize.Height;
                    _boundaryArea.top = _sProperty.BottomExt;
                    _boundaryArea.bottom = (int)(contentSize.y - _viewRect.rect.height - _sProperty.TopExt);
                    break;
                case ScrollDir.LeftToRight:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt - _sProperty.WidthExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _wholeSize.Width;
                    _boundaryArea.left = _sProperty.LeftExt;
                    _boundaryArea.right = (int)(contentSize.x - _viewRect.rect.width - _sProperty.RightExt);
                    break;
                case ScrollDir.RightToLeft:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt - _sProperty.WidthExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _wholeSize.Width;
                    _boundaryArea.left = _sProperty.RightExt;
                    _boundaryArea.right = (int)(contentSize.x - _viewRect.rect.width - _sProperty.LeftExt);
                    break;
            }
            _contentRect.sizeDelta = contentSize;
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

        public override void OnStart(List<T> _tmpDataSet = null)
        {
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
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;
            OnAnchorSet();
        }

        public override void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveListener(OnRefreshHandler);
            _dataSet.Clear();
            _itemSet.Clear();
            GC.Collect();
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

        public override void AddItem(T data,int itemIdx = -1)
        {
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
                        _dataSet.Insert(_dataSet.Count-itemIdx - 1, data);
                    else
                        _dataSet.Insert(0, data);
                    break;
            }     
        }

        public override void ToTop(bool isDrawEnable = true)
        {

        }

        public override void ToBottom(bool isDrawEnable = true)
        {

        }

        public override void UpdateItem(T data, int itemIdx)
        {
            
        }

        public override Vector4 GetLocationParam()
        {
            throw new NotImplementedException();
        }

        public override void ToLocation(Vector4 locationNode, bool isDrawEnable = true)
        {
            
        }
    }
}