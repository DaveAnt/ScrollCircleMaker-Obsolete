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
    public struct SkillInfo
    {
        public string SkillName;
        public string SkillPath;
        public int SkillHurt;
    }
    [MakerHandle(MakerHandle.SingleRectCircleHelper)]
    public class ClickMaker : BaseDirectMaker<SkillInfo>
    {
        public static SpriteAtlas SkillItems;
        public override void OnStart(Transform transform)
        {
            baseHelper = new SingleRectCircleHelper<SkillInfo>(transform,()=> {
                return new SkillItem();
            });
            SkillItems = Resources.Load<SpriteAtlas>("SkillIcons");
            System.Random rnd = new System.Random();
            for (int i = 1; i <= SkillItems.spriteCount; ++i)
            {
                //模拟人物数据
                SkillInfo skillInfo;
                skillInfo.SkillPath = "skill_icon ("+i+")";
                skillInfo.SkillName = "技能" + i;
                skillInfo.SkillHurt = rnd.Next(1, 100);
                baseHelper.AddItem(skillInfo);
            }
            baseHelper.OnStart();
        }
    }
    //How to update item?
    public class SkillItem : BaseItem<SkillInfo>
    {
        Image skillIMG;
        Button skillBtn;
        public override void InitComponents()
        {
            skillIMG = _transform.GetComponent<Image>();
            skillBtn = _transform.GetComponent<Button>();
        }

        public override void InitEvents()
        {
            skillBtn.onClick.AddListener(() =>
            {
                Debug.Log("使用" + itemData.SkillName + ",造成伤害"+itemData.SkillHurt);
            });
        }

        public override void OnDestroy()
        {
			         
        }

        public override void UpdateView(SkillInfo data, int globalSeat)
        {
            base.UpdateView(data, globalSeat);
            skillIMG.sprite = ClickMaker.SkillItems.GetSprite(data.SkillPath);
        }
    }
}