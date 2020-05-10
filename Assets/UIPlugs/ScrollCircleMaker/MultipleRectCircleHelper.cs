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
        private Transform _gridLayoutTrans;
        private Vector2Int _wholeSize,_lookSize, _maxRanks;
        private Vector2 _tmpContentPos;
        private int _itemIdx, _dataIdx;
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
            _gridLayoutTrans = _gridLayoutGroup.transform;
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
                    _maxRanks.x = (int)((tmpWidth + _sProperty.WidthExt) / (_itemRect.rect.width + _sProperty.WidthExt));
                    _maxRanks.y = (int)(Math.Ceiling(_viewRect.rect.height/(_itemRect.rect.height + _sProperty.HeightExt))+1);
                    _sProperty.maxItems = _maxRanks.x * _maxRanks.y;
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                    float tmpHeight = _contentRect.rect.height - _sProperty.TopExt - _sProperty.BottomExt;
                    _maxRanks.y = (int)((tmpHeight + _sProperty.HeightExt) / (_itemRect.rect.height + _sProperty.HeightExt));
                    _maxRanks.x = (int)(Math.Ceiling(_viewRect.rect.width / (_itemRect.rect.width + _sProperty.WidthExt)) + 1);
                    _sProperty.maxItems = _maxRanks.x * _maxRanks.y;
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
            _lookSize.x = (int)(_maxRanks.x * (_itemRect.rect.width + _sProperty.WidthExt) - _sProperty.WidthExt);
            _lookSize.y = (int)(_maxRanks.y * (_itemRect.rect.height + _sProperty.HeightExt) - _sProperty.HeightExt);
        }

        private void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            //不需要刷新
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
                    if (_tmpContentPos.y < _sProperty.TopExt || _tmpContentPos.y > 
                            _contentRect.rect.height - _viewRect.rect.height - _sProperty.BottomExt)
                        return;
                    while (_tmpContentPos.y + _viewRect.rect.height >
                        _gridLayoutGroup.padding.top + _lookSize.y + _sProperty.HeightExt)
                    {
                        for (int i = 0; i < _maxRanks.x; ++i)
                        {
                            int _tmpItemIdx = _itemIdx + i;
                            int tmpDataIdx = _dataIdx +_sProperty.maxItems + i;
                            _itemSet[_tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                            if (tmpDataIdx < _dataSet.Count)
                            {
                                if(_itemSet[_tmpItemIdx].gameObject.activeSelf == false)
                                    _itemSet[_tmpItemIdx].gameObject.SetActive(true);
                                _itemSet[_tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                            }          
                            else
                                _itemSet[_tmpItemIdx].gameObject.SetActive(false);
                            _itemSet[_tmpItemIdx].transform.SetAsLastSibling();
                        }

                        _itemIdx = _itemIdx + _maxRanks.x >= _sProperty.maxItems?0:_itemIdx + _maxRanks.x;
                        _dataIdx = _dataIdx + _maxRanks.x >= _dataSet.Count?_dataIdx: _dataIdx + _maxRanks.x;
                        _gridLayoutGroup.padding.top += _wholeSize.y;
                        _gridLayoutGroup.SetLayoutVertical();
                    }
                    while (_contentRect.anchoredPosition.y  <
                        _gridLayoutGroup.padding.top - _sProperty.HeightExt)
                    {
                        for (int i = 0; i < _maxRanks.x; ++i)
                        {

                            _gridLayoutTrans.GetChild(_sProperty.initItems - 1).SetAsFirstSibling();
                        }                           
                        _gridLayoutGroup.padding.top -= _wholeSize.y;
                        _gridLayoutGroup.SetLayoutVertical();
                    }
                    break;
                case ScrollDir.BottomToTop:
                    _gridLayoutGroup.SetLayoutVertical();
                    break;
                case ScrollDir.LeftToRight:
                    _gridLayoutGroup.SetLayoutHorizontal();
                    break;
                case ScrollDir.RightToLeft:
                    _gridLayoutGroup.SetLayoutHorizontal();
                    break;
            }

            _timer = 0;
        }

        private void OnResolveGroupEnum()
        {
            int sign = _sProperty.isCircleEnable ? 1 : 0;
            sign=(short)_sProperty.scrollDir*100 + (short)_sProperty.scrollSort * 10 +sign;
            switch (sign)
            {
                case 0:case 1:case 130:case 131:case 200:case 201:case 330:case 331:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
                    break;
                case 10:case 11:case 120:case 121:case 230:case 231:case 300:case 301:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperRight;
                    break;
                case 20:case 21:case 110:case 111:case 220:case 221:case 310:case 311:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerRight;
                    break;
                case 30:case 31:case 100:case 101:case 210:case 211:case 320:case 321:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerLeft;
                    break;
            }

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
                    _wholeSize.y = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = _sProperty.TopExt + _sProperty.BottomExt - _sProperty.HeightExt
                        + (float)Math.Ceiling((float)(_dataSet.Count / _maxRanks.x)) * _wholeSize.y;
                    break;
                case ScrollDir.BottomToTop:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    _wholeSize.y = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = _sProperty.TopExt + _sProperty.BottomExt - _sProperty.HeightExt
                        + (float)Math.Ceiling((float)(_dataSet.Count / _maxRanks.x)) * _wholeSize.y;
                    break;
                case ScrollDir.LeftToRight:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    _wholeSize.x = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt - _sProperty.WidthExt
                        + (float)Math.Ceiling((float)(_dataSet.Count / _maxRanks.y)) * _wholeSize.x;
                    break;
                case ScrollDir.RightToLeft:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    _wholeSize.x = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt - _sProperty.WidthExt
                        + (float)Math.Ceiling((float)(_dataSet.Count / _maxRanks.y)) * _wholeSize.x;
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
                _dataSet.AddRange(_tmpDataSet);
            switch (_sProperty.scrollSort)
            {
                case ScrollSort.FrontDir:
                case ScrollSort.FrontZDir:
                    _itemIdx = 0;_dataIdx = 0;
                    for (int i = 0; i < _sProperty.maxItems; ++i)
                    {
                        if (i >= _dataSet.Count) break;//表示没有数据
                        InitItem(i);
                    }
                    break;
                case ScrollSort.BackDir:
                case ScrollSort.BackZDir:
                    int firstItem = _dataSet.Count - _sProperty.maxItems;
                    firstItem = firstItem < 0 ? 0 : firstItem;
                    for (int i = firstItem; i < _dataSet.Count; ++i)
                        InitItem(i);
                    break;
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
        }

        public override void ResetItems()
        {
            _dataSet.Clear();
            _itemSet.Clear();
        }

        public override void AddItem(T data,int itemIdx = -1)
        {
            _dataSet.Add(data);
        }

        public override void ToTop(bool isDrawEnable = true)
        {

        }

        public override void ToBottom(bool isDrawEnable = true)
        {

        }

        public override void UpdateItem(T data, int itemIdx)
        {
            throw new NotImplementedException();
        }

        public override Vector4 GetLocationParam()
        {
            throw new NotImplementedException();
        }

        public override void ToLocation(Vector4 locationNode, bool isDrawEnable = true)
        {
            throw new NotImplementedException();
        }
    }
}