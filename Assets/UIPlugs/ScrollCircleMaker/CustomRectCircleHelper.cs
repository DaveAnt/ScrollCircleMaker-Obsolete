//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        /// <summary>
        /// 首物品实例
        /// </summary>
        private RectTransform headItemRect
        {
            get
            {
                return _contentRect.GetChild(0).transform as RectTransform;
            }
        }
        /// <summary>
        /// 首物品高宽
        /// </summary>
        private int headItemLen
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return (int)headItemRect.rect.height;
                    default:
                        return (int)headItemRect.rect.width;
                }
            }
        }
        /// <summary>
        /// 尾物品实例
        /// </summary>
        private RectTransform footItemRect
        {
            get
            {
                return _contentRect.GetChild(_itemSet.Count - 1).transform as RectTransform;
            }
        }
        /// <summary>
        /// 尾物品高宽
        /// </summary>
        private int footItemLen
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return (int)footItemRect.rect.height;
                    default:
                        return (int)footItemRect.rect.width;
                }
            }
        }
        /// <summary>
        /// 首物品位置
        /// </summary>
        private float headItemSeat
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.TopToBottom:
                        return -headItemRect.anchoredPosition.y;
                    case ScrollDir.BottomToTop:
                        return headItemRect.anchoredPosition.y;
                    case ScrollDir.LeftToRight:
                        return headItemRect.anchoredPosition.x;
                    default:
                        return -headItemRect.anchoredPosition.x;
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
            if (itemsPos == null)
                _itemsPos = _sProperty.ItemsPos;
            else
            {
                _itemsPos = itemsPos;
                _sProperty.ItemsPos = itemsPos;
            }
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i].y = -_itemsPos[i].y;
                    _itemRect.anchorMin = _itemRect.anchorMax = _itemRect.pivot = new Vector2(0,1);
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item2.y.CompareTo(item1.y)));
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i].y = _itemsPos[i].y - _itemsPos[0].y;
                    break;
                case ScrollDir.BottomToTop:
                    _frontDir = -1;
                    _itemRect.anchorMin = _itemRect.anchorMax = _itemRect.pivot = Vector2.zero;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.y.CompareTo(item2.y)));
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i].y = _itemsPos[i].y - _itemsPos[0].y;
                    break;
                case ScrollDir.LeftToRight:
                    _frontDir = -1;
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i].y = -_itemsPos[i].y;
                    _itemRect.anchorMin = _itemRect.anchorMax = _itemRect.pivot = new Vector2(0, 1);
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item1.x.CompareTo(item2.x)));
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i].x = _itemsPos[i].x - _itemsPos[0].x;
                    break;
                case ScrollDir.RightToLeft:
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i] = -_itemsPos[i];
                    _itemRect.anchorMin = _itemRect.anchorMax = _itemRect.pivot = Vector2.one;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    Array.Sort(_itemsPos, new Comparison<Vector2>((item1, item2) => item2.x.CompareTo(item1.x)));
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    for (int i = 0; i < _itemsPos.Length; ++i)
                        _itemsPos[i].x = _itemsPos[i].x -_itemsPos[0].x;
                    break;
            }
            _sProperty.initItems = (int)(viewRect / itemsRect + 1) * _itemsPos.Length;
            _scrollRect.inertia = _sProperty.scrollType != ScrollType.Drag;
        }
        protected override void OnRefreshHandler(Vector2 v2)
        {
            base.OnRefreshHandler(v2);
            OnRefreshCircle();
        }
        public override void OnStart(List<T> tmpDataSet = null)
        {
            base.OnStart(tmpDataSet);
            contentRect = getContentRect();
            _sProperty.visibleItems = _dataSet.Count;
            if (_sProperty.isCircleEnable)
                nowSeat = topSeat;
            if(_dataSet.Count > 0)
                OnRefreshItems();
        }
        public override void DelItem(int itemIdx)
        {
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count - 1);
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
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count - 1);
            _dataSet[itemIdx] = data;
            OnRefreshOwn();
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
        /// 对齐偏移
        /// </summary>
        /// <param name="tmpItemIdx">需要对齐到的索引</param>
        private void ToItemOffset(int tmpItemIdx)
        {
            for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                _itemSet[i].transform.SetAsLastSibling();
            for (int i = tmpItemIdx - 1; i >= _sProperty.itemIdx; --i)
                _itemSet[i].transform.SetAsFirstSibling();
        }
        /// <summary>
        /// 物品循环自适应
        /// </summary>
        private void ToItemCircle()
        {
            int tmpItemIdx;
            if (isHighDefine)
            {
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.itemIdx = _sProperty.dataIdx = 0;
                ToItemOffset(tmpItemIdx);
                OnRefreshItems();
                nowSeat = topSeat;
            }
            else if (isLowerDefine)
            {
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _dataSet.Count - _sProperty.initItems % _dataSet.Count;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.initItems;
                ToItemOffset(tmpItemIdx);
                OnRefreshItems();
                nowSeat = bottomSeat;
            }
        }
        /// <summary>
        /// 计算布局
        /// </summary>
        private float getContentRect()
        {
            int itemExcess = _dataSet.Count % _itemsPos.Length - 1;
            float tmpRectangle = contentBorder + (itemsRect + spacingExt)
                * (_dataSet.Count / _itemsPos.Length);
            if (itemExcess >= 0)
            { 
                switch (_scrollRect.vertical)
                {
                    case true:
                        tmpRectangle += Mathf.Abs(_itemsPos[itemExcess].y - _itemsPos[0].y) + spacingExt;
                        break;
                    default:
                        tmpRectangle += Mathf.Abs(_itemsPos[itemExcess].x - _itemsPos[0].x) + spacingExt;
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
        private Vector2 SetItemPos(int dataIdx)
        {
            Vector2 itemPos = default;
            int tmpDataIdx;
            if (dataIdx < 0)
                tmpDataIdx = dataIdx + _dataSet.Count;
            else if (dataIdx >= _dataSet.Count)
                tmpDataIdx = dataIdx % _dataSet.Count;
            else
                tmpDataIdx = dataIdx;
            int itemsNum = tmpDataIdx / _itemsPos.Length;
            int itemsExt = tmpDataIdx % _itemsPos.Length;
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:      
                    itemPos = _itemsPos[itemsExt];
                    itemPos.y -= topSeat + (itemsRect + spacingExt)  * itemsNum;
                    if (dataIdx < 0)
                        itemPos.y += contentRect - contentBorder;
                    else if (dataIdx >= _dataSet.Count)
                        itemPos.y -= contentRect - contentBorder;
                    break;
                case ScrollDir.BottomToTop:
                    itemPos = _itemsPos[itemsExt];
                    itemPos.y += topSeat + (itemsRect + spacingExt) * itemsNum;
                    if (dataIdx < 0)
                        itemPos.y -= contentRect - contentBorder;
                    else if (dataIdx >= _dataSet.Count)
                        itemPos.y += contentRect - contentBorder;
                    break;
                case ScrollDir.LeftToRight:
                    itemPos = _itemsPos[itemsExt];
                    itemPos.x += topSeat + (itemsRect + spacingExt) * itemsNum;
                    if (dataIdx < 0)
                        itemPos.x -= contentRect - contentBorder;
                    else if (dataIdx >= _dataSet.Count)
                        itemPos.x += contentRect - contentBorder;
                    break;
                case ScrollDir.RightToLeft:
                    itemPos = _itemsPos[itemsExt];
                    itemPos.x -= topSeat + (itemsRect + spacingExt) * itemsNum;
                    if (dataIdx < 0)
                        itemPos.x += contentRect - contentBorder;
                    else if (dataIdx >= _dataSet.Count)
                        itemPos.x -= contentRect - contentBorder;
                    break;
            }
            return itemPos;
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
                _itemSet[tmpItemIdx].rectTrans.anchoredPosition = SetItemPos(_sProperty.dataIdx + i);
                if (!_sProperty.isCircleEnable && _sProperty.dataIdx + i >= _dataSet.Count)
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
                else
                {
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
                }                 
            }
        }
        /// <summary>
        /// 下刷新
        /// </summary>
        private void OnRefreshItemDown()
        {
            int tmpDataIdx = (_sProperty.dataIdx + _sProperty.initItems) % _dataSet.Count;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
            _itemSet[_sProperty.itemIdx].rectTrans.anchoredPosition = SetItemPos(_sProperty.dataIdx + _sProperty.initItems);
            _itemSet[_sProperty.itemIdx].transform.SetAsLastSibling();
            _sProperty.itemIdx = (_sProperty.itemIdx + 1) % _sProperty.initItems;
            _sProperty.dataIdx = _sProperty.dataIdx + 1;
        }
        /// <summary>
        /// 上刷新
        /// </summary>
        private void OnRefreshItemUp()
        {
            _sProperty.dataIdx -= 1;
            _sProperty.itemIdx = _sProperty.itemIdx - 1 < 0 ? _sProperty.initItems - 1 : _sProperty.itemIdx - 1;
            int tmpDataIdx = _sProperty.dataIdx < 0 ? _dataSet.Count + _sProperty.dataIdx : _sProperty.dataIdx;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
            _itemSet[_sProperty.itemIdx].rectTrans.anchoredPosition = SetItemPos(_sProperty.dataIdx);
            _itemSet[_sProperty.itemIdx].transform.SetAsFirstSibling();
        }
        /// <summary>
        /// 循环刷新
        /// </summary>
        private void OnRefreshCircle()
        {
            while (nowSeat > headItemSeat + headItemLen)
            {  
                if (_sProperty.dataIdx + _sProperty.initItems >= _dataSet.Count && !_sProperty.isCircleEnable) break;
                OnRefreshItemDown();
            }
            while (nowSeat < headItemSeat)
            {               
                if (_sProperty.dataIdx <= 0 && !_sProperty.isCircleEnable) break;
                OnRefreshItemUp();
            }
            if (_sProperty.isCircleEnable)
                ToItemCircle();
        }
        /// <summary>
        /// 动画定位
        /// </summary>
        /// <param name="toSeat">真实位置</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveSeat(float toSeat)
        {
            float toCacheSeat = toSeat;
            _scrollRect.enabled = false;
            toSeat = Mathf.Clamp(toSeat, 0, footSeat);
            yield return new WaitForEndOfFrame();
            while (toSeat > headItemSeat + headItemLen && nowSeat < toSeat)
            {
                nowSeat += _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat > headItemSeat + headItemLen)
                {
                    if (!_sProperty.isCircleEnable && _sProperty.dataIdx + _sProperty.initItems >= _dataSet.Count)
                        continue;
                    OnRefreshItemDown();
                }
            }
            while (toSeat < headItemSeat && nowSeat > toSeat)
            {
                nowSeat -= _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat < headItemSeat)
                {
                    if (!_sProperty.isCircleEnable && _sProperty.dataIdx <= 0)
                        continue;
                    OnRefreshItemUp();
                }
            }
            nowSeat = toSeat;
            _scrollRect.enabled = true;
            _toLocationEvent?.Invoke();

            if (_sProperty.scrollType == ScrollType.Limit && _sProperty.isCircleEnable)
            {
                ToItemCircle();
                ToLocation(nowSeat + toCacheSeat - toSeat);
            }
        }
        /// <summary>
        /// 直接定位
        /// </summary>
        /// <param name="toSeat">真实位置</param>
        private void ToDirectSeat(float toSeat)
        {
            nowSeat = Mathf.Clamp(toSeat, 0, footSeat);
            OnRefreshCircle();
        }
        /// <summary>
        /// 动画定位
        /// </summary>
        /// <param name="toIndex">数据索引</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveIndex(int toIndex)
        {
            _scrollRect.enabled = false;
            toIndex = Mathf.Clamp(toIndex, 0, _dataSet.Count - _sProperty.initItems);
            yield return new WaitForEndOfFrame();
            while (toIndex > _sProperty.dataIdx)
            {
                nowSeat += _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat > headItemSeat + headItemLen)
                    OnRefreshItemDown();
            }
            while (toIndex < _sProperty.dataIdx)
            {
                nowSeat -= _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat < headItemSeat)
                    OnRefreshItemUp();
            }
            _scrollRect.enabled = true;
            _toLocationEvent?.Invoke();
        }
        /// <summary>
        /// 直接定位
        /// </summary>
        /// <param name="toIndex">数据索引</param>
        private void ToDirectIndex(int toIndex)
        {
            toIndex = Mathf.Clamp(toIndex, 0, _dataSet.Count - _sProperty.initItems);
            while (toIndex > _sProperty.dataIdx)
            {
                nowSeat += headItemLen;
                if (nowSeat > headItemSeat + headItemLen)
                    OnRefreshItemDown();
            }
            while (toIndex < _sProperty.dataIdx)
            {
                nowSeat -= footItemLen;
                if (nowSeat < headItemSeat)
                    OnRefreshItemUp();
            }
        }
        #endregion
    }
}