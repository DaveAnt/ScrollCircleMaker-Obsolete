﻿//------------------------------------------------------------
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
    /// 多行规则长度滑动循环
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
        private float _timer = 0;
        /// <summary>
        /// 多行规则滑动构造函数
        /// </summary>
        /// <param name="contentTrans">content的transform组件</param>
        /// <param name="createItemFunc">创建item函数</param>
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
                throw new Exception("当前参数设置,无法容纳Item!");

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
        /// 解析GridLayoutGroup排版
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

        protected override void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            if (lockRefresh) return;
            if (_timer < _sProperty.refreshRatio) {
                _timer += Time.deltaTime;
                return;
            }

            if (_sProperty.scrollType == ScrollType.Limit)
                ToLocation(nowSeat + _sProperty.stepLen * slideDir);
            else
                OnRefreshCircle();
            _timer = 0;
        }

        public override void OnStart(List<T> tmpDataSet = null)
        {
            base.OnStart(tmpDataSet);
            _sProperty.visibleItems = _dataSet.Count;
            _cExtra.length = (ushort)Math.Ceiling(_dataSet.Count / (float)autoRanks);
            _cExtra.totalItems = (ushort)(_cExtra.length * autoRanks);
            contentRectangle = contentBorder + _cExtra.length * itemLen - spacingExt;
            if (_sProperty.isCircleEnable)
            {
                nowSeat = topSeat;
                contentSite = (int)(topSeat + spacingExt);
                contentRectangle += spacingExt;
            }
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
            ToAdaptionContent(default, itemIdx, 0);
        }

        public override void DelItem(Func<T, T, bool> seekFunc, T data)
        {
            for (int i = _dataSet.Count - 1; i >= 0; ++i)
            {
                if (seekFunc(data, _dataSet[i]))
                {
                    _dataSet.RemoveAt(i);
                    ToAdaptionContent(default, i, 0);
                    break;
                }
            }
            Debug.LogWarning("无法找到对应Item！");
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
            ToAdaptionContent(data, itemIdx, 1);
        }
        public override void UpdateItem(T data, int itemIdx)
        {
            if (itemIdx < 0 || itemIdx >= _dataSet.Count)
                throw new Exception("UpdateItem超范围！");
            _dataSet[itemIdx] = data;
            int tmpOffset = _sProperty.dataIdx > itemIdx ? _cExtra.totalItems -
                    _sProperty.dataIdx + itemIdx : itemIdx - _sProperty.dataIdx;
            if (tmpOffset < _sProperty.initItems)
            {
                int tmpItemIdx = (_sProperty.itemIdx + tmpOffset) % _sProperty.initItems;
                if (tmpItemIdx < _itemSet.Count)
                    _itemSet[tmpItemIdx].UpdateView(data, itemIdx);
            }
        }

        public override void ToLocation(float toSeat, bool isDrawEnable = true)
        {
            if (isDrawEnable)
                _sProperty.StartCoroutine(ToAutoMoveSeat(toSeat));
            else
                ToDirectSeat(toSeat);
        }
        public override void ToLocation(int toIndex, bool isDrawEnable = true)
        {
            if (isDrawEnable)
                _sProperty.StartCoroutine(ToAutoMoveIndex(toIndex));
            else
                ToDirectIndex(toIndex);
        }
        public override void ToLocation(Func<T, T, bool> seekFunc, T data, bool isDrawEnable = true)
        {
            for (int i = _dataSet.Count - 1; i >= 0; ++i)
            {
                if (seekFunc(data, _dataSet[i]))
                {
                    ToLocation(i, isDrawEnable);
                    break;
                }
            }
            Debug.LogError("匹配数据定位失败,无法找到对应数据:" + data);
        }

        #region//-------------------------------------内置函数------------------------------------------//    
        /// <summary>
        /// 计算删除0、添加1导致的位移
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="globalSeat">位置</param>
        /// <param name="opHandle">操作标志位</param>
        /// <returns></returns>
        private void ToAdaptionContent(T data, int globalSeat, byte opHandle)
        {
            if (!_firstRun) return;
            _sProperty.visibleItems = _dataSet.Count;
            lockRefresh = _sProperty.initItems >= _dataSet.Count;
            switch (opHandle)
            {
                case 0://删除
                    if (_dataSet.Count + autoRanks == _cExtra.totalItems)
                    {
                        _cExtra.length -= 1;
                        _cExtra.totalItems -= (ushort)autoRanks;
                        contentRectangle -= itemLen;
                    }
                    break;
                case 1://添加
                    if (_dataSet.Count > _cExtra.totalItems)
                    {
                        _cExtra.length += 1;
                        _cExtra.totalItems += (ushort)autoRanks;
                        contentRectangle += itemLen;
                    }
                    break;
                default:
                    Debug.LogError("ToAdaptionContent opHandle error:" + opHandle);
                    break;
            }
            OnRefreshItems();
        }
        /// <summary>
        /// 对齐偏移
        /// </summary>
        /// <param name="tmpItemIdx"></param>
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
            int tmpItemIdx; Vector2 tmpForce;
            if (isHighDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.itemIdx = _sProperty.dataIdx = 0;
                nowSeat = topSeat;
                contentSite = (int)(topSeat + spacingExt);
                _gridLayoutGroup.SetLayoutVertical();
                ToItemOffset(tmpItemIdx);
                OnRefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
            else if (isLowerDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _cExtra.totalItems - _sProperty.initItems;
                _sProperty.dataIdx = _sProperty.dataIdx < 0 ? 0 : _sProperty.dataIdx;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.initItems;
                nowSeat = bottomSeat;
                contentSite = (int)(topSeat + spacingExt + _sProperty.dataIdx * itemLen / autoRanks);
                _gridLayoutGroup.SetLayoutVertical();
                ToItemOffset(tmpItemIdx);
                OnRefreshItems();
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
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
                    if (_itemSet[tmpItemIdx].transform.localScale == Vector3.zero)
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
        /// 循环刷新
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
                if(toCacheSeat != toSeat)
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
        /// <param name="toIndex">位置索引</param>
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
        /// <param name="toIndex">位置索引</param>
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