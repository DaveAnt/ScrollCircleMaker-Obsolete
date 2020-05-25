//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker       //多行矩形滑动循环
{
    public struct BoundaryInt
    { 
        public short dir;//方向1或-1
        public int area;//最大显示区域高或宽
        public int length;//数据向上取整的大小
    }
    public class MultipleRectCircleHelper<T> : BaseScrollCircleHelper<T>
    {
        private GridLayoutGroup _gridLayoutGroup;
        private SizeInt _wholeSize, _maxRanks;
        private BoundaryInt _boundaryArea;
        private Vector2 _tmpContentPos;
        private bool _lockSlide, _firstRun;
        private float _timer = 0;
        
        private int _contentSite//偏移锚点
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

        private int _contentExt//偏移
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        return _sProperty.TopExt;
                    case ScrollDir.BottomToTop:
                        return _sProperty.BottomExt;
                    case ScrollDir.LeftToRight:
                        return _sProperty.LeftExt;
                    default:
                        return _sProperty.RightExt;
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
            if (_sProperty == null) Debug.LogError("Content must have ScrollCircleComponent!");

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
            _gridLayoutGroup.padding.left = _sProperty.LeftExt;
            _gridLayoutGroup.padding.right = _sProperty.RightExt;
            _gridLayoutGroup.padding.top = _sProperty.TopExt;
            _gridLayoutGroup.padding.bottom = _sProperty.BottomExt;
            _gridLayoutGroup.spacing = new Vector2(_sProperty.WidthExt, _sProperty.HeightExt);
            _gridLayoutGroup.cellSize = _itemRect.rect.size;
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
            switch (sign)
            {
                case 0:case 1:case 2:case 3:
                case 20:case 21:case 22:case 23:
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    break;
                case 10:case 11: case 12:case 13:
                    _gridLayoutGroup.childAlignment = TextAnchor.LowerLeft;
                    break;
                case 30: case 31:case 32:case 33:
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperRight;
                    break;
            }
        }

        private void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            if (_lockSlide) return;
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
        public override void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveListener(OnRefreshHandler);
            _dataSet.Clear();
            _itemSet.Clear();
            GC.Collect();
        }
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

        //内部接口
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

        private void ToItemAline(int tmpItemIdx)//对齐偏移
        { 
            for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                _itemSet[i].transform.SetAsLastSibling();
            for (int i = tmpItemIdx - 1; i >= _sProperty.itemIdx; --i)
                _itemSet[i].transform.SetAsFirstSibling();
        }

