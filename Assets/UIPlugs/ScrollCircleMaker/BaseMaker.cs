//------------------------------------------------------------
// ScrollCircleMaker
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseMaker
    {
        public abstract void OnStart(Transform transform);
        public abstract void OnDestroy();
        public abstract void OnUpdate();
    }
}
