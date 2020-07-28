//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
namespace UIPlugs.ScrollCircleMaker
{
    public class CircleAndUpdateMaker : BaseDirectMaker<string>
    {
        public static SpriteAtlas DiaItems;
        public override void OnStart(Transform transform)
        {
            baseHelper = new SingleRectCircleHelper<string>(transform,()=> {
                return new ShapItem();
            });
            DiaItems = Resources.Load<SpriteAtlas>("DiaItems");
            for (int i = 1; i <= DiaItems.spriteCount; ++i)
                baseHelper.AddItem("dia ("+i+")");
            baseHelper.OnStart();
            //How to load data?
        }
    }
    //How to update item?
    public class ShapItem : BaseItem<string>
    {
        Image diaIMG;
        Transform diaTrans;
        RectTransform diaRectTrans;
        sbyte isBig = 2;
        public override void InitComponents()
        {
            diaTrans = _transform.Find("DiaIMG");
            diaRectTrans = diaTrans as RectTransform;
            diaIMG = diaTrans.GetComponent<Image>();
        }

        public override void InitEvents()
        {
			          
        }

        public override void OnDestroy()
        {
			         
        }

        public override void UpdateView(string data, int globalSeat)
        {
        	base.UpdateView(data, globalSeat);
            diaIMG.sprite = CircleAndUpdateMaker.DiaItems.GetSprite(data);
        }

        public override void OnUpdate()
        {
            if (diaRectTrans.rect.width > 160)
                isBig = -2;
            else if (diaRectTrans.rect.width < 60)
                isBig = 2;
            diaRectTrans.sizeDelta = new Vector2(diaRectTrans.rect.width + isBig, diaRectTrans.rect.height +isBig);
        }
    }
}