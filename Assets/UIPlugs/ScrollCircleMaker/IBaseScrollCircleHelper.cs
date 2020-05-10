//------------------------------------------------------------
// ScrollCircleMaker
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UIPlugs.ScrollCircleMaker
{
    public interface IBaseScrollCircleHelper<T>
    {
        void OnStart(List<T> _tmpDataSet = null);//启动
        void OnDestroy();//销毁
        void OnUpdate();//更新
        void AddItem(T data, int itemIdx = -1);//添加数据
        void UpdateItem(T data, int itemIdx);
        void ResetItems();//清空数据
        Vector4 GetLocationParam();//获取当前定位参数
        void ToLocation(Vector4 locationNode, bool isDrawEnable = true);
        void ToTop(bool isDrawEnable = true);//置顶 true存在过程动画
        void ToBottom(bool isDrawEnable = true);//置底
    }
}
