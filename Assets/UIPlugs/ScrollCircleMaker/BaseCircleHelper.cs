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
    public abstract class BaseCircleHelper<T>
    {
        protected List<T> _dataSet;
        protected List<BaseItem<T>> _itemSet;
        protected Func<BaseItem<T>> _createItemFunc;
        protected Action _toLocationEvent;

        protected ScrollRect _scrollRect;
        protected RectTransform _viewRect, _contentRect, _itemRect;

        protected ScrollCircleComponent _sProperty;
        protected GameObject _baseItem;
        /// <summary>
        /// 动画结束回调
        /// </summary>
        public event Action toLocationEvent 
        {
            add {
                _toLocationEvent += value;
            }
            remove
            {
                _toLocationEvent -= value;
            }
        }
        /// <summary>
        /// 插件组件
        /// </summary>
        public ScrollCircleComponent sProperty
        {
            get{
                return _sProperty;
            }
        }
        /// <summary>
        /// Item数量
        /// </summary>
        public int itemCount
        {
            get {
                if (_dataSet == null)
                    return 0;
                else
                    return _dataSet.Count;
            }
        }
        /// <summary>
        /// 启动插件
        /// </summary>
        /// <param name="_tmpDataSet"></param>
        public abstract void OnStart(List<T> _tmpDataSet = null);
        /// <summary>
        /// 释放插件
        /// </summary>
        public virtual void OnDestroy()
        {
            foreach (var baseItem in _itemSet)
                baseItem.OnDestroy();
            _toLocationEvent = null;
            _createItemFunc = null;
            _scrollRect.onValueChanged.RemoveListener(OnRefreshHandler);
            _dataSet.Clear();
            _itemSet.Clear();
            GC.Collect();
        }
        /// <summary>
        /// item更新接口
        /// </summary>
        public virtual void OnUpdate()
        {
            if (_itemSet == null) return;
            foreach (BaseItem<T> baseItem in _itemSet)
                baseItem.OnUpdate();
        }
        /// <summary>
        /// 控制滑动开关
        /// </summary>
        /// <param name="state"></param>
        public virtual void OnSwitchSlide(bool state)
        {
            try{
                _scrollRect.enabled = state;
            }
            catch (Exception e){
                Debug.LogError("OnSwitchSlide Error!" + e.Message);
            }
        }
        /// <summary>
        /// 刷新方式
        /// </summary>
        /// <param name="v2"></param>
        protected abstract void OnRefreshHandler(Vector2 v2);
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="itemIdx">索引</param>
        public abstract void DelItem(int itemIdx);
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="seekFunc">匹配函数</param>
        /// <param name="data">移除数据</param>
        public abstract void DelItem(Func<T, T, bool> seekFunc, T data);
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="itemIdx">索引</param>
        public abstract void AddItem(T data, int itemIdx = -1);
        /// <summary>
        /// 更新Item样式
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="itemIdx">索引</param>
        public abstract void UpdateItem(T data,int itemIdx);
        /// <summary>
        /// 重置插件
        /// </summary>
        public abstract void ResetItems();
        /// <summary>
        /// 获取定位
        /// </summary>
        /// <returns>位置</returns>
        public abstract int GetLocation();
        /// <summary>
        /// 定位接口
        /// </summary>
        /// <param name="toSeat">位置</param>
        /// <param name="isDrawEnable">是否存在动画过程</param>
        public abstract void ToLocation(int toSeat, bool isDrawEnable = true);
        /// <summary>
        /// 置顶
        /// </summary>
        /// <param name="isDrawEnable">是否存在动画过程</param>
        public abstract void ToTop(bool isDrawEnable = true);
        /// <summary>
        /// 置底
        /// </summary>
        /// <param name="isDrawEnable">是否存在动画过程</param>
        public abstract void ToBottom(bool isDrawEnable = true);
    }
}
