//------------------------------------------------------------
// ScrollCircleMaker
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
            ibaseCircleHelper = new MultipleRectCircleHelper<int>(transform,()=> {
                return new HeroItem();
            });
            for (int i = 0; i < 48; ++i)
                ibaseCircleHelper.AddItem(i);
            ibaseCircleHelper.OnStart();
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
