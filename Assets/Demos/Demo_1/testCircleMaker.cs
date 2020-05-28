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
    public class testCircleMaker : BaseCircleMaker<int>
    {
        public override void OnStart(Transform transform)
        {
            baseHelper = new MultipleRectCircleHelper<int>(transform,()=> {
                return new HeroaItem();
            });
            for (int i = 0; i < 3784; ++i)
                baseHelper.AddItem(i);
            baseHelper.OnStart();
            //baseHelper.ToLocation(1000000000);
        }

        public void MoveOk()
        {
           // Debug.LogError("1111111");
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

        public override void UpdateView(int data)
        {
            text.text = data.ToString();
        }
    }
}
