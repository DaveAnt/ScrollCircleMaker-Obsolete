//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker
{
    /// <summary>
    /// 自定义排版
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CustomRectCircleHelper<T> : BaseCircleHelper<T> 
    {
        private Vector2[] _itemsPos;
        private sbyte _slideDir = 1;

        public CustomRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc,Vector2[] itemsPos = null)
            :base(contentTrans,createItemFunc)
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.y.CompareTo(item2.y)));
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.BottomToTop:
                    _slideDir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.y.CompareTo(item2.y)));
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.LeftToRight:
                    _slideDir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.x.CompareTo(item2.x)));
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    break;
                case ScrollDir.RightToLeft:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.x.CompareTo(item2.x)));
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    break;
            }
        }
        protected override void OnRefreshHandler(Vector2 v2)
        {

        }
        public override void OnStart(List<T> tmpDataSet = null)
        {
            if (tmpDataSet != null)
            {
                if (_sProperty.scrollSort == ScrollSort.BackDir ||
                _sProperty.scrollSort == ScrollSort.BackZDir)
                    tmpDataSet.Reverse();
                _dataSet.AddRange(tmpDataSet);
            }
            bool breakStatus;
            Vector2 tmpItemPos; 
            for (int i = 0; i < _dataSet.Count; ++i)
            {
                tmpItemPos = ItemPos(i + 1);
                breakStatus = _scrollRect.vertical ?
                   tmpItemPos.y >  _viewRect.rect.height:
                   tmpItemPos.x > _viewRect.rect.width;
                if (breakStatus)
                {
                    _sProperty.initItems = i;
                    break;
                }
                InitItem(i,tmpItemPos);
            }
            OnAnchorSet();
        }

        public override void DelItem(int itemIdx)
        {

        }

        public override void DelItem(Func<T, T, bool> seekFunc, T data)
        {

        }

        public override void AddItem(T data, int itemIdx = int.MaxValue)
        {
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count);
            switch (_sProperty.scrollSort)
            {
                case ScrollSort.BackDir:
                case ScrollSort.BackZDir:
                    itemIdx = _dataSet.Count - itemIdx;
                    break;
            }
            _dataSet.Insert(itemIdx, data);
        }
        public override void UpdateItem(T data, int itemIdx)
        {

        }

        public override void ToLocation(float toSeat, bool isDrawEnable = true)
        {

        }

        public override void ToLocation(int toSeat, bool isDrawEnable = true)
        {

        }
        public override void ToLocation(Func<T, T, bool> seekFunc, T data, bool isDrawEnable = true)
        {

        }
        #region ------------------------内置函数---------------------------------
        /// <summary>
        /// 数据底部所在位置
        /// </summary>
        /// <param name="isRealBottom">是否真实底部</param>
        /// <returns></returns>
        private int BottomSeat(bool isRealBottom = true)
        {
            switch (_scrollRect.vertical)
            {
                case true:
                    if (isRealBottom)
                        return (int)(_contentRect.rect.height - _viewRect.rect.height);
                    return (int)(_contentRect.rect.height + _sProperty.WidthExt - ContentBorder(true));
                default:
                    if (isRealBottom)
                        return (int)(_contentRect.rect.width - _viewRect.rect.width);
                    return (int)(_contentRect.rect.width + _sProperty.WidthExt - ContentBorder(true));
            }
        }
        /// <summary>
        /// 扩展长度
        /// </summary>
        /// <param name="isCircleEnable">是否循环扩展长度</param>
        /// <returns></returns>
        private float ContentBorder(bool isCircleEnable)
        {
            switch (_scrollRect.vertical)
            {
                case true:
                    if (isCircleEnable)
                        return 2 * (_viewRect.rect.height + _sProperty.WidthExt);
                    return _sProperty.TopExt + _sProperty.BottomExt;
                default:
                    if (isCircleEnable)
                        return 2 * (_viewRect.rect.width + _sProperty.WidthExt);
                    return _sProperty.LeftExt + _sProperty.RightExt;
            }
        }
        /// <summary>
        /// Item位置
        /// </summary>
        /// <param name="itemIdx">位置索引</param>
        /// <returns></returns>
        private Vector2 ItemPos(int itemIdx)
        {
            Vector2 tmpItemPos;
            int totalNum = itemIdx / _itemsPos.Length;
            int remainNum = itemIdx % _itemsPos.Length;
            if (_scrollRect.vertical)
            {
                tmpItemPos.x = remainNum == 0 ? _itemsPos[_itemsPos.Length - 1].x : _itemsPos[remainNum - 1].x;
                tmpItemPos.y = totalNum * (_itemsPos[_itemsPos.Length - 1].y + _sProperty.WidthExt) +
                    (remainNum == 0 ? -_sProperty.WidthExt : _itemsPos[remainNum - 1].y);
            }
            else
            {
                tmpItemPos.x = totalNum * (_itemsPos[_itemsPos.Length - 1].x + _sProperty.WidthExt)+
                    (remainNum == 0 ? -_sProperty.WidthExt : _itemsPos[remainNum - 1].x);
                tmpItemPos.y = remainNum == 0 ? _itemsPos[_itemsPos.Length - 1].y : _itemsPos[remainNum - 1].y;
            }
            return tmpItemPos;
        }
        /// <summary>
        /// 初始化item
        /// </summary>
        /// <param name="itemIdx">位置</param>
        private void InitItem(int itemIdx,Vector2 itemPos)
        {
            BaseItem<T> baseItem = _createItemFunc();
            baseItem.gameObject = GameObject.Instantiate(_baseItem, _contentRect);
            baseItem.gameObject.name = _baseItem.name + itemIdx;
            baseItem.rectTrans.anchoredPosition = itemPos;
            baseItem.InitComponents();
            baseItem.InitEvents();
            baseItem.UpdateView(_dataSet[itemIdx],itemIdx);
            _itemSet.Add(baseItem);
        }
        /// <summary>
        /// 自适应高宽
        /// </summary>
        private void OnAnchorSet()
        {           
           
        }
        #endregion
    }
}
