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

namespace UIPlugs.ScrollCircleMaker
{
    public class CustomRectCircleHelper<T> : BaseCircleHelper<T> //自定义轨迹
    {
        public CustomRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc)
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
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                 
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    break;
            }
        }
        protected override void OnRefreshHandler(Vector2 v2)
        {

        }

        public override void OnStart(List<T> _tmpDataSet = null)
        {

        }

        public override void DelItem(int itemIdx)
        {

        }

        public override void DelItem(Func<T, T, bool> seekFunc, T data)
        {

        }

        public override void AddItem(T data, int itemIdx = -1)
        {

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
