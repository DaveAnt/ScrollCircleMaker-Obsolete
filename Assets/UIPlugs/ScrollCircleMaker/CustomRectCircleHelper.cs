//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections;
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
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i].y = _itemsPos[i].y - _itemsPos[0].y;
                    break;
                case ScrollDir.BottomToTop:
                    _frontDir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.y.CompareTo(item2.y)));
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    for (int i = 0; i < _itemsPos.Length; ++i)
                    {
                        float viewOffset = viewRectangle - _itemsPos[i].y;
                        _itemsPos[i].y = _itemsPos[i].y + viewOffset;
                    }   
                    break;
                case ScrollDir.LeftToRight:
                    _frontDir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.x.CompareTo(item2.x)));
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i].x = _itemsPos[i].x - _itemsPos[0].x;
                    break;
                case ScrollDir.RightToLeft:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.x.CompareTo(item2.x)));
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    for (int i = 0; i < _itemsPos.Length; ++i)
                    {
                        float viewOffset = viewRectangle - _itemsPos[i].x;
                        _itemsPos[i].x = _itemsPos[i].x + viewOffset;
                    }   
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
            contentRectangle = OnCalRectangle();
            OnRefreshItems();
        }
        public override void DelItem(int itemIdx)
        {
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count);
            _dataSet.RemoveAt(itemIdx);
            OnRefreshOwn();
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
        public override void SwapItem(int firstIdx, int nextIdx)
        {
            firstIdx = Mathf.Clamp(firstIdx, 0, _dataSet.Count - 1);
            nextIdx = Mathf.Clamp(nextIdx, 0, _dataSet.Count - 1);
            if (firstIdx == nextIdx) throw new Exception("Swap Item Same!");
            T swapData = _dataSet[firstIdx];
            _dataSet[firstIdx] = _dataSet[nextIdx];
            _dataSet[nextIdx] = swapData;
            OnRefreshOwn();
        }
        public override void ToLocation(float toSeat, bool isDrawEnable = true)
        {
            if (Mathf.Abs(toSeat - nowSeat) < 0.1f)
                Debug.LogWarning("ToLocation Has Arrived!");
            else if (isDrawEnable)
                _sProperty.StartCoroutine(ToAutoMoveSeat(toSeat));
            else
                ToDirectSeat(toSeat);
        }

        public override void ToLocation(int toIndex, bool isDrawEnable = true)
        {
            if (_dataSet.Count < _sProperty.initItems)
                Debug.LogWarning("ToLocation ItemIndex Overflow!");
            else if (isDrawEnable)
                _sProperty.StartCoroutine(ToAutoMoveIndex(toIndex));
            else
                ToDirectIndex(toIndex);
        }
        #region ------------------------内置函数---------------------------------
        /// <summary>
        /// 计算布局
        /// </summary>
        private float OnCalRectangle()
        {
            int itemExcess = _dataSet.Count % _itemsPos.Length - 1;
            float tmpRectangle = contentBorder + (itemsRect + spacingExt)
                * (_dataSet.Count / _itemsPos.Length);
            if (itemExcess >= 0)
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        tmpRectangle += _itemsPos[itemExcess].y - _itemsPos[0].y +spacingExt;
                        break;
                    case ScrollDir.BottomToTop:
                        tmpRectangle += _itemsPos[_itemsPos.Length - 1].y - _itemsPos[_itemsPos.Length - itemExcess- 1].y + spacingExt;
                        break;
                    case ScrollDir.LeftToRight:
                        tmpRectangle += _itemsPos[itemExcess].x - _itemsPos[0].x +spacingExt;
                        break;
                    default:
                        tmpRectangle += _itemsPos[_itemsPos.Length - 1].x - _itemsPos[_itemsPos.Length - itemExcess - 1].x + spacingExt;
                        break;
                }
            }
            if (!_sProperty.isCircleEnable)
                tmpRectangle -= spacingExt;
            return tmpRectangle;
        }
        /// <summary>
        /// 计算物品位置
        /// </summary>
        /// <param name="dataIdx">数据索引</param>
        private Vector2 OnCalItemPos(int dataIdx)
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    break;
                case ScrollDir.BottomToTop:
                    break;
                case ScrollDir.LeftToRight:
                    break;
                case ScrollDir.RightToLeft:
                    break;
            }
            //dataIdx % dataIdx / _itemsPos.Length
            return default;
        }
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
        /// <summary>
        /// 动画定位
        /// </summary>
        /// <param name="toSeat">真实位置</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveSeat(float toSeat)
        {
            yield return new WaitForEndOfFrame();
        }
        /// <summary>
        /// 直接定位
        /// </summary>
        /// <param name="toSeat">真实位置</param>
        private void ToDirectSeat(float toSeat)
        {

        }
        /// <summary>
        /// 动画定位
        /// </summary>
        /// <param name="toIndex">数据索引</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveIndex(int toIndex)
        {
            yield return new WaitForEndOfFrame();
        }
        /// <summary>
        /// 直接定位
        /// </summary>
        /// <param name="toIndex">数据索引</param>
        private void ToDirectIndex(int toIndex)
        {

        }

        #endregion
    }
}
