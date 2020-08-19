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
    public struct HeroItem
    {
        public string HeroIMG;
        public string HeroName;
    }
    [MakerHandle(MakerHandle.MultipleRectCircleHelper)]
    public class SimpleMaker : BaseDirectMaker<HeroItem>
    {
        public static SpriteAtlas HeroItems;
        public string[] HeroName = {
            "杨戬","橘右京","雅典娜","夏侯惇","关羽",
            "赵云","花木兰","宫本","典韦","老布",
            "悟空","达摩","德玛","老夫子","曹操",
            "钟无艳","露娜","哪吒","八神","凯",
            "安琪拉","王昭君","貂蝉","小乔","甄姬",
            "妲己","武则天","高渐离","扁鹊","姜子牙",
            "周瑜","嬴政","狂铁","芈月","钟馗",
            "不知火舞","东皇太一","大乔","李白","诸葛亮"
        };

        public override void OnStart(Transform transform)
        {
            baseHelper = new MultipleRectCircleHelper<HeroItem>(transform,()=> {
                return new SimpleItem();
            });
            HeroItems = Resources.Load<SpriteAtlas>("HeroItems");
            for (int i = 0; i < HeroName.Length; ++i)
            {
                //模拟人物数据
                HeroItem heroItem;
                heroItem.HeroIMG = "HeroItems_" + i;
                heroItem.HeroName = HeroName[i];
                baseHelper.AddItem(heroItem);
            }
                
            baseHelper.OnStart();
        }
    }
    //How to update item?
    public class SimpleItem : BaseItem<HeroItem>
    {
        Text text;
        Image heroBG;
        public override void InitComponents()
        {
            text = _transform.Find("Text").GetComponent<Text>();
            heroBG = _transform.Find("HeroBG").GetComponent<Image>();
        }

        public override void InitEvents()
        {
            
        }

        public override void OnDestroy()
        {

        }

        public override void UpdateView(HeroItem data, int globalSeat)
        {
            text.text = data.HeroName;
            heroBG.sprite = SimpleMaker.HeroItems.GetSprite(data.HeroIMG);
        }
    }
}