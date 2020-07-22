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
    /// 自定义滑动循环
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CustomRectCircleHelper<T> : BaseCircleHelper<T> 
    {
        #region
        /// <summary>
        /// 整体布局高宽
        /// </summary>
        private float itemsRect
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return Mathf.Abs(_itemsPos[_itemsPos.Length - 1].y - _itemsPos[0].y);
                    default:
                        return Mathf.Abs(_itemsPos[_itemsPos.Length - 1].x - _itemsPos[0].x);
                }
            }
        }
        private float calRectangle
        {
            get
            {
                int itemNum = _dataSet.Count / _itemsPos.Length;
                int itemExcess = _dataSet.Count % _itemsPos.Length - 1;
                float tmpRectangle = contentBorder + itemNum * (itemsRect + spacingExt);
                if (!_sProperty.isCircleEnable)
                    tmpRectangle -= spacingExt;
                return tmpRectangle;
            }
        }
        #endregion
        private Vector2[] _itemsPos;
        /// <summary>
        /// 自定义布局滑动构造
        /// </summary>
        /// <param name="contentTrans">内容组件</param>
        /// <param name="createItemFunc">创建物品函数</param>
        /// <param name="itemsPos">物品位置</param>
        public CustomRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc,Vector2[] itemsPos = null)
            :base(contentTrans,createItemFunc)
        {
            _itemsPos = itemsPos;
            _sProperty.ItemsPos = itemsPos;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.y.CompareTo(item2.y)));
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.BottomToTop:
                    _frontDir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.y.CompareTo(item2.y)));
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.LeftToRight:
                    _frontDir = -1;
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
            base.OnRefreshHandler(v2);
            OnRefreshCircle();
        }
        public override void OnStart(List<T> tmpDataSet = null)
        {
            base.OnStart(tmpDataSet);
            _sProperty.initItems = (int)(viewRectangle / itemsRect + 1) * _itemsPos.Length;
            contentRectangle = contentBorder;
            OnRefreshItems();
        }
        public override void DelItem(int itemIdx)
        {
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count);
            switch (_sProperty.scrollSort)
            {
                case ScrollSort.BackDir:
                case ScrollSort.BackZDir:
                    itemIdx = _dataSet.Count - itemIdx;
                    break;
            }
            _dataSet.RemoveAt(itemIdx);
            OnRefreshOwn();
        }

        public override void DelItem(Func<T, T, bool> seekFunc, T data)
        {
            for (int i = _dataSet.Count - 1; i >= 0; ++i)
            {
                if (seekFunc(data, _dataSet[i]))
                {
                    _dataSet.RemoveAt(i);
                    OnRefreshOwn();
                    return;
                }
            }
            Debug.LogWarning("DelItem SeekFunc Fail!");
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
            OnRefreshOwn();
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
        #region ------------------------内置函数---------------------------------
        /// <summary>
        /// 刷新当前样式
        /// </summary>
        private void OnRefreshOwn()
        {
            if (!_firstRun) return;
            _sProperty.visibleItems = _dataSet.Count;
            lockRefresh = _sProperty.initItems >= _dataSet.Count;
            OnRefreshItems();
        }
        /// <summary>
        /// 刷新所有物品
        /// </summary>
        private void OnRefreshItems()
        {
            int tmpItemIdx, tmpDataIdx;
            for (int i = 0; i < _sProperty.initItems; ++i)
            {
                tmpDataIdx = (_sProperty.dataIdx + i) % _dataSet.Count;
                tmpItemIdx = (_sProperty.itemIdx + i) % _sProperty.initItems;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                _itemSet[tmpItemIdx].rectTrans.anchoredPosition = _itemsPos[tmpDataIdx % _itemsPos.Length];
                if (!_sProperty.isCircleEnable && _sProperty.dataIdx + i >= _dataSet.Count)
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
                else
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
            }
        }
        /// <summary>
        /// 下刷新
        /// </summary>
        private void OnRefreshDown()
        {
            
        }
        /// <summary>
        /// 上刷新
        /// </summary>
        private void OnRefreshUp()
        {
            
        }
        /// <summary>
        /// 循环刷新
        /// </summary>
        private void OnRefreshCircle()
        {
            
        }

        #endregion
    }
}
