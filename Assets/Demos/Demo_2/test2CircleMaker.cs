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
    public class test2CircleMaker : BaseDirectMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new SingleRectCircleHelper<int>(transform,()=> {
                return new shapItem();
            });
            for (int i = 0; i < 35; ++i)
                baseHelper.AddItem(i);
            baseHelper.OnStart();
            //baseHelper.ToBottom(false);
            //How to load data?
        }
    }
    //How to update item?
    public class shapItem : BaseItem<int>
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
            rectTrans.sizeDelta = new Vector2(100,100+ data * 10);
        }
    }
}