//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
namespace UIPlugs.ScrollCircleMaker
{
    /// <summary>
    /// 纵横轨迹解决方案
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseDirectMaker<T> : BaseMaker//启动器
    {
        protected BaseCircleHelper<T> baseHelper;

        public BaseCircleHelper<T> Helper
        {
            get{
                return baseHelper;
            }
        }
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
