//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseItem<T>
    {
        protected Transform _transform;
        protected GameObject _gameObject;

        public Transform transform
        {
            get {
                return _transform;
            }
            set
            {
                _transform = value;
            }
        }

        public GameObject gameObject
        {
            get {
                return _gameObject;
            }
            set
            {
                _gameObject = value;
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
        /// 更新Item样式
        /// </summary>
        /// <param name="data"></param>
        public abstract void UpdateView(T data);
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