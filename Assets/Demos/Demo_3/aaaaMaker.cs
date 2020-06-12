//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
namespace UIPlugs.ScrollCircleMaker
{
    public class aaaaMaker : BaseCircleMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new CustomRectCircleHelper<int>(transform,()=> {
                return new customItem();
            });
            //How to load data?
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

        public override void UpdateView(int data, int? globalSeat = null)
        {

        }
    }
}