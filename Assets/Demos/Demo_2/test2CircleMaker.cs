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
    public class test2CircleMaker : BaseCircleMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new SingleRectCircleHelper<int>(transform,()=> {
                return new shapItem();
            });
            for (int i = 0; i < 37; ++i)
                baseHelper.AddItem(i);
            baseHelper.OnStart();
            //baseHelper.ToBottom(false);s
            //How to load data?
        }
    }
    //How to update item?
    public class shapItem : BaseItem<int>
    {
        Text text;
        public override void InitComponents(Transform transform)
        {
            text = _transform.Find("Text").GetComponent<Text>();
        }

        public override void InitEvents()
        {
            
        }

        public override void OnDestroy()
        {

        }

        public override void UpdateView(int data, int? globalSeat = null)
        {
            text.text = data.ToString();
            rectTrans.sizeDelta = new Vector2(100,100+ globalSeat.Value * 3);
        }
    }
}