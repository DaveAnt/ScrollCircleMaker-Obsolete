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

        public abstract void InitComponents();
        public abstract void InitEvents();
        public abstract void UpdateView(T data);
        public abstract void OnDestroy();
        public virtual void OnUpdate() { }
    }
}