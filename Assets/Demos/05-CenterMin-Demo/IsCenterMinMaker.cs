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
    [MakerHandle(MakerHandle.SingleRectCircleHelper)]
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
        RectTransform taskRect;
        IsCenterMinMaker baseMaker;
        public override void InitComponents()
        {
            text = _transform.Find("Text").GetComponent<Text>();
            taskRect = _transform.Find("TaskIMG").GetComponent<RectTransform>();
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
            taskRect.sizeDelta = new Vector2(200 + itemOffset*10,200 + itemOffset * 10);
            switch (baseMaker.Helper.sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    rectTrans.sizeDelta = new Vector2(200, 200 + itemOffset * 10);
                    break;
                default:
                    rectTrans.sizeDelta = new Vector2(200 + itemOffset * 10, 200);
                    break;
            }            
        }
    }
}