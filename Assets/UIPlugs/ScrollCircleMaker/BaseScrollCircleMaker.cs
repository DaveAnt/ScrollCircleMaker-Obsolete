//------------------------------------------------------------
// ScrollCircleMaker
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseScrollCircleMaker<T> : BaseMaker//启动器
    {
        public IBaseScrollCircleHelper<T> ibaseCircleHelper;

        public override void OnDestroy()
        {
            ibaseCircleHelper?.OnDestroy();
        }
        public override void OnUpdate()//更新
        {
            ibaseCircleHelper?.OnUpdate();
        }
        public void AddItem(T data, int itemIdx = -1)//添加数据
        {
            ibaseCircleHelper?.AddItem(data,itemIdx);
        }
        public void UpdateItem(T data, int itemIdx)
        {
            ibaseCircleHelper?.UpdateItem(data,itemIdx);
        }
        public void ResetItems()//清空数据
        {
            ibaseCircleHelper?.ResetItems();
        }
        public Vector4? GetLocationParam()//获取当前定位参数
        {
            return ibaseCircleHelper?.GetLocationParam();
        }
        public void ToLocation(Vector4 locationNode, bool isDrawEnable = true)
        {
            ibaseCircleHelper?.ToLocation(locationNode, isDrawEnable);
        }
        public void ToTop(bool isDrawEnable = true)//置顶 true存在过程动画
        {
            ibaseCircleHelper?.ToTop(isDrawEnable);
        }
        public void ToBottom(bool isDrawEnable = true)//置底
        {
            ibaseCircleHelper?.ToBottom(isDrawEnable);
        }
    }
}
