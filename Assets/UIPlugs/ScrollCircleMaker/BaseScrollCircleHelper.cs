//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseScrollCircleHelper<T>
    {
        protected List<T> _dataSet;
        protected List<BaseItem<T>> _itemSet;
        protected Func<BaseItem<T>> _createItemFunc;

        protected ScrollRect _scrollRect;
        protected RectTransform _viewRect, _contentRect, _itemRect;

        protected ScrollCircleComponent _sProperty;
        protected GameObject _baseItem;
        public abstract void OnStart(List<T> _tmpDataSet = null);//启动
        public abstract void OnDestroy();//销毁
        public virtual void OnUpdate()
        {
            if (_itemSet == null) return;
            foreach (BaseItem<T> baseItem in _itemSet)
                baseItem.OnUpdate();
        }
        public virtual void OnSwitchlide(bool state)
        {
            try{
                _scrollRect.enabled = state;
            }
            catch (Exception e){
                Debug.LogError("_scrollRect.enabled = state" + e.Message);
            }
        }

        public abstract void AddItem(T data,int itemIdx = -1);//添加数据
        public abstract void UpdateItem(T data,int itemIdx);
        public abstract void ResetItems();//清空数据
        public abstract int GetLocation();
        public abstract void ToLocation(int toSeat, bool isDrawEnable = true);
        public abstract void ToTop(bool isDrawEnable = true);//置顶 true存在过程动画
        public abstract void ToBottom(bool isDrawEnable = true);//置底
    }
}
