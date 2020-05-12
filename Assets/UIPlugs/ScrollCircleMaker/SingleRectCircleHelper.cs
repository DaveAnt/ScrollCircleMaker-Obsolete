//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker       //单行矩形滑动循环  支持不规则长宽 聊天框，收纳框等等
{
    public class SingleRectCircleHelper<T> : BaseScrollCircleHelper<T>
    {
        public override void OnStart(List<T> _tmpDataSet = null)
        {
            
        }
        public override void OnDestroy()
        {
            throw new NotImplementedException();
        }

        public override void AddItem(T data, int itemIdx = -1)
        {
            throw new NotImplementedException();
        }

        public override void UpdateItem(T data, int itemIdx)
        {
            throw new NotImplementedException();
        }

        public override void ResetItems()
        {
            throw new NotImplementedException();
        }

        public override int GetLocation()
        {
            throw new NotImplementedException();
        }

        public override void ToLocation(int locationNode, bool isDrawEnable = true)
        {
            throw new NotImplementedException();
        }

        public override void ToTop(bool isDrawEnable = true)
        {
            throw new NotImplementedException();
        }

        public override void ToBottom(bool isDrawEnable = true)
        {
            throw new NotImplementedException();
        }
    }
}
