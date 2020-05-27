//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIPlugs.ScrollCircleMaker
{
    public class CustomRectCircleHelper<T> : BaseCircleHelper<T> //自定义轨迹
    {
        public CustomRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc)
        {

        }
        protected override void OnRefreshHandler(Vector2 v2)
        {

        }

        public override void OnStart(List<T> _tmpDataSet = null)
        {

        }

        public override void AddItem(T data, int itemIdx = -1)
        {

        }
        public override void UpdateItem(T data, int itemIdx)
        {

        }
        public override void ResetItems()
        {

        }

        public override int GetLocation()
        {
            throw new NotImplementedException();
        }

        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {

        }

        public override void ToBottom(bool isDrawEnable = true)
        {

        }

        public override void ToTop(bool isDrawEnable = true)
        {

        }
    }
}
