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
    public class SimpleMaker : BaseDirectMaker<HeroItem>
    {
        public static SpriteAtlas HeroItems;
        public string[] HeroName = {
            "杨戬","橘右京","雅典娜","夏侯惇","关羽",
            "赵云","花木兰","宫本","典韦","老布",
            "悟空","达摩","德玛","老夫子","曹操",
            "母老虎","露娜","哪吒","八神","凯",
            "草里蹲","泉水蹲","貂蝉","不知道","冰雪子",
            "妲己","武则天","高渐离","扁鹊","钓鱼子",
            "周瑜","剑王","铁通","芈月","钟馗",
            "火扇子","泥鳅","大乔","李白","诸葛亮"
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
        Text Text;
        Image HeroBG;
        public override void InitComponents()
        {
            Text = _transform.Find("Text").GetComponent<Text>();
            HeroBG = _transform.Find("HeroBG").GetComponent<Image>();
        }

        public override void InitEvents()
        {
            
        }

        public override void OnDestroy()
        {

        }

        public override void UpdateView(HeroItem data, int globalSeat)
        {
            Text.text = data.HeroName;
            HeroBG.sprite = SimpleMaker.HeroItems.GetSprite(data.HeroIMG);
        }
    }
}