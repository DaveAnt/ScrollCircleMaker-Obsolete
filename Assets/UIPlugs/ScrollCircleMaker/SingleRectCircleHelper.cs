//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker       //单行矩形滑动循环  支持不规则长宽 聊天框，收纳框等等
{
    public class SingleRectCircleHelper<T> : BaseCircleHelper<T>
    {
        private HorizontalOrVerticalLayoutGroup _singleLayoutGroup;
        private BoundaryInt _cExtra;
        private SizeInt _wholeSize;
        private Vector2 _tmpContentPos;
        private bool _lockSlide, _firstRun;
        private float _timer = 0;
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
                    
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    
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
            OnAnchorSet();
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
                    contentSize.y = _sProperty.BottomExt + _sProperty.TopExt
                        + _dataSet.Count * _wholeSize.Height - _sProperty.WidthExt;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    contentSize.x = _sProperty.LeftExt + _sProperty.RightExt
                        + _dataSet.Count * _wholeSize.Width - _sProperty.WidthExt;
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
            
        }
        public override int GetLocation()
        {
            throw new NotImplementedException();
        }

        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {
            
        }

        public override void ToBottom(bool isDrawEnable = true)
        {
            
        }

        public override void ToTop(bool isDrawEnable = true)
        {
            
        }
    }
}
