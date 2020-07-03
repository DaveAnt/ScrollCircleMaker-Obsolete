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
        /// 自适应的高宽
        /// </summary>
        protected float _contentSize
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return _contentRect.sizeDelta.y;
                    default:
                        return _contentRect.sizeDelta.x;
                }
            }
            set
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        _contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, value);
                        break;
                    default:
                        _contentRect.sizeDelta = new Vector2(value, _contentRect.sizeDelta.y);
                        break;
                }
            }
        }
        /// <summary>
        /// 边距拉伸
        /// </summary>
        protected int _contentStretch
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
        /// <summary>
        /// 上界限判断
        /// </summary>
        protected bool _lowerDefine
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        return _contentRect.anchoredPosition.y <= 1;
                    case ScrollDir.BottomToTop:
                        return _contentRect.anchoredPosition.y >= -1;
                    case ScrollDir.LeftToRight:
                        return _contentRect.anchoredPosition.x >= -1;
                    default:
                        return _contentRect.anchoredPosition.x <= 1;
                }
            }
        }
        /// <summary>
        /// 下界限判断
        /// </summary>
        protected bool _highDefine
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return Mathf.Abs(_contentRect.anchoredPosition.y) >=
                            (int)(_contentRect.rect.height - _viewRect.rect.height);
                    default:
                        return Mathf.Abs(_contentRect.anchoredPosition.x) >=
                            (int)(_contentRect.rect.width - _viewRect.rect.width);
                }
            }
        }
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
        /// 插件参数
        /// </summary>
        public ScrollCircleComponent sProperty
        {
            get{
                return _sProperty;
            }
        }
        /// <summary>
        /// 视图中心
        /// </summary>
        public int itemCore
        {
            get {
                return ((_sProperty.dataIdx + _sProperty.maxItems) / 2) % _dataSet.Count;
            }
        }
        /// <summary>
        /// Item数量
        /// </summary>
        public int dataCount
        {
            get {
                return _dataSet == null ? 0 : _dataSet.Count;
            }
        }
        /// <summary>
        /// Item实例数量
        /// </summary>
        public int itemCount
        {
            get {
                return _itemSet == null ? 0 : _itemSet.Count;
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
