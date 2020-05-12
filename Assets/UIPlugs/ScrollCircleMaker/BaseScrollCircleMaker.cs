//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
namespace UIPlugs.ScrollCircleMaker
{
    public abstract class BaseScrollCircleMaker<T> : BaseMaker//启动器
    {
        public BaseScrollCircleHelper<T> baseCircleHelper;

        public override void OnDestroy()
        {
            baseCircleHelper?.OnDestroy();
        }
        public override void OnUpdate()//更新
        {
            baseCircleHelper?.OnUpdate();
        }
        public void AddItem(T data, int itemIdx = -1)//添加数据
        {
            baseCircleHelper?.AddItem(data,itemIdx);
        }
        public void UpdateItem(T data, int itemIdx)
        {
            baseCircleHelper?.UpdateItem(data,itemIdx);
        }
        public void ResetItems()//清空数据
        {
            baseCircleHelper?.ResetItems();
        }
        public Vector4? GetLocationParam()//获取当前定位参数
        {
            return baseCircleHelper?.GetLocationParam();
        }
        public void ToLocation(Vector4 locationNode, bool isDrawEnable = true)
        {
            baseCircleHelper?.ToLocation(locationNode, isDrawEnable);
        }
        public void ToTop(bool isDrawEnable = true)//置顶 true存在过程动画
        {
            baseCircleHelper?.ToTop(isDrawEnable);
        }
        public void ToBottom(bool isDrawEnable = true)//置底
        {
            baseCircleHelper?.ToBottom(isDrawEnable);
        }
    }
}
