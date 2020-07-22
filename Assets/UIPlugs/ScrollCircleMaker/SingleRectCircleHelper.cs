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
    /// 单行不规则滑动循环
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SingleRectCircleHelper<T> : BaseCircleHelper<T>
    {
        #region 单行所需接口
        /// <summary>
        /// 首物品实例
        /// </summary>
        private RectTransform headItemRect
        {
            get
            {
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.BottomToTop:
                    case ScrollDir.RightToLeft:
                        return _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                    default:
                        return _contentRect.GetChild(0).transform as RectTransform;
                }
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
                        return (int)headItemRect.rect.height + spacingExt;
                    default:
                        return (int)headItemRect.rect.width + spacingExt;
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
                switch (_sProperty.scrollDir)
                {
                    case ScrollDir.BottomToTop:
                    case ScrollDir.RightToLeft:
                        return _contentRect.GetChild(0).transform as RectTransform;
                    default:
                        return _contentRect.GetChild(_contentRect.childCount - 1).transform as RectTransform;
                }
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
                        return (int)footItemRect.rect.height + spacingExt;
                    default:
                        return (int)footItemRect.rect.width + spacingExt;
                }
            }
        }
        /// <summary>
        /// 物品最小高宽
        /// </summary>
        private int minItemLen
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
        /// 内容组件高宽
        /// </summary>
        private float calRectangle
        {
            get 
            {
                float calRectangle = contentBorder + _dataSet.Count * minItemLen;
                for (int i = 0; i < _sProperty.initItems; ++i)
                {
                    if (_itemSet[i].transform.localScale == Vector3.zero)
                        continue;
                    if (_scrollRect.vertical)
                        calRectangle += _itemSet[i].rectTrans.rect.height - _itemRect.rect.height;
                    else
                        calRectangle += _itemSet[i].rectTrans.rect.width - _itemRect.rect.width;
                }
                if (!_sProperty.isCircleEnable)
                    calRectangle -= spacingExt;
                return calRectangle;
            }
        }
        #endregion
        private HorizontalOrVerticalLayoutGroup _singleLayoutGroup;
        private SizeInt _wholeSize;
        /// <summary>
        /// 单行不规则构造
        /// </summary>
        /// <param name="contentTrans">内容组件</param>
        /// <param name="createItemFunc">创建物品函数</param>
        public SingleRectCircleHelper(Transform contentTrans, Func<BaseItem<T>> createItemFunc):
            base(contentTrans,createItemFunc)
        {
            _wholeSize.Width = (ushort)(_itemRect.rect.width + spacingExt);
            _wholeSize.Height = (ushort)(_itemRect.rect.height + spacingExt);
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.BottomToTop:
                    _singleLayoutGroup = _contentRect.GetComponent<VerticalLayoutGroup>() ??
                         _contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
                    _layoutGroup = _singleLayoutGroup;
                    _sProperty.initItems = (int)Math.Ceiling(_viewRect.rect.height / _wholeSize.Height) + 1;
                    _scrollRect.horizontal = false;
                    _scrollRect.vertical = true;
                    break;
                case ScrollDir.LeftToRight:
                case ScrollDir.RightToLeft:
                    _singleLayoutGroup = _contentRect.GetComponent<HorizontalLayoutGroup>() ??
                        _contentRect.gameObject.AddComponent<HorizontalLayoutGroup>();
                    _layoutGroup = _singleLayoutGroup;
                    _sProperty.initItems = (int)Math.Ceiling(_viewRect.rect.width / _wholeSize.Width) + 1;
                    _scrollRect.horizontal = true;
                    _scrollRect.vertical = false;
                    break;
            }
            if (_sProperty.initItems <= 0)
                throw new Exception("Initialize item is 0!");

            _singleLayoutGroup.childControlHeight = false;
            _singleLayoutGroup.childControlWidth = false;
            _singleLayoutGroup.childForceExpandHeight = false;
            _singleLayoutGroup.childForceExpandWidth = false;
            _singleLayoutGroup.spacing = spacingExt;
            _singleLayoutGroup.padding.top = _sProperty.TopExt;
            _singleLayoutGroup.padding.bottom = _sProperty.BottomExt;
            _singleLayoutGroup.padding.right = _sProperty.RightExt;
            _singleLayoutGroup.padding.left = _sProperty.LeftExt;
            _scrollRect.inertia = _sProperty.scrollType != ScrollType.Drag;
            OnResolveGroupEnum();
        }
        /// <summary>
        /// 解析排版
        /// </summary>
        private void OnResolveGroupEnum()
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                    _singleLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 1);
                    break;
                case ScrollDir.BottomToTop:
                    _frontDir = -1;
                    _singleLayoutGroup.childAlignment = TextAnchor.LowerLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0.5f, 0);
                    break;
                case ScrollDir.LeftToRight:
                    _frontDir = -1;
                    _singleLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(0, 0.5f);
                    break;
                case ScrollDir.RightToLeft:
                    _singleLayoutGroup.childAlignment = TextAnchor.UpperRight;
                    _contentRect.anchorMin = _contentRect.anchorMax = _contentRect.pivot = new Vector2(1, 0.5f);
                    break;
            }
        }

        protected override void OnRefreshHandler(Vector2 v2)//刷新Item的移动
        {
            base.OnRefreshHandler(v2);
            OnRefreshCircle();        
        }

        public override void OnStart(List<T> tmpDataSet = null)
        {
            base.OnStart(tmpDataSet);
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.BottomToTop:
                case ScrollDir.RightToLeft:
                    for (int i = _itemSet.Count - 2; i >= 0; --i)
                        _itemSet[i].transform.SetAsLastSibling();
                    break;
            }
            _sProperty.visibleItems = _dataSet.Count;
            if (_sProperty.isCircleEnable)
            {
                nowSeat = topSeat;
                contentSite = (int)topSeatExt;
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
            if (itemIdx < 0 || itemIdx >= _dataSet.Count)
                throw new Exception("UpdateItem Overflow!");
            _dataSet[itemIdx] = data;
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
        #region//---------------------------内置函数-------------------------------//
        /// <summary>
        /// 对齐偏移
        /// </summary>
        /// <param name="tmpItemIdx">对齐位置</param>
        private void ToItemOffset(int tmpItemIdx)
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.LeftToRight:
                    for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                        _itemSet[i].transform.SetAsLastSibling();
                    for (int i = tmpItemIdx - 1; i >= _sProperty.itemIdx; --i)
                        _itemSet[i].transform.SetAsFirstSibling();
                    break;
                default:
                    for (int i = tmpItemIdx; i < _sProperty.itemIdx; ++i)
                        _itemSet[i].transform.SetAsFirstSibling();
                    for (int i = tmpItemIdx - 1; i >= _sProperty.itemIdx; --i)
                        _itemSet[i].transform.SetAsLastSibling();
                    break;
            }
        }
        /// <summary>
        /// 物品循环自适应
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
                ToItemOffset(tmpItemIdx);
                OnRefreshItems();
                nowSeat = topSeat;
                contentSite = (int)topSeatExt;
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
            else if (isLowerDefine)
            {
                tmpForce = _scrollRect.velocity;
                _scrollRect.enabled = false;
                tmpItemIdx = _sProperty.itemIdx;
                _sProperty.dataIdx = _dataSet.Count - _sProperty.initItems;
                _sProperty.itemIdx = _sProperty.dataIdx % _sProperty.initItems;
                ToItemOffset(tmpItemIdx);
                OnRefreshItems();
                nowSeat = bottomSeat;
                contentSite = (int)(topSeatExt + _sProperty.dataIdx * minItemLen);
                _scrollRect.enabled = true;
                _scrollRect.velocity = tmpForce;
            }
        }
        /// <summary>
        /// 设置排序
        /// </summary>
        /// <param name="tmpItemIdx">设置索引</param>
        /// <param name="tmpSlideDir">滑动方向</param>
        private void SetItemSibling(int tmpItemIdx, bool tmpSlideDir)
        {
            switch (_sProperty.scrollDir)
            {
                case ScrollDir.TopToBottom:
                case ScrollDir.LeftToRight:
                    if (tmpSlideDir)
                        _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                    else
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                    break;
                default:
                    if (tmpSlideDir)
                        _itemSet[tmpItemIdx].transform.SetAsFirstSibling();
                    else
                        _itemSet[tmpItemIdx].transform.SetAsLastSibling();
                    break;
            }
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
                if (!_sProperty.isCircleEnable && _sProperty.dataIdx + i >= _dataSet.Count)
                    _itemSet[tmpItemIdx].transform.localScale = Vector3.zero;
                else
                    _itemSet[tmpItemIdx].UpdateView(_dataSet[tmpDataIdx],tmpDataIdx);
            }
            contentRectangle = calRectangle;
            _singleLayoutGroup.SetLayoutVertical();
        }
        /// <summary>
        /// 下滑动刷新物品
        /// </summary>
        private void OnRefreshItemDown()
        {
            int tmpItemLen = headItemLen,tmpDataIdx = (_sProperty.dataIdx + _sProperty.initItems) % _dataSet.Count;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + tmpDataIdx;
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[tmpDataIdx], tmpDataIdx);
            SetItemSibling(_sProperty.itemIdx, true);
            _sProperty.itemIdx = (_sProperty.itemIdx + 1) % _sProperty.initItems;
            _sProperty.dataIdx = (_sProperty.dataIdx + 1) % _dataSet.Count;
            if (isCircleStatus || _sProperty.dataIdx == 0)
                contentSite += tmpItemLen;
            else
            {
                contentRectangle += footItemLen - tmpItemLen;
                nowSeat += minItemLen - tmpItemLen;
                contentSite += minItemLen;
            }
        }
        /// <summary>
        /// 上滑动刷新物品
        /// </summary>
        private void OnRefreshItemUp()
        {
            int tmpItemLen = footItemLen;
            _sProperty.itemIdx = _sProperty.itemIdx - 1 < 0 ? _sProperty.initItems - 1 : _sProperty.itemIdx - 1;
            _sProperty.dataIdx = _sProperty.dataIdx - 1 < 0 ? _dataSet.Count - 1 : _sProperty.dataIdx - 1;
            _itemSet[_sProperty.itemIdx].gameObject.name = _baseItem.name + _sProperty.dataIdx;
            _itemSet[_sProperty.itemIdx].UpdateView(_dataSet[_sProperty.dataIdx],_sProperty.dataIdx);
            SetItemSibling(_sProperty.itemIdx,false);
            if (isCircleStatus)
                contentSite -= headItemLen;
            else
            {
                contentRectangle += headItemLen - tmpItemLen;
                nowSeat += headItemLen - minItemLen;
                contentSite -= minItemLen;
            }
        }
        /// <summary>
        /// 循环刷新物品
        /// </summary>
        private void OnRefreshCircle()
        {
            while (nowSeat > contentSite + headItemLen)
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
            while (toSeat > contentSite + headItemLen && nowSeat < toSeat)
            {
                nowSeat += _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat > contentSite + headItemLen)
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
            if (nowSeat > contentSite + headItemLen)//向下
            {
                tmpRow = (int)(nowSeat - contentSite) / minItemLen;
                contentSite += minItemLen * tmpRow;
                _sProperty.dataIdx = (_sProperty.dataIdx + tmpRow) % _dataSet.Count;
                _sProperty.itemIdx = (_sProperty.itemIdx + tmpRow) % _sProperty.initItems;
                ToItemOffset(tmpItemIdx);
            }
            else if (nowSeat < contentSite - spacingExt)
            {
                tmpRow = (int)Math.Ceiling((contentSite - nowSeat - spacingExt) / minItemLen);
                contentSite -= minItemLen * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - tmpRow % _dataSet.Count;
                _sProperty.dataIdx = _sProperty.dataIdx < 0 ? _dataSet.Count + _sProperty.dataIdx : _sProperty.dataIdx;
                _sProperty.itemIdx = _sProperty.itemIdx - tmpRow % _sProperty.initItems;
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
            toIndex = Mathf.Clamp(toIndex, 0, _dataSet.Count - _sProperty.initItems);
            yield return new WaitForEndOfFrame();
            while (toIndex > _sProperty.dataIdx)
            {
                nowSeat += _sProperty.autoMoveRatio;
                yield return new WaitForEndOfFrame();
                if (nowSeat > contentSite + headItemLen)
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
            toIndex = Mathf.Clamp(toIndex, 0, _dataSet.Count - _sProperty.initItems);
            int tmpRow,tmpItemIdx = _sProperty.itemIdx;
            if (toIndex > _sProperty.dataIdx)//向下
            {
                tmpRow = toIndex - _sProperty.dataIdx;
                nowSeat += minItemLen * tmpRow;
                contentSite += minItemLen * tmpRow;
                _sProperty.dataIdx = (_sProperty.dataIdx + tmpRow) % _dataSet.Count;
                _sProperty.itemIdx = (_sProperty.itemIdx + tmpRow) % _sProperty.initItems;
                ToItemOffset(tmpItemIdx);
            }
            else if (toIndex < _sProperty.dataIdx)
            {
                tmpRow = _sProperty.dataIdx - toIndex;
                nowSeat -= minItemLen * tmpRow;
                contentSite -= minItemLen * tmpRow;
                _sProperty.dataIdx = _sProperty.dataIdx - tmpRow % _dataSet.Count;
                _sProperty.dataIdx = _sProperty.dataIdx < 0 ? _dataSet.Count + _sProperty.dataIdx : _sProperty.dataIdx;
                _sProperty.itemIdx = _sProperty.itemIdx - tmpRow % _sProperty.initItems;
                _sProperty.itemIdx = _sProperty.itemIdx < 0 ? _sProperty.initItems + _sProperty.itemIdx : _sProperty.itemIdx;
                ToItemOffset(tmpItemIdx);
            }
            OnRefreshItems();
        }
        #endregion
    }
}
