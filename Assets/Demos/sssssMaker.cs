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
    public class sssssMaker : BaseCircleMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new SingleRectCircleHelper<int>(transform,()=> {
                return new sssssItem();
            });
            //How to load data?
        }
    }
    //How to update item?
    public class sssssItem : BaseItem<int>
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

        public override void UpdateView(int data)
        {

        }
    }
}