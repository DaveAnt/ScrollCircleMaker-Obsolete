## HOMEPAGE

- **简体中文** - [https://daveant.gitee.io/posts/ScrollCircleMaker-v1.0教程/](https://daveant.gitee.io/posts/ScrollCircleMaker-v1.0教程/) 
- **QQ讨论** - 939564762

## ScrollCircle Maker 简介

<u><font color=FF0000>ScrollCircle Maker目前已经废弃，由于不满意此工程的设计结构和性能，作者准备基于ScriptableProcessor插件重写ScrollCircle Maker，后续两个插件都会低价上架Unity商店，此工程仅供参考。</font></u>

ScrollCircle Maker是基于 Unity 引擎ScrollView组件的循环复用滑动插件，主要对游戏开发过程中滑动模块进行优化和一些滑动表现的封装，很大程度地提高滑动模块的开发进度并保证滑动质量，插件启动时会运算出需要实例化最少的物品数量来表现出滑动的最佳效果。在ScrollCircle Maker v1.0 版本中，包含以下3大个内置辅助器，多行规则长度辅助器、单行不规则长度辅助器、自定义位置辅助器，后续可能按照开发者需求添加另外的辅助器供开发者使用。

1. **多行规则长度辅助器 (MultipleRectCircleHelper)** - n\*m行列的不变高宽时，比如游戏中的背包滑动。

2. **单行不规则长度辅助器 (SingleRectCircleHelper)** - 1\*n或n\*1可变高宽时，比如聊天模块的不定高度滑动。

3. **自定义位置辅助器 (CustomRectCircleHelper)** - 当每个物品位置不定时，开发者可自行传入布局数组来决定排版。

插件接口文档表格

函数名|函数作用|参数说明
-|-|-
OnStart|启动插件|物品数据列表
OnDestroy|释放插件|无
ResetItems|重置插件|无
OnSlideLockout|锁定滑动|是否滑动
DelItem|移除物品数据|物品索引
DelItem|移除物品数据|匹配物品函数、移除物品数据
AddItem|添加物品数据|物品数据、物品索引
UpdateItem|更新物品样式|物品数据、物品索引
SwapItem|交换物品位置	|被交换物品索引、交换物品索引
SwapItem|交换物品位置	|匹配物品函数、被交换物品数据、交换物品数据
ToLocation|真实位置定位|真实位置值、是否存在定位动画过程
ToLocation|数据索引定位|数据索引、是否存在定位动画过程
ToLocation|数据匹配定位|匹配物品函数、是否存在定位动画过程
ToTop|置顶定位|是否存在定位动画过程
ToBottom|置底定位|是否存在定位动画过程

---

## INTRODUCTI

Scrollcircle maker is a unity based The cyclic reuse of sliding plug-ins of the engine Scrollview component mainly optimizes the sliding module and encapsulates some sliding performance in the game development process, greatly improves the development progress of the sliding module and ensures the sliding quality. When the plug-in starts, it will calculate the number of items that need to be instantiated to show the best effect of sliding. In addition, the length of the built-in rule maker in version 1.0 may not be used by the auxiliary developers according to their needs.

1. **Multi line rule length helper (MultipleRectCircleHelper)** - When the height and width of the n \ * m column is constant, such as the backpack sliding in the game
2. **One line irregular length helper (SingleRectCircleHelper)** - 1 \* n or n \* 1 with variable height and width, such as the variable height sliding of chat module
3. **Custom location helper (CustomRectCircleHelper)** - When the location of each item is not fixed, the developer can input the layout array to determine the layout

Plug in interface document form

Function|Effect|Parameter description
-|-|-
OnStart|Start plug-in|Item data list
OnDestroy|Releasing plug-ins|null
ResetItems|Reset plug-in|null
OnSlideLockout|Lock slide|Sliding or not
DelItem|Remove item data|Item index
DelItem|Remove item data|Matching item function、Remove item data
AddItem|Add item data|Item data、Item Index
UpdateItem|Update item style|Item data、Item Index
SwapItem|Exchange item location	|Index of exchanged items、Exchange index
SwapItem|Exchange item location	|Matching item function、Exchanged item data、Exchange item data
ToLocation|Real location|True location value、Is there a positioning animation process
ToLocation|Data index positioning|data Index、Is there a positioning animation process
ToLocation|Data matching and positioning|Matching item function、Is there a positioning animation process
ToTop|Top positioning|Is there a positioning animation process
ToBottom|Bottom positioning|Is there a positioning animation process
