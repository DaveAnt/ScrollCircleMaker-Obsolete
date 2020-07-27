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
    public class ChatMaker : BaseDirectMaker<string>
    {
        public string[] ChatItem = {
            "你好，这个是关于优化滑动性能(减少实例化)的滑动插件。",
            "可以运行时进行删除、添加、更新、交换物品，十分的强大。存在三种滑动方式并且可以支持循环滑动，你可以使用它解决任何滑动需求。",
            "当然你也可以为这个插件添加自己想要的辅助器，目前存在三大辅助器(多行规则长度滑动、单行不规则长度滑动、自定义位置滑动)。",
            "可能存在很多未知的BUG,但是作者坚信这个插件以后会越来越好的。",
            "插件上架到了Unity的商店里了,希望可以购买表示鼓励,也可以选择开源地址白嫖。",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
            "您好，这个是关于优化滑动性能(减少实例化)和提高开发速率的滑动插件",
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