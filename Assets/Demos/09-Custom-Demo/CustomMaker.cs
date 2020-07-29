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
    public class CustomMaker : BaseDirectMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            Vector2[] vector2 = new Vector2[10];
            for (int i = 0; i < vector2.Length; ++i)
                vector2[i] = new Vector2(i * 100, i * 100);
            baseHelper = new CustomRectCircleHelper<int>(transform,()=> {
                return new CustomItem();
            }, vector2);
            for (int i = 0; i < 1000; ++i)
                baseHelper.AddItem(i);
            baseHelper.OnStart();
            //How to load data?
        }
    }
    //How to update item?
    public class CustomItem : BaseItem<int>
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