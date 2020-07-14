//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseMaker
    {
        /// <summary>
        /// 启动解决方案
        /// </summary>
        /// <param name="transform"></param>
        public abstract void OnStart(Transform transform);
        /// <summary>
        /// 销毁解决方案
        /// </summary>
        public abstract void OnDestroy();
        /// <summary>
        /// 持续更新Item
        /// </summary>
        public abstract void OnUpdate();
    }
}
