//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using UnityEngine;
namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseItem<T>
    {
        protected T _itemData;
        protected int _globalSeat;
        protected Transform _transform;
        protected GameObject _gameObject;
        /// <summary>
        /// 更新数据
        /// </summary>
        public T itemData
        {
            get {
                return _itemData;
            }
        }
        /// <summary>
        /// 全局位置
        /// </summary>
        public int globalSeat
        {
            get {
                return _globalSeat;
            }
        }
        /// <summary>
        /// 物品位置组件
        /// </summary>
        public Transform transform
        {
            get {
                return _transform;
            }
        }
        /// <summary>
        /// 物品UI位置组件
        /// </summary>
        public RectTransform rectTrans
        {
            get
            {
                return _transform as RectTransform;
            }
        }
        /// <summary>
        /// 物品实例
        /// </summary>
        public GameObject gameObject
        {
            get {
                return _gameObject;
            }
            set
            {
                if (_gameObject == null)
                {
                    _gameObject = value;
                    _transform = _gameObject.transform;
                }
            }
        }
        /// <summary>
        /// 初始化组件
        /// </summary>
        public abstract void InitComponents();
        /// <summary>
        /// 初始化事件
        /// </summary>
        public abstract void InitEvents();
        /// <summary>
        /// 更新物品样式
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="globalSeat">全局位置</param>
        public virtual void UpdateView(T data, int globalSeat)
        {
            _itemData = data;
            _globalSeat = globalSeat;
        }
        /// <summary>
        /// 销毁
        /// </summary>
        public abstract void OnDestroy();
        /// <summary>
        /// 持续更新物品
        /// </summary>
        public virtual void OnUpdate() { }
    }
}