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
    public class testCircleMaker : BaseScrollCircleMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseCircleHelper = new MultipleRectCircleHelper<int>(transform,()=> {
                return new HeroItem();
            });
            for (int i = 0; i < 9999; ++i)
                baseCircleHelper.AddItem(i);
            baseCircleHelper.OnStart();
            baseCircleHelper.ToBottom();
        }

    }

    public class HeroItem : BaseItem<int>
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

        public override void UpdateView(int data)
        {
            text.text = data.ToString();
        }
    }
}
