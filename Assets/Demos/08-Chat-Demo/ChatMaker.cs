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
    public class ChatMaker : BaseDirectMaker<string>
    {
        public string[] ChatItem = {
            "你好，这个是关于优化滑动性能(减少实例化)的滑动插件。",
            "可以运行时进行删除、添加、更新、交换物品。存在三种滑动方式并且可以支持循环滑动，可以使用它解决任何滑动需求。",
            "当然也可以为这个插件添加辅助器，目前存在三大辅助器(多行规则长度滑动、单行不规则长度滑动、自定义位置滑动)。",
            "可能存在很多未知的BUG,但是作者相信会慢慢修复和完善的",
            "插件会上架到Unity商店里,也可以选择开源地址帮助维护或白嫖。",
            "还是希望各位谨慎使用单行不规则长度滑动、自定义位置滑动,至于多行规则长度滑动应该是不存在问题的",
            "如果使用时有什么不足或者问题，希望即时联系到作者，以便修改。",
            "它可以实现聊天框的滚动，收纳框，基本的游戏物品列表需求和自定义位置的滑动。",
            "但是需要注意的是，当你改变长度时，必须对应改变父节点的高宽，就像这个Demo一样。",
            "接下来就来测试一下吧！",
            "测试1",
            "测试1\n测试1",
            "测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1\n测试1",
            "测试1\n测试1\n测试1",
            "测试1\n测试1",
            "测试1"          
        };
        public override void OnStart(Transform transform)
        {
            baseHelper = new SingleRectCircleHelper<string>(transform,()=> {
                return new ChatItem();
            });
            //How to load data?
            for (int i = 0; i < ChatItem.Length; ++i)
                baseHelper.AddItem(ChatItem[i]);
            baseHelper.OnStart();
        }
    }
    //How to update item?
    public class ChatItem : BaseItem<string>
    {
        Text Text;
        public override void InitComponents()
        {
            Text = _transform.Find("Text").GetComponent<Text>();
        }

        public override void InitEvents()
        {
            
        }

        public override void OnDestroy()
        {
            
        }

        public override void UpdateView(string data, int globalSeat)
        {
            Text.text = data;
            Text.GetComponent<ContentSizeFitter>().SetLayoutVertical();
            rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x, Text.GetComponent<RectTransform>().rect.height + 80);
        }
    }
}