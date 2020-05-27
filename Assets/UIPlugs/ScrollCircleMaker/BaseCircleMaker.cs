//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseCircleMaker<T> : BaseMaker//启动器
    {
        protected BaseCircleHelper<T> baseHelper;
        public override void OnDestroy()
        {
            baseHelper?.OnDestroy();
        }
        public override void OnUpdate()//更新
        {
            baseHelper?.OnUpdate();
        }
    }
}
