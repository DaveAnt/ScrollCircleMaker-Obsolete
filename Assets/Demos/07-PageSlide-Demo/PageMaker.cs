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
    public struct PageInfo
    {
        public string Content;
        public int PageNum;    
    }
    [MakerHandle(MakerHandle.SingleRectCircleHelper)]
    public class PageMaker : BaseDirectMaker<PageInfo>
    {
        public string[] PageItems = {
            "这是一个神奇的故事",
            "以前有个山，山里有个庙",
            "庙里有个老和尚",
            "老和尚和小和尚说",
            "以前有个大山，大山里有个庙",
            "庙里有个小和尚",
            "小和尚和老和尚说",
            "庙里有个老和尚",
            "老和尚和小和尚说",
            "以前有个山，山里有个庙",
            "庙里有个小和尚",
            "小和尚和老和尚说",
            "庙里有个老和尚"
        };
        public override void OnStart(Transform transform)
        {
            baseHelper = new SingleRectCircleHelper<PageInfo>(transform,()=> {
                return new PageItem();
            });
            //How to load data?
            for(int i=0;i< PageItems.Length; ++i)
            {
                PageInfo pageInfo;
                pageInfo.Content = PageItems[i];
                pageInfo.PageNum = i;
                baseHelper.AddItem(pageInfo);
            }
            baseHelper.OnStart();
        }
    }
    //How to update item?
    public class PageItem : BaseItem<PageInfo>
    {
        Text pageNum, pageContent;
        public override void InitComponents()
        {
            pageContent = _transform.Find("Content").GetComponent<Text>();
            pageNum = _transform.Find("Num").GetComponent<Text>();
        }

        public override void InitEvents()
        {
			          
        }

        public override void OnDestroy()
        {
			         
        }

        public override void UpdateView(PageInfo data, int globalSeat)
        {
        	base.UpdateView(data, globalSeat);
            pageContent.text = data.Content;
            pageNum.text = data.PageNum.ToString();
        }
    }
}