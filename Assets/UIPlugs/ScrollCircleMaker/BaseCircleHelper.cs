//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private PointerEventData _eventData;

        /// <summary>
        /// 定位动画结束回调
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
            get {
                return _sProperty;
            }
        }
        /// <summary>
        /// 物品中心
        /// </summary>
        public int itemCore
        {
            get {
                return (_sProperty.dataIdx + _itemSet.Count / 2) % _dataSet.Count;
            }
        }
        /// <summary>
        /// 视图中心
        /// </summary>
        public int viewCore
        {
            get {
                return _dataSet.Count / 2;
            }
        }
        
        /// <summary>
        /// 物品数据数量
        /// </summary>
        public int dataCount
        {
            get {
                return _dataSet == null ? 0 : _dataSet.Count;
            }
        }
        /// <summary>
        /// 物品实例数量
        /// </summary>
        public int itemCount
        {
            get {
                return _itemSet == null ? 0 : _itemSet.Count;
            }
        }
        /// <summary>
        /// 滑动构造基类
        /// </summary>
        /// <param name="contentTrans">包含物品父组件</param>
        /// <param name="createItemFunc">创建物品函数</param>
        protected BaseCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc)
        {
            _createItemFunc = createItemFunc;
            _contentRect = contentTrans as RectTransform;
            _viewRect = contentTrans.parent.GetComponent<RectTransform>();
            _scrollRect = _viewRect.parent.GetComponent<ScrollRect>();
            _sProperty = _contentRect.GetComponent<ScrollCircleComponent>();
            _eventData = new PointerEventData(EventSystem.current);
            if (_sProperty == null)
                throw new Exception("Content must have ScrollCircleComponent!");
            _baseItem = _sProperty.baseItem;
            _itemRect = _baseItem.transform.GetComponent<RectTransform>();
            _scrollRect.onValueChanged.AddListener(OnRefreshHandler);
            _itemSet = new List<BaseItem<T>>();
            _dataSet = new List<T>();
        }
        /// <summary>
        /// 启动插件
        /// </summary>
        /// <param name="tmpDataSet">物品数据列表</param>
        public virtual void OnStart(List<T> tmpDataSet = null)
        {
            _firstRun = true;
            lockRefresh = _sProperty.initItems >= _dataSet.Count;
            if (tmpDataSet != null)
            {
                switch (_sProperty.scrollSort)
                {
                    case ScrollSort.BackDir:
                    case ScrollSort.BackZDir:
                        tmpDataSet.Reverse();
                        break;
                }
                _dataSet.AddRange(tmpDataSet);
            }
            for (int i = 0; i < _sProperty.initItems; ++i)
                InitItem(i);
        }
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
        /// 重置插件
        /// </summary>
        public virtual void ResetItems()
        {
            _firstRun = false;
            foreach (var baseItem in _itemSet)
                baseItem.OnDestroy();
            contentSite = (int)topSeat;
            contentRect = 0;
            nowSeat = 0;
            _dataSet.Clear();
            _itemSet.Clear();
            GC.Collect();       
        }
        /// <summary>
        /// 持续更新物品样式
        /// </summary>
        public virtual void OnUpdate()
        {
            if (_itemSet == null) return;
            foreach (BaseItem<T> baseItem in _itemSet)
                baseItem.OnUpdate();
        }
        /// <summary>
        /// 锁定滑动
        /// </summary>
        /// <param name="lockStatus">开关滑动</param>
        public virtual void OnSlideLockout(bool lockStatus)
        {
            try {
                _scrollRect.enabled = lockStatus;
            }
            catch (Exception e) {
                throw new Exception("OnSlideLockout Error!" + e.Message);
            }
        }
        /// <summary>
        /// 刷新方式
        /// </summary>
        /// <param name="v2">偏移间距</param>
        protected virtual void OnRefreshHandler(Vector2 v2)
        {
            if (lockRefresh)
                return;
            if (_timer < _sProperty.refreshRatio)
            {
                _timer += Time.deltaTime;
                return;
            }
            _timer = 0;
            if (_sProperty.scrollType == ScrollType.Limit
                && _scrollRect.velocity != Vector2.zero)
            {
                if(nowSeat >= 0 && nowSeat <= footSeat)
                    ToLocation(nowSeat + _sProperty.stepLen * slideDir);
                _scrollRect.velocity = Vector2.zero;
                return;
            }                
        }
        /// <summary>
        /// 移除物品数据
        /// </summary>
        /// <param name="itemIdx">物品索引</param>
        public abstract void DelItem(int itemIdx);
        /// <summary>
        /// 移除物品数据
        /// </summary>
        /// <param name="seekFunc">匹配物品函数</param>
        /// <param name="data">移除物品数据</param>
        public virtual void DelItem(Func<T, T, bool> seekFunc, T data)
        {
            int itemIdx = -1;
            for (int i = _dataSet.Count - 1; i >= 0; --i)
            {
                if (seekFunc(data, _dataSet[i]))
                {
                    itemIdx = i;
                    break;
                }
            }
            if (itemIdx == -1)
            {
                Debug.LogWarning("DelItem SeekFunc Fail!");
                return;
            }
            DelItem(itemIdx);
        }
        /// <summary>
        /// 添加物品数据
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="itemIdx">物品索引</param>
        public abstract void AddItem(T data, int itemIdx = int.MaxValue);
        /// <summary>
        /// 更新物品样式
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="itemIdx">物品索引</param>
        public abstract void UpdateItem(T data, int itemIdx);
        /// <summary>
        /// 交换物品位置
        /// </summary>
        /// <param name="firstIdx">被交换物品索引</param>
        /// <param name="nextIdx">交换物品索引</param>
        public abstract void SwapItem(int firstIdx, int nextIdx);
        /// <summary>
        /// 交换物品位置
        /// </summary>
        /// <param name="seekFunc">匹配查询物品</param>
        /// <param name="firstData">被交换物品数据</param>
        /// <param name="nextData">交换物品数据</param>
        public virtual void SwapItem(Func<T, T, bool> seekFunc, T firstData, T nextData)
        {
            int firstIdx = -1, nextIdx = -1;
            for (int i = _dataSet.Count - 1; i >= 0; --i)
            {
                if (seekFunc(firstData, _dataSet[i]))
                {
                    firstIdx = i;
                    if (nextIdx != -1)
                        break;
                }

                if (seekFunc(nextData, _dataSet[i]))
                {
                    nextIdx = i;
                    if (firstIdx != -1)
                        break;
                }
            }
            if (nextIdx == -1 || firstIdx == -1)
            {
                Debug.LogWarning("SwapItem SeekFunc Fail!");
                return;
            }
            SwapItem(firstIdx, nextIdx);
        }
        /// <summary>
        /// 初始化物品
        /// </summary>
        /// <param name="itemIdx">位置索引</param>
        protected virtual void InitItem(int itemIdx)
        {
            BaseItem<T> baseItem = _createItemFunc();
            baseItem.gameObject = GameObject.Instantiate(_baseItem, _contentRect);
            baseItem.gameObject.name = _baseItem.name + itemIdx;
            baseItem.transform.localScale = Vector3.zero;
            baseItem.InitComponents();
            baseItem.InitEvents();
            _itemSet.Add(baseItem);
        }
        /// <summary>
        /// 修正拖动位置
        /// </summary>
        protected void RefurbishMousePos()
        {
            if (Input.touchCount != 0 || _sProperty.isDargging)
            {
                _eventData.position = Input.mousePosition;
                _scrollRect.OnEndDrag(_eventData);
                _scrollRect.OnBeginDrag(_eventData);
            }
        }
        /// <summary>
        /// 真实位置定位
        /// </summary>
        /// <param name="toSeat">真实位置</param>
        /// <param name="isDrawEnable">是否存在动画过程</param>
        public abstract void ToLocation(float toSeat, bool isDrawEnable = true);
        /// <summary>
        /// 数据索引定位
        /// </summary>
        /// <param name="toIndex">数据索引</param>
        /// <param name="isDrawEnable">是否存在动画过程</param>
        public abstract void ToLocation(int toIndex, bool isDrawEnable = true);
        /// <summary>
        /// 数据匹配定位
        /// </summary>
        /// <param name="seekFunc">匹配物品函数</param>
        /// <param name="isDrawEnable">是否存在动画过程</param>
        public virtual void ToLocation(Func<T, T, bool> seekFunc, T data, bool isDrawEnable = true)
        {
            for (int i = _dataSet.Count - 1; i >= 0; ++i)
            {
                if (seekFunc(data, _dataSet[i]))
                {
                    ToLocation(i, isDrawEnable);
                    break;
                }
            }
            Debug.LogWarning("ToLocation SeekFunc Fail!" + data);
        }
        /// <summary>
        /// 置顶定位
        /// </summary>
        /// <param name="isDrawEnable">是否存在动画过程</param>
        public virtual void ToTop(bool isDrawEnable = true)
        {
            ToLocation(topSeat, isDrawEnable);
        }
        /// <summary>
        /// 置底定位
        /// </summary>
        /// <param name="isDrawEnable">是否存在动画过程</param>
        public virtual void ToBottom(bool isDrawEnable = true)
        {
            ToLocation(bottomSeat, isDrawEnable);
        }
        #region 辅助器内置共需属性
        /// <summary>
        /// 刷新速率
        /// </summary>
        protected float _timer = 0;
        /// <summary>
        /// 缓存位置
        /// </summary>
        protected Vector2 _cacheSeat;
        /// <summary>
        /// 布局基类
        /// </summary>
        protected LayoutGroup _layoutGroup;
        /// <summary>
        /// 滑动反向
        /// </summary>
        protected sbyte _frontDir = 1;
        /// <summary>
        /// 刷新状态、首次使用
        /// </summary>
        protected bool _lockRefresh = false,_firstRun = false;
        /// <summary>
        /// 禁止刷新
        /// </summary>
        protected bool lockRefresh
        {
            get {
                return _lockRefresh;
            }
            set {
                _lockRefresh = value && !_sProperty.isCircleEnable;
            }
        }
        /// <summary>
        /// 包含物品自适应高宽
        /// </summary>
        protected float contentRect
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        return _contentRect.rect.height;
                    default:
                        return _contentRect.rect.width;
                }
            }
            set
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        _contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, value);
                        break;
                    default:
                        _contentRect.sizeDelta = new Vector2(value, _contentRect.sizeDelta.y);
                        break;
                }
            }
        }
        /// <summary>
        /// 视图自适应高宽
        /// </summary>
        protected float viewRect
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        return _viewRect.rect.height;
                    default:
                        return _viewRect.rect.width;
                }
            }
        }
        /// <summary>
        /// 偏移锚点
        /// </summary>
        protected int contentSite
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom: return _layoutGroup.padding.top;
                    case ScrollDir.BottomToTop: return _layoutGroup.padding.bottom;
                    case ScrollDir.LeftToRight: return _layoutGroup.padding.left;
                    default: return _layoutGroup.padding.right;
                }
            }
            set
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom: _layoutGroup.padding.top = value; break;
                    case ScrollDir.BottomToTop: _layoutGroup.padding.bottom = value; break;
                    case ScrollDir.LeftToRight: _layoutGroup.padding.left = value; break;
                    default: _layoutGroup.padding.right = value; break;
                }
            }
        }
        /// <summary>
        /// 真实位置
        /// </summary>
        protected float nowSeat
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true: return  _contentRect.anchoredPosition.y * _frontDir;
                    default: return  _contentRect.anchoredPosition.x * _frontDir;
                }
            }
            set
            {
                _cacheSeat = _contentRect.anchoredPosition;
                switch (_scrollRect.vertical)
                {
                    case true: _cacheSeat.y = value * _frontDir; break;
                    default: _cacheSeat.x = value * _frontDir; break;
                }
                _contentRect.anchoredPosition = _cacheSeat;
                RefurbishMousePos();
            }
        }
        /// <summary>
        /// 扩展顶部
        /// </summary>
        protected float topSeatExt
        {
            get
            {
                return topSeat + spacingExt;
            }
        }
        /// <summary>
        /// 顶部位置
        /// </summary>
        protected float topSeat
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        if (_sProperty.isCircleEnable)
                            return _viewRect.rect.height;
                        return _sProperty.TopExt;
                    case ScrollDir.BottomToTop:
                        if (_sProperty.isCircleEnable)
                            return _viewRect.rect.height;
                        return _sProperty.BottomExt;
                    case ScrollDir.LeftToRight:
                        if (_sProperty.isCircleEnable)
                            return _viewRect.rect.width;
                        return _sProperty.LeftExt;
                    default:
                        if (_sProperty.isCircleEnable)
                            return _viewRect.rect.width;
                        return _sProperty.RightExt;
                }
            }
        }
        /// <summary>
        /// 底部位置
        /// </summary>
        protected float bottomSeat
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        if(_sProperty.isCircleEnable)
                            return _contentRect.rect.height - _viewRect.rect.height - _viewRect.rect.height;
                        return _contentRect.rect.height - _viewRect.rect.height;
                    default:
                        if (_sProperty.isCircleEnable)
                            return _contentRect.rect.width - _viewRect.rect.width - _viewRect.rect.width;
                        return _contentRect.rect.width - _viewRect.rect.width;
                }
            }
        }
        /// <summary>
        /// 真实底部
        /// </summary>
        protected float footSeat
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return _contentRect.rect.height - _viewRect.rect.height;
                    default:
                        return _contentRect.rect.width - _viewRect.rect.width;    
                }
            }
        }
        /// <summary>
        /// 物品间距
        /// </summary>
        protected int spacingExt
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        return _sProperty.HeightExt;
                    default:
                        return _sProperty.WidthExt;
                }
            }
        }
        /// <summary>
        /// 上下扩展间距
        /// </summary>
        protected float contentBorder
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        if (_sProperty.isCircleEnable)
                            return 2 * _viewRect.rect.height;
                        return _sProperty.TopExt + _sProperty.BottomExt;
                    default:
                        if (_sProperty.isCircleEnable)
                            return 2 * _viewRect.rect.width;
                        return _sProperty.LeftExt + _sProperty.RightExt;
                }
            }
        }
        /// <summary>
        /// 滑动方向
        /// </summary>
        protected int slideDir
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                    case ScrollDir.BottomToTop:
                        if (_scrollRect.velocity.y == 0)
                            return 0;
                        if (_scrollRect.velocity.y > 0)
                            return _frontDir;
                        return -_frontDir;
                    default:
                        if (_scrollRect.velocity.x == 0)
                            return 0;
                        if (_scrollRect.velocity.x > 0)
                            return _frontDir;
                        return -_frontDir;
                }
            }
        }
        /// <summary>
        /// 上区域判断
        /// </summary>
        protected bool isLowerDefine
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
        /// 下区域判断
        /// </summary>
        protected bool isHighDefine
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
        /// 处于循环状态
        /// </summary>
        protected bool isCircleStatus
        {
            get
            {
                return _sProperty.isCircleEnable && _sProperty.dataIdx 
                    + _sProperty.initItems > _dataSet.Count;
            }
        }
        #endregion
    }
}