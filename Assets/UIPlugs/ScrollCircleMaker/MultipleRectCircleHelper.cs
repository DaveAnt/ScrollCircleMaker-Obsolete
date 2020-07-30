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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIPlugs.ScrollCircleMaker
{
    /// <summary>
    /// 多行规则滑动循环
    /// </summary>
    /// <typeparam name="T">数据结构</typeparam>
    public sealed class MultipleRectCircleHelper<T> : BaseCircleHelper<T>
    {
        #region 多行所需属性
        /// <summary>
        /// 自适应整体高宽
        /// </summary>
        private int itemLen
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return _wholeSize.Height;
                    default:
                        return _wholeSize.Width;
                }
            }
        }
        /// <summary>
        /// 自适应行列
        /// </summary>
        private int autoRanks
        {
            get
            {
                switch (_scrollRect.vertical)
                {
                    case true:
                        return _maxRanks.Width;
                    default:
                        return _maxRanks.Height;
                }
            }
        }
        /// <summary>
        /// 额外数据
        /// </summary>
        struct ContentExtra
        {
            public ushort length;
            public ushort totalItems;
        }
        #endregion
        private GridLayoutGroup _gridLayoutGroup;
        private SizeInt _wholeSize, _maxRanks;
        private ContentExtra _cExtra;
        /// <summary>
        /// 多行规则滑动构造
        /// </summary>
        /// <param name="contentTrans">内容组件</param>
        /// <param name="createItemFunc">创建物品函数</param>
        public MultipleRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc) :
            base(contentTrans, createItemFunc)
        {
            _gridLayoutGroup = _contentRect.GetComponent<GridLayoutGroup>()
                ?? _contentRect.gameObject.AddComponent<GridLayoutGroup>();
            _layoutGroup = _gridLayoutGroup;
            _wholeSize.Width = (ushort)(_itemRect.rect.width + _sProperty.WidthExt);
            _wholeSize.Height = (ushort)(_itemRect.rect.height + _sProperty.HeightExt);
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                    float tmpWidth = _contentRect.rect.width - _sProperty.LeftExt - _sProperty.RightExt;
                    _maxRanks.Width = (ushort)((tmpWidth + _sProperty.WidthExt) / _wholeSize.Width);
                    _maxRanks.Height = (ushort)(Math.Ceiling(_viewRect.rect.height / _wholeSize.Height) + 1);
                    _sProperty.initItems = _maxRanks.Width * _maxRanks.Height;
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                default:
                    _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                    float tmpHeight = _contentRect.rect.height - _sProperty.TopExt - _sProperty.BottomExt;
                    _maxRanks.Height = (ushort)((tmpHeight + _sProperty.HeightExt) / _wholeSize.Height);
                    _maxRanks.Width = (ushort)(Math.Ceiling(_viewRect.rect.width / _wholeSize.Width) + 1);
                    _sProperty.initItems = _maxRanks.Width * _maxRanks.Height;
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    break;
            }
            if (_sProperty.initItems <= 0)
                throw new Exception("Initialize item is 0!");

            _gridLayoutGroup.cellSize = _itemRect.rect.size;
            _gridLayoutGroup.padding.left = _sProperty.LeftExt;
            _gridLayoutGroup.padding.right = _sProperty.RightExt;
            _gridLayoutGroup.padding.top = _sProperty.TopExt;
            _gridLayoutGroup.padding.bottom = _sProperty.BottomExt;
            _gridLayoutGroup.spacing = new Vector2(_sProperty.WidthExt, _sProperty.HeightExt);
            _scrollRect.inertia = _sProperty.scrollType != ScrollType.Drag;
            OnResolveGroupEnum();
        }
        /// <summary>
        /// 解析布局排版
        /// </summary>
        private void OnResolveGroupEnum()
        {
            int sign = (short)_sProperty.scrollDir * 10 + (short)_sProperty.scrollSort;
            switch (sign)
            {
                case 0: case 2: case 20: case 22:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
                    break;
                case 1: case 3: case 30: case 32:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperRight;
                    break;
                case 10: case 12: case 21: case 23:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerLeft;
                    break;
                case 11: case 13: case 31: case 33:
                    _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerRight;
                    break;
            }
            switch (sign)
            {
                case 0: case 1: case 2: case 3:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    break;
                case 10: case 11: case 12: case 13:
                    _frontDir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    _gridLayoutGroup.childAlignment = TextAnchor.LowerLeft;
                    break;
                case 20: case 21: case 22: case 23:
                    _frontDir = -1;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    break;
                case 30: case 31: case 32: case 33:
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    _gridLayoutGroup.childAlignment = TextAnchor.UpperRight;
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
            _sProperty.visibleItems = _dataSet.Count;
            _cExtra.length = (ushort)Math.Ceiling(_dataSet.Count / (float)autoRanks);
            _cExtra.totalItems = (ushort)(_cExtra.length * autoRanks);
            contentRect = contentBorder + _cExtra.length * itemLen - spacingExt;
            if (_sProperty.isCircleEnable)
            {
                nowSeat = topSeat;
                contentSite = (int)topSeatExt;
                contentRect += spacingExt;
            }
            if(_dataSet.Count > 0)
                OnRefreshItems();
        }
        public override void DelItem(int itemIdx)
        {
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count - 1);
            _dataSet.RemoveAt(itemIdx);
            OnRefreshOwn(0);
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
            OnRefreshOwn(1);
        }
        public override void UpdateItem(T data, int itemIdx)
        {
            itemIdx = Mathf.Clamp(itemIdx, 0, _dataSet.Count - 1);
            _dataSet[itemIdx] = data;
            if (!_firstRun) return;
            int tmpOffset = _sProperty.dataIdx > itemIdx ? _cExtra.totalItems -
                    _sProperty.dataIdx + itemIdx : itemIdx - _sProperty.dataIdx;
            if (tmpOffset < _sProperty.initItems)
            {
                int tmpItemIdx = (_sProperty.itemIdx + tmpOffset) % _sProperty.initItems;
                if (tmpItemIdx < _itemSet.Count)
                    _itemSet[tmpItemIdx].UpdateView(data, itemIdx);
            }
        }
        public override void SwapItem(int firstIdx, int nextIdx)
        {
            firstIdx = Mathf.Clamp(firstIdx, 0, _dataSet.Count - 1);
            nextIdx = Mathf.Clamp(nextIdx, 0, _dataSet.Count - 1);
            if (firstIdx == nextIdx) throw new Exception("Swap Item Same!");
            T swapData = _dataSet[firstIdx];
            UpdateItem(_dataSet[nextIdx], firstIdx);
            UpdateItem(swapData,nextIdx);
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
            if (_cExtra.totalItems < _sProperty.initItems)
                Debug.LogWarning("ToLocation ItemIndex Overflow!");
            else if (isDrawEnable)
                _sProperty.StartCoroutine(ToAutoMoveIndex(toIndex));
            else
                ToDirectIndex(toIndex);
        }
        #region//-------------------------------------内置函数------------------------------------------//    
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
        /// 循环自适应
        /// </summary>
        private void ToItemCircle()
        {
            int tmpItemIdx;
            if (isHighDefine)
            {
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.itemIdx = _sProperty.dataIdx = 0;
                nowSeat = topSeat;
                contentSite = (int)topSeatExt;
                ToItemOffset(tmpItemIdx);
                OnRefreshItems();
            }
            else if (isLowerDefine)
            {
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _cExtra.totalItems - _sProperty.initItems % _cExtra.totalItems;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.initItems;
                nowSeat = bottomSeat;
                contentSite = (int)(topSeatExt + _sProperty.dataIdx * itemLen / autoRanks);
                ToItemOffset(tmpItemIdx);
                OnRefreshItems();
            }
        }
        /// <summary>
        /// 刷新当前样式
        /// </summary>
        /// <param name="opHandle">操作标志位</param>
        /// <returns></returns>
        private void OnRefreshOwn(byte opHandle)
        {
            if (!_firstRun) return;
            _sProperty.visibleItems = _dataSet.Count;
            lockRefresh = _sProperty.initItems >= _dataSet.Count;
            switch (opHandle)
            {
                case 0:
                    if (_dataSet.Count + autoRanks == _cExtra.totalItems)
                    {
                        _cExtra.length -= 1;
                        _cExtra.totalItems -= (ushort)autoRanks;
                        contentRect -= itemLen;
                    }
                    break;
                case 1:
                    if (_dataSet.Count > _cExtra.totalItems)
                    {
                        _cExtra.length += 1;
                        _cExtra.totalItems += (ushort)autoRanks;
                        contentRect += itemLen;
                    }
                    break;
                default:
                    Debug.LogError("ToAdaptionContent opHandle error:" + opHandle);
                    break;
            }
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
                tmpDataIdx = (_sProperty.dataIdx + i) % _cExtra.totalItems;
                tmpItemIdx = (_sProperty.itemIdx + i) % _sProperty.initItems;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                if (tmpDataIdx >= _dataSet.Count || (!_sProperty.isCircleEnable 
                    && _sProperty.dataIdx + i >= _cExtra.totalItems))
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
                else
                {
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
                }           
            }
            _gridLayoutGroup.SetLayoutVertical();
        }
        /// <summary>
        /// 下滑动时刷新
        /// </summary>
        private void OnRefreshItemDown()
        {
            int tmpItemIdx, tmpDataIdx , itemLen = this.itemLen, autoRanks = this.autoRanks;
            for (int i = 0; i < autoRanks; ++i)
            {
                tmpItemIdx = _sProperty.itemIdx + i;
                tmpDataIdx = (_sProperty.dataIdx + _sProperty.initItems + i) % _cExtra.totalItems;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                if (tmpDataIdx < _dataSet.Count)
                {
                    if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                        _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
                }
                else
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
                _itemSet[tmpItemIdx].transform.SetAsLastSibling();
            }
            _sProperty.dataIdx = _sProperty.dataIdx + autoRanks >=
                _dataSet.Count ? 0 : _sProperty.dataIdx + autoRanks;
            _sProperty.itemIdx = _sProperty.itemIdx + autoRanks >=
                _sProperty.initItems ? 0 : _sProperty.itemIdx + autoRanks;
            contentSite += itemLen;
            _gridLayoutGroup.SetLayoutVertical();
        }
        /// <summary>
        /// 上滑动时刷新
        /// </summary>
        private void OnRefreshItemUp()
        {
            int tmpItemIdx, tmpDataIdx, itemLen = this.itemLen, autoRanks = this.autoRanks;
            _sProperty.dataIdx = _sProperty.dataIdx - autoRanks < 0 ?
                _cExtra.totalItems - autoRanks : _sProperty.dataIdx - autoRanks;
            _sProperty.itemIdx = _sProperty.itemIdx - autoRanks < 0 ?
                _sProperty.initItems - autoRanks : _sProperty.itemIdx - autoRanks;
            for (int i = autoRanks - 1; i >= 0; --i)
            {
                tmpItemIdx = _sProperty.itemIdx + i;
                tmpDataIdx = _sProperty.dataIdx + i;
                _itemSet[tmpItemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
                if (tmpDataIdx < _dataSet.Count)
                {
                    if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
                        _itemSet[tmpItemIdx].transform.localScale = Vector3.one;
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
                }
                else
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
                _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
            }
            contentSite -= itemLen;
            _gridLayoutGroup.SetLayoutVertical();
        }
        /// <summary>
        /// 物品循环刷新
        /// </summary>
        private void OnRefreshCircle()
        {
            while (nowSeat > contentSite + itemLen)
            {
                if (_sProperty.dataIdx + _sProperty.initItems >= _dataSet.Count && !_sProperty.isCircleEnable) break;
                OnRefreshItemDown();
            }
            while (nowSeat < contentSite - spacingExt)
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
            while (toSeat > contentSite + itemLen && nowSeat < toSeat)
            {
                nowSeat += _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat > contentSite + itemLen)
                {
                    if (!_sProperty.isCircleEnable && _sProperty.dataIdx + _sProperty.initItems >= _dataSet.Count)
                        continue;
                    OnRefreshItemDown(); 
                }            
            }
            while (toSeat < contentSite - spacingExt && nowSeat > toSeat)
            {
                nowSeat -= _sProperty.autoMoveRatio;     
                yield return new WaitForEndOfFrame();
                if (nowSeat < contentSite - spacingExt)
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
            toSeat = Mathf.Clamp(toSeat, 0, footSeat);
            nowSeat = toSeat;
            int tmpItemIdx = _sProperty.itemIdx, tmpRow;
            if (nowSeat > contentSite + itemLen)//向下
            {
                tmpRow = (int)(nowSeat - contentSite) / itemLen;
                contentSite += itemLen * tmpRow;
                _sProperty.dataIdx = (_sProperty.dataIdx + autoRanks * tmpRow) % _cExtra.totalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + autoRanks * tmpRow) % _sProperty.initItems;
                ToItemOffset(tmpItemIdx);
            }
            else if (nowSeat < contentSite - spacingExt)
            {
                tmpRow = (int)Math.Ceiling((contentSite - nowSeat - spacingExt) / itemLen);
                contentSite -= itemLen * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - autoRanks * tmpRow % _cExtra.totalItems;
                _sProperty.dataIdx = _sProperty.dataIdx < 0 ? _cExtra.totalItems + _sProperty.dataIdx : _sProperty.dataIdx;
                _sProperty.itemIdx = _sProperty.itemIdx - autoRanks * tmpRow % _sProperty.initItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.initItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemOffset(tmpItemIdx);
            }
            OnRefreshItems();
        }
        /// <summary>
        /// 动画定位
        /// </summary>
        /// <param name="toIndex">数据索引</param>
        /// <returns></returns>
        private IEnumerator ToAutoMoveIndex(int toIndex)
        {
            _scrollRect.enabled = false;
            toIndex = Mathf.Clamp(toIndex, 0,_cExtra.totalItems - _sProperty.initItems);
            yield return new WaitForEndOfFrame();
            while (toIndex > _sProperty.dataIdx)
            {
                nowSeat += _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat > contentSite + itemLen)
                    OnRefreshItemDown();
            }
            while (toIndex < _sProperty.dataIdx)
            {
                nowSeat -= _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat < contentSite - spacingExt)
                    OnRefreshItemUp();
            }
            _scrollRect.enabled = true;
        }
        /// <summary>
        /// 直接定位
        /// </summary>
        /// <param name="toIndex">数据索引</param>
        private void ToDirectIndex(int toIndex)
        {
            toIndex = Mathf.Clamp(toIndex, 0, _cExtra.totalItems - _sProperty.initItems);
            int tmpItemIdx = _sProperty.itemIdx, tmpRow;
            if (toIndex > _sProperty.dataIdx)//向下
            {
                tmpRow = (toIndex - _sProperty.dataIdx) / autoRanks;
                nowSeat += itemLen * tmpRow;
                contentSite += itemLen * tmpRow;
                _sProperty.dataIdx = (_sProperty.dataIdx + autoRanks * tmpRow) % _cExtra.totalItems;
                _sProperty.itemIdx = (_sProperty.itemIdx + autoRanks * tmpRow) % _sProperty.initItems;
                ToItemOffset(tmpItemIdx);
            }
            else if (toIndex < _sProperty.dataIdx)
            {
                tmpRow = (_sProperty.dataIdx - toIndex) / autoRanks;
                nowSeat -= itemLen * tmpRow;
                contentSite -= itemLen * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - autoRanks * tmpRow % _cExtra.totalItems;
                _sProperty.dataIdx = _sProperty.dataIdx < 0 ? _cExtra.totalItems + _sProperty.dataIdx : _sProperty.dataIdx;
                _sProperty.itemIdx = _sProperty.itemIdx - autoRanks * tmpRow % _sProperty.initItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.initItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemOffset(tmpItemIdx);
            }
            OnRefreshItems();
        }
    }
    #endregion
}