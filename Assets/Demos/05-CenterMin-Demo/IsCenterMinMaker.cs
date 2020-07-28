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
    public class IsCenterMinMaker : BaseDirectMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new SingleRectCircleHelper<int>(transform,()=> {
                return new TaskItem();
            });
            for (int i = 0; i < 100; i++)
                baseHelper.AddItem(i);
            baseHelper.OnStart();
            //How to load data?
        }
    }
    //How to update item?
    public class TaskItem : BaseItem<int>
    {
        Text text;
        IsCenterMinMaker baseMaker;
        public override void InitComponents()
        {
            text = _transform.Find("Text").GetComponent<Text>();
            baseMaker = _transform.parent.GetComponent<ScrollCircleComponent>().baseMaker as IsCenterMinMaker;
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
            int itemOffset = Mathf.Abs(baseMaker.Helper.viewCore - globalSeat);
            rectTrans.sizeDelta = new Vector2(200 + itemOffset*10,200 + itemOffset * 10);
        }
    }
}