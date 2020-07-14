//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.UI;
namespace UIPlugs.ScrollCircleMaker
{
    public class testCircleMaker : BaseDirectMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new MultipleRectCircleHelper<int>(transform,()=> {
                return new HeroaItem();
            });
            for (int i = 0; i < 1113; ++i)
                baseHelper.AddItem(i);
            baseHelper.OnStart();
            baseHelper.ToLocation(340);
        }
    }

    public class HeroaItem : BaseItem<int>
    {
        Text text;
        public override void InitComponents()
        {
            text = _transform.Find("Text").GetComponent<Text>();
        }

        public override void InitEvents()
        {
            
        }

        public override void OnDestroy()
        {
            
        }

        public override void UpdateView(int data, int globalSeat)
        {
            text.text = data.ToString();
        }
    }
}
