//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
namespace UIPlugs.ScrollCircleMaker
{
    public class aaaaMaker : BaseDirectMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new CustomRectCircleHelper<int>(transform,()=> {
                return new customItem();
            });
            //How to load data?
            for (int i = 0; i < 37; ++i)
                baseHelper.AddItem(i);
            baseHelper.OnStart();
        }
    }
    //How to update item?
    public class customItem : BaseItem<int>
    {
        public override void InitComponents()
        {
            
        }

        public override void InitEvents()
        {
            
        }

        public override void OnDestroy()
        {
            
        }

        public override void UpdateView(int data, int globalSeat)
        {

        }
    }
}