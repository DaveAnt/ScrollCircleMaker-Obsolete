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
    [MakerHandle(MakerHandle.MultipleRectCircleHelper)]
    public class CURDMaker : BaseDirectMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new MultipleRectCircleHelper<int>(transform,()=> {
                return new CURDItem();
            });
            for (int i = 0; i < 3; ++i)
                baseHelper.AddItem(i);
            baseHelper.OnStart();
        }
    }
    //How to update item?
    public class CURDItem : BaseItem<int>
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
        	base.UpdateView(data, globalSeat);
            text.text = data.ToString();
        }
    }
}