#if IsCircleEnable
        private void RefreshItems()
        {
            int tmpItemIdx, tmpDataIdx;
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                if (i > _itemSet.Count - 1) return;
                tmpDataIdx = (_sProperty.dataIdx + i) % _boundaryArea.length;
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
        }

        private void OnCircleVertical()
        {
            int tmpItemIdx, tmpDataIdx;
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Abs(_tmpContentPos.y);
            while (_tmpContentPos.y  > _contentSite  + _wholeSize.Height)//向下
            {
                for (int i = 0; i < _maxRanks.Width; ++i)
                {
                    tmpItemIdx = _sProperty.itemIdx + i;
                    tmpDataIdx = (_sProperty.dataIdx + _sProperty.maxItems + i) % _boundaryArea.length;
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
                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Width >= 
                    _dataSet.Count ? 0 : _sProperty.dataIdx + _maxRanks.Width;
                _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Width >=
                    _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Width;
                _contentSite += _wholeSize.Height;
                _gridLayoutGroup.SetLayoutVertical();
            }
            while (_tmpContentPos.y < _contentSite - _sProperty.HeightExt)
            {
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width < 0 ?
                    _boundaryArea.length - _maxRanks.Width : _sProperty.dataIdx - _maxRanks.Width;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width < 0 ?
                    _sProperty.maxItems - _maxRanks.Width : _sProperty.itemIdx - _maxRanks.Width;

                for (int i = _maxRanks.Width - 1; i >= 0; --i)
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
                _contentSite -= _wholeSize.Height;
                _gridLayoutGroup.SetLayoutVertical();
            }

            //强制上定位
            if (_tmpContentPos.y >= _contentRect.rect.height - _viewRect.rect.height)
            {
                Vector2 tmpVelocity = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.itemIdx = _sProperty.dataIdx = 0;
                _contentSite = (int)_viewRect.rect.height + _sProperty.HeightExt;
                _tmpContentPos.y = (int)(_viewRect.rect.height + _sProperty.HeightExt) * _boundaryArea.dir;
                _contentRect.anchoredPosition = _tmpContentPos;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemAline(tmpItemIdx);
                RefreshItems();              
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpVelocity;
            }

            //强制下定位
            else if (_tmpContentPos.y <= 1)
            {
                Vector2 tmpVelocity = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _boundaryArea.length - _sProperty.maxItems;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.maxItems;
                _contentSite = (int)_viewRect.rect.height + _sProperty.HeightExt + _sProperty.dataIdx * _wholeSize.Height / _maxRanks.Width;
                _tmpContentPos.y = _boundaryArea.area * _boundaryArea.dir;
                _contentRect.anchoredPosition = _tmpContentPos;
                _gridLayoutGroup.SetLayoutVertical();
                ToItemAline(tmpItemIdx);
                RefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpVelocity;
            }
        }

        private void OnCircleHorizontal()
        {
            int tmpItemIdx, tmpDataIdx;
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = Mathf.Abs(_tmpContentPos.x);
            while (_tmpContentPos.x > _contentSite  + _sProperty.WidthExt)//向下
            {
                for (int i = 0; i < _maxRanks.Height; ++i)
                {
                    tmpItemIdx = _sProperty.itemIdx + i;
                    tmpDataIdx = (_sProperty.dataIdx + _sProperty.maxItems + i) % _boundaryArea.length;
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
                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Height >=
                    _dataSet.Count ? 0 : _sProperty.dataIdx + _maxRanks.Height;
                _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Height >=
                    _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Height;
                _contentSite += _wholeSize.Width;
                _gridLayoutGroup.SetLayoutHorizontal();
            }
            while (_tmpContentPos.x < _contentSite - _sProperty.WidthExt)
            {
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height < 0 ?
                    _boundaryArea.length - _maxRanks.Height : _sProperty.dataIdx - _maxRanks.Height;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height < 0 ?
                    _sProperty.maxItems - _maxRanks.Height : _sProperty.itemIdx - _maxRanks.Height;

                for (int i = _maxRanks.Height - 1; i >= 0; --i)
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
                _contentSite -= _wholeSize.Width;
                _gridLayoutGroup.SetLayoutHorizontal();
            }
        }

        private void OnAnchorSet()
        {
            _boundaryArea.dir = 1;
            _sProperty.initItems = _itemSet.Count;
            _lockSlide = _sProperty.maxItems >= _dataSet.Count;
            Vector2 contentSize = _contentRect.sizeDelta;
            Vector2 contentPosition = _contentRect.anchoredPosition;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);   
                    _boundaryArea.area = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _wholeSize.Height;
                    _boundaryArea.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _maxRanks.Width;
                    contentSize.y = _boundaryArea.area - _sProperty.HeightExt;
                    if (contentSize.y >= _viewRect.rect.height){
                        contentSize.y += 2 * (_viewRect.rect.height + _sProperty.HeightExt);
                        contentPosition.y = (int)_viewRect.rect.height + _sProperty.HeightExt;
                        _gridLayoutGroup.padding.top = (int)_viewRect.rect.height + _sProperty.HeightExt;
                        _contentRect.anchoredPosition = contentPosition;
                    }else
                        contentSize.y = contentSize.y + _sProperty.TopExt + _sProperty.BottomExt;
                    break;
                case ScrollDir.BottomToTop:
                    _boundaryArea.dir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    _boundaryArea.area = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _wholeSize.Height;
                    _boundaryArea.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _maxRanks.Width;                   
                    contentSize.y = _boundaryArea.area - _sProperty.HeightExt;
                    if (contentSize.y >= _viewRect.rect.height){
                        contentSize.y += 2 * (_viewRect.rect.height + _sProperty.HeightExt);
                        contentPosition.y = -(int)_viewRect.rect.height - _sProperty.HeightExt;
                        _gridLayoutGroup.padding.bottom = (int)_viewRect.rect.height + _sProperty.HeightExt;
                        _contentRect.anchoredPosition = contentPosition;
                    }else
                        contentSize.y = contentSize.y + _sProperty.TopExt + _sProperty.BottomExt;               
                    break;
                case ScrollDir.LeftToRight:
                    _boundaryArea.dir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);              
                    _boundaryArea.area = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _wholeSize.Width;
                    _boundaryArea.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _maxRanks.Height;                
                    contentSize.x = _boundaryArea.area - _sProperty.WidthExt;
                    if (contentSize.x >= _viewRect.rect.width){
                        contentSize.x += 2 * (_viewRect.rect.width + _sProperty.WidthExt);
                        contentPosition.x = -(int)_viewRect.rect.width - _sProperty.WidthExt;
                        _gridLayoutGroup.padding.left = (int)_viewRect.rect.width + _sProperty.WidthExt;
                        _contentRect.anchoredPosition = contentPosition;                       
                    }else
                        contentSize.x = contentSize.x + _sProperty.LeftExt + _sProperty.RightExt;                     
                    break;
                case ScrollDir.RightToLeft:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    _boundaryArea.area = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _wholeSize.Width;
                    _boundaryArea.length = (int)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _maxRanks.Height;                   
                    contentSize.x = _boundaryArea.area - _sProperty.WidthExt;
                    if (contentSize.x >= _viewRect.rect.width){
                        contentSize.x += 2 * (_viewRect.rect.width + _sProperty.WidthExt);
                        contentPosition.x = (int)_viewRect.rect.width + _sProperty.WidthExt;
                        _gridLayoutGroup.padding.right = (int)_viewRect.rect.width + _sProperty.WidthExt;
                        _contentRect.anchoredPosition = contentPosition;
                    }else
                        contentSize.x = contentSize.x + _sProperty.LeftExt + _sProperty.RightExt;
                    break;
            }
            _contentRect.sizeDelta = contentSize;
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
            int tmpItemIdx, tmpDataIdx;
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = Mathf.Clamp(_tmpContentPos.y *
                _boundaryArea.dir, 0,  _boundaryArea.area);
            while (_tmpContentPos.y + _contentExt >
                _contentSite + _wholeSize.Height)//向下
            {
                if (_sProperty.dataIdx + _maxRanks.Width >= _dataSet.Count) break;

                for (int i = 0; i < _maxRanks.Width; ++i)
                {
                    tmpItemIdx = _sProperty.itemIdx + i;
                    tmpDataIdx = _sProperty.dataIdx + _sProperty.maxItems + i;
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

                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Width;
                _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Width >=
                    _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Width;
                _contentSite += _wholeSize.Height;
                _gridLayoutGroup.SetLayoutVertical();
            }
            while (_tmpContentPos.y < _contentSite - _sProperty.HeightExt)
            {
                if (_sProperty.dataIdx <= 0) break;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Width < 0 ?
                    _sProperty.maxItems - _maxRanks.Width : _sProperty.itemIdx - _maxRanks.Width;

                for (int i = _maxRanks.Width - 1; i >= 0; --i)
                {
                    tmpItemIdx = _sProperty.itemIdx + i;
                    tmpDataIdx = _sProperty.dataIdx + i;
                    _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                    if (tmpDataIdx >= 0)
                    {
                        if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                            _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                        _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                    }
                    _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                }
                _contentSite -= _wholeSize.Height;
                _gridLayoutGroup.SetLayoutVertical();
            }
        }

        private void OnCircleHorizontal()
        {
            int tmpItemIdx, tmpDataIdx;
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = Mathf.Clamp(_tmpContentPos.x * 
                _boundaryArea.dir,0, _boundaryArea.area);
            while (_tmpContentPos.x + _contentExt >
                    _contentSite + _wholeSize.Width)
            {
                if (_sProperty.dataIdx + _maxRanks.Height >= _dataSet.Count) break;

                for (int i = 0; i < _maxRanks.Height; ++i)
                {
                    tmpItemIdx = _sProperty.itemIdx + i;
                    tmpDataIdx = _sProperty.dataIdx + _sProperty.maxItems + i;
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

                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Height;
                _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Height >=
                    _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Height;
                _contentSite += _wholeSize.Width;
                _gridLayoutGroup.SetLayoutHorizontal();
            }
            while (_tmpContentPos.x < _contentSite - _sProperty.WidthExt)
            {
                if (_sProperty.dataIdx <= 0) break;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height;
                _sProperty.itemIdx = _sProperty.itemIdx - _maxRanks.Height < 0 ?
                    _sProperty.maxItems - _maxRanks.Height : _sProperty.itemIdx - _maxRanks.Height;

                for (int i = _maxRanks.Height - 1; i >= 0; --i)
                {
                     tmpItemIdx = _sProperty.itemIdx + i;
                     tmpDataIdx = _sProperty.dataIdx + i;
                    _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                    if (tmpDataIdx >= 0)
                    {
                        if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                            _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                        _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                    }
                    _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                }
                _contentSite -= _wholeSize.Width;
                _gridLayoutGroup.SetLayoutHorizontal();
            }
        }

        private IEnumerator ToAutoMoveVSeat(int toSeat)
        {           
            if(_boundaryArea.area <= 0) yield break;
            toSeat = Mathf.Clamp(toSeat, 0, _boundaryArea.area);

            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat + _contentExt >
                _contentSite + _wholeSize.Height)
            {
                if (_sProperty.dataIdx + _maxRanks.Width >= _dataSet.Count)
                {
                    _scrollRect.enabled = true;
                    yield break;
                }

                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _boundaryArea.dir < toSeat)
                {         
                    _tmpContentPos.y = _tmpContentPos.y + _boundaryArea.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _scrollRect.enabled = true;
                    yield break;
                }

                if (Mathf.Abs(_tmpContentPos.y) +  _contentExt>
                    _contentSite + _wholeSize.Height)
                {
                    for (int i = 0; i < _maxRanks.Width; ++i)
                    {
                        int tmpItemIdx = _sProperty.itemIdx + i;
                        int tmpDataIdx = _sProperty.dataIdx + _sProperty.maxItems + i;
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

                    _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Width;
                    _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Width >=
                        _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Width;
                    _contentSite += _wholeSize.Height;
                    _gridLayoutGroup.SetLayoutVertical();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.HeightExt)
            {
                if (_sProperty.dataIdx <= 0)
                {
                    _scrollRect.enabled = true;
                    yield break;
                }

                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.y * _boundaryArea.dir > toSeat)
                {            
                    _tmpContentPos.y =_tmpContentPos.y - _boundaryArea.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _scrollRect.enabled = true;
                    yield break;
                }

                if (_tmpContentPos.y * _boundaryArea.dir < _contentSite - _sProperty.HeightExt)
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
                            if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                                _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                            _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                        }
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                    }
                    _contentSite -= _wholeSize.Height;
                    _gridLayoutGroup.SetLayoutVertical();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.y = toSeat * _boundaryArea.dir;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
        }

        private IEnumerator ToAutoMoveHSeat(int toSeat)
        {
            if (_boundaryArea.area <= 0) yield break;
            toSeat = Mathf.Clamp(toSeat, 0, _boundaryArea.area);
            _scrollRect.enabled = false;
            yield return new WaitForEndOfFrame();
            while (toSeat + _contentExt >
                    _contentSite + _wholeSize.Width)
            {
                if (_sProperty.dataIdx + _maxRanks.Height >= _dataSet.Count) {
                    _scrollRect.enabled = true;
                    yield break;
                }

                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _boundaryArea.dir < toSeat){
                    _tmpContentPos.x = _tmpContentPos.x + _boundaryArea.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else {
                    _scrollRect.enabled = true;
                    yield break;
                }              

                if (Mathf.Abs(_tmpContentPos.x) + _contentExt >
                    _contentSite + _wholeSize.Width)
                {
                    for (int i = 0; i < _maxRanks.Height; ++i)
                    {
                        int tmpItemIdx = _sProperty.itemIdx + i;
                        int tmpDataIdx = _sProperty.dataIdx + _sProperty.maxItems + i;
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

                    _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Height;
                    _sProperty.itemIdx = _sProperty.itemIdx + _maxRanks.Height >=
                        _sProperty.maxItems ? 0 : _sProperty.itemIdx + _maxRanks.Height;
                    _contentSite += _wholeSize.Width;
                    _gridLayoutGroup.SetLayoutHorizontal();
                    yield return new WaitForEndOfFrame();
                }
            }
            while (toSeat < _contentSite - _sProperty.WidthExt)
            {
                if (_sProperty.dataIdx <= 0) {
                    _scrollRect.enabled = true;
                    yield break;
                }

                _tmpContentPos = _contentRect.anchoredPosition;
                if (_tmpContentPos.x * _boundaryArea.dir > toSeat){
                    _tmpContentPos.x = _tmpContentPos.x - _boundaryArea.dir * _sProperty.autoMoveRatio;
                    _contentRect.anchoredPosition = _tmpContentPos;
                    yield return new WaitForEndOfFrame();
                }
                else{
                    _scrollRect.enabled = true;
                    yield break;
                }

                if (_tmpContentPos.x * _boundaryArea.dir < _contentSite - _sProperty.WidthExt)
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
                            if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                                _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                            _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                        }
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                    }
                    _contentSite -= _wholeSize.Width;
                    _gridLayoutGroup.SetLayoutHorizontal();
                    yield return new WaitForEndOfFrame();
                }
            }
            _tmpContentPos.x = toSeat * _boundaryArea.dir;
            _contentRect.anchoredPosition = _tmpContentPos;
            _scrollRect.enabled = true;
        }

        private void ToDirectVSeat(int toSeat)
        {
            if (_boundaryArea.area <= 0) return;
            toSeat = Mathf.Clamp(toSeat, 0, _boundaryArea.area);
   
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.y = toSeat * _boundaryArea.dir;
            _contentRect.anchoredPosition = _tmpContentPos;

            int tmpItemIdx, tmpRow;
            if (Mathf.Abs(_tmpContentPos.y) > _contentSite + _wholeSize.Height)//向下
            {
                tmpItemIdx = _sProperty.itemIdx;
                tmpRow = (int)(Mathf.Abs(_tmpContentPos.y) - _contentSite) / _wholeSize.Height;
                _contentSite += _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Width * tmpRow;
                _sProperty.itemIdx = (tmpItemIdx + _maxRanks.Width * tmpRow) % _sProperty.maxItems;
                ToItemAline(tmpItemIdx);
            }
            else if (Mathf.Abs(_tmpContentPos.y) < _contentSite - _sProperty.HeightExt)
            {
                tmpItemIdx = _sProperty.itemIdx;
                tmpRow = (int)Math.Ceiling((_contentSite - Mathf.Abs(_tmpContentPos.y)
                    - _contentExt) / _wholeSize.Height);
                _contentSite -= _wholeSize.Height * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Width * tmpRow;
                _sProperty.itemIdx = tmpItemIdx - _maxRanks.Width * tmpRow;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems - _maxRanks.Width : _sProperty.itemIdx;
                ToItemAline(tmpItemIdx);
            }

            RefreshItems();
            _gridLayoutGroup.SetLayoutVertical();
        }

        private void ToDirectHSeat(int toSeat)
        {
            if (_boundaryArea.area <= 0) return;
            toSeat = Mathf.Clamp(toSeat, 0, _boundaryArea.area);
            _tmpContentPos = _contentRect.anchoredPosition;
            _tmpContentPos.x = toSeat * _boundaryArea.dir;
            _contentRect.anchoredPosition = _tmpContentPos;

            int tmpItemIdx, tmpColumn;
            if (Mathf.Abs(_tmpContentPos.x) > _contentSite + _wholeSize.Width)//向下
            {
                tmpItemIdx = _sProperty.itemIdx;
                tmpColumn = (int)(Mathf.Abs(_tmpContentPos.x) - _contentSite) / _wholeSize.Width;
                _contentSite += _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = _sProperty.dataIdx + _maxRanks.Height * tmpColumn;
                _sProperty.itemIdx = (tmpItemIdx + _maxRanks.Height * tmpColumn) % _sProperty.maxItems;
                ToItemAline(tmpItemIdx);
            }
            else if (Mathf.Abs(_tmpContentPos.x) < _contentSite - _sProperty.HeightExt)
            {
                tmpItemIdx = _sProperty.itemIdx;
                tmpColumn = (int)Math.Ceiling((_contentSite - Mathf.Abs(_tmpContentPos.x)
                    - _contentExt) / _wholeSize.Width);
                _contentSite -= _wholeSize.Width * tmpColumn;
                _sProperty.dataIdx = _sProperty.dataIdx - _maxRanks.Height * tmpColumn;
                _sProperty.itemIdx = tmpItemIdx - _maxRanks.Height * tmpColumn;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.maxItems - _maxRanks.Height : _sProperty.itemIdx;
                ToItemAline(tmpItemIdx);
            }

            RefreshItems();
            _gridLayoutGroup.SetLayoutHorizontal();
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
                    _boundaryArea.dir = 1;
                    _boundaryArea.area = (int)(contentSize.y - _viewRect.rect.height);
                    break;
                case ScrollDir.BottomToTop:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    _wholeSize.Height = (int)(_itemRect.rect.height + _sProperty.HeightExt);
                    contentSize.y = _sProperty.TopExt + _sProperty.BottomExt - _sProperty.HeightExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Width) * _wholeSize.Height;
                    _boundaryArea.dir = -1;
                    _boundaryArea.area = (int)(contentSize.y - _viewRect.rect.height);
                    break;
                case ScrollDir.LeftToRight:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt - _sProperty.WidthExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _wholeSize.Width;
                    _boundaryArea.dir = -1;
                    _boundaryArea.area = (int)(contentSize.x - _viewRect.rect.width);
                    break;
                case ScrollDir.RightToLeft:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    _wholeSize.Width = (int)(_itemRect.rect.width + _sProperty.WidthExt);
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt - _sProperty.WidthExt
                        + (float)Math.Ceiling(_dataSet.Count / (float)_maxRanks.Height) * _wholeSize.Width;
                    _boundaryArea.dir = 1;
                    _boundaryArea.area = (int)(contentSize.x - _viewRect.rect.width);
                    break;
            }
            _contentRect.sizeDelta = contentSize;
        }

        private void RefreshItems()
        {
            int tmpItemIdx,tmpDataIdx;
            for (int i = 0; i < _sProperty.maxItems; ++i)
            {
                if (i > _itemSet.Count - 1) return;
                tmpDataIdx = _sProperty.dataIdx + i;
                tmpItemIdx = (_sProperty.itemIdx + i) % _sProperty.maxItems;             
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                if (_sProperty.dataIdx + i >= 0 &&
                    _sProperty.dataIdx + i < _dataSet.Count)
                {
                    if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                        _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx]);
                }
                else
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
            }
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
            ToLocation(_boundaryArea.area, isDrawEnable);
        }
#endif
    }
}