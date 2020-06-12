//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using UnityEngine;
namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseItem<T>
    {
        protected Transform _transform;
        protected GameObject _gameObject;

        public RectTransform rectTrans
        {
            get {
                return _transform as RectTransform;
            }
        }

        public Transform transform
        {
            get {
                return _transform;
            }
        }

        public GameObject gameObject
        {
            get {
                return _gameObject;
            }
        }
        /// <summary>
        /// 设置Item关联transform
        /// </summary>
        /// <param name="transform">Item的Transform</param>
        public virtual void SetTransform(Transform transform)
        {
            _transform = transform;
            _gameObject = _transform.gameObject;
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
        /// 更新Item样式
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="globalSeat">位置信息 *单行辅助器仅有* </param>
        public abstract void UpdateView(T data,int? globalSeat = null);
        /// <summary>
        /// 销毁
        /// </summary>
        public abstract void OnDestroy();
        /// <summary>
        /// 持续更新样式
        /// </summary>
        public virtual void OnUpdate() { }
    }
}