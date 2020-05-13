//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace UIPlugs.ScrollCircleMaker
{
    public class CustomRectCircleHelper<T> : BaseScrollCircleHelper<T> //自定义轨迹
    {
        public override void AddItem(T data, int itemIdx = -1)
        {
            throw new NotImplementedException();
        }

        public override int GetLocation()
        {
            throw new NotImplementedException();
        }

        public override void OnDestroy()
        {
            throw new NotImplementedException();
        }

        public override void OnStart(List<T> _tmpDataSet = null)
        {
            throw new NotImplementedException();
        }

        public override void ResetItems()
        {
            throw new NotImplementedException();
        }

        public override void ToBottom(bool isDrawEnable = true)
        {
            throw new NotImplementedException();
        }

        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {
            throw new NotImplementedException();
        }

        public override void ToTop(bool isDrawEnable = true)
        {
            throw new NotImplementedException();
        }

        public override void UpdateItem(T data, int itemIdx)
        {
            throw new NotImplementedException();
        }
    }
}
