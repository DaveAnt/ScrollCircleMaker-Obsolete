//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://daveant.gitee.io/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using UnityEngine;

namespace UIPlugs.ScrollCircleMaker
{
    public struct Size
    {
        public float Width;
        public float Height;

        public Size(float Width,float Height)
        {
            this.Width = Width;
            this.Height = Height;
        }
    }

    public struct SizeInt
    {
        public ushort Width;
        public ushort Height;

        public SizeInt(ushort Width, ushort Height)
        {
            this.Width = Width;
            this.Height = Height;
        }
    }

    public enum ScrollSort//数据排序
    {
        FrontDir,//正方向
        FrontZDir,//正方向z
        BackDir,//反方向
        BackZDir,//反方向z
    }

    public enum ScrollDir//滑动方向
    {
        TopToBottom,
        BottomToTop,
        LeftToRight,
        RightToLeft,
    }

    public enum ScrollType//滑动限速scrollSort
    {
        Default, //默认  
        Limit, //限速 
        Drag, //拖动
    }

    public class ScrollCircleComponent : MonoBehaviour
    {
        //参数面板
        [SerializeField]
        private int _baseHelperIdx = 1;//默认是multiple
        [SerializeField]
        private string _scrollMaker;
        [SerializeField]
        private GameObject _baseItem;
        [SerializeField]
        private ScrollType _scrollType;
        [SerializeField]
        private ScrollDir _scrollDir;
        [SerializeField]
        private ScrollSort _scrollSort;
        [SerializeField]
        private float _refreshRatio;
        [SerializeField]
        private int _autoMoveRatio;
        [SerializeField]
        private RectOffset _padding;   //上下左右剩余空间
        [SerializeField]
        private Vector2Int _spacing;  //x表示左右间距,y表示上下间距
        [SerializeField]
        private bool _isUpdateEnable, _isCircleEnable;
        [SerializeField]
        private int _stepLen;
        [SerializeField]
        private Vector2[] _itemsPos;
        [SerializeField]
        private int _dataIdx = 0, _itemIdx = 0;
        [SerializeField]
        private int _visibleItems, _initItems;
        /// <summary>
        /// Item实例
        /// </summary>
        public GameObject baseItem
        {
            get {
                return _baseItem;
            }
        }
        /// <summary>
        /// 滚动类型
        /// </summary>
        public ScrollType scrollType
        {
            get {
                return _scrollType;
            }
        }
        /// <summary>
        /// 滚动方向
        /// </summary>
        public ScrollDir scrollDir
        {
            get {
                return _scrollDir;
            }
        }
        /// <summary>
        /// 数据排序
        /// </summary>
        public ScrollSort scrollSort
        {
            get {
                return _scrollSort;
            }
        }
        /// <summary>
        /// 刷新速度
        /// </summary>
        public float refreshRatio
        {
            get {
                return _refreshRatio;
            }
        }
        /// <summary>
        /// 自动移动速度
        /// </summary>
        public int autoMoveRatio
        {
            get {
                return _autoMoveRatio;
            }
        }
        /// <summary>
        /// 左余留
        /// </summary>
        public int LeftExt
        {
            get {
                return _padding.left;
            }
        }
        /// <summary>
        /// 右余留
        /// </summary>
        public int RightExt
        {
            get {
                return _padding.right;
            }
        }
        /// <summary>
        /// 顶部余留
        /// </summary>
        public int TopExt
        {
            get {
                return _padding.top;
            }
        }
        /// <summary>
        /// 底部余留
        /// </summary>
        public int BottomExt
        {
            get {
                return _padding.bottom;
            }
        }
        /// <summary>
        /// 宽间距
        /// </summary>
        public int WidthExt
        {
            get {
                return _spacing.x;
            }
        }
        /// <summary>
        /// 高间距
        /// </summary>
        public int HeightExt
        {
            get {
                return _spacing.y;
            }
        }
        /// <summary>
        /// 自定义辅助器位置
        /// </summary>
        public Vector2[] ItemsPos
        {
            get {
                return _itemsPos;
            }
            set
            {
                _itemsPos = value;
            }
        }
        /// <summary>
        /// 是否环形
        /// </summary>
        public bool isCircleEnable
        {
            get {
                return _isCircleEnable;
            }
        }
        /// <summary>
        /// 限速步数
        /// </summary>
        public int stepLen
        {
            get {
                return _stepLen;
            }
        }
        /// <summary>
        /// 物品索引
        /// </summary>
        public int itemIdx
        {
            get {
                return _itemIdx;
            }
            set {
                _itemIdx = value;
            }
        }
        /// <summary>
        /// 数据索引
        /// </summary>
        public int dataIdx
        {
            get {
                return _dataIdx;
            }
            set {
                _dataIdx = value;
            }
        }
        /// <summary>
        /// 可见物品数量
        /// </summary>
        public int visibleItems
        {
            get {
                return _visibleItems;
            }
            set {
                _visibleItems = value;
            }
        }
        /// <summary>
        /// 实例物品数量
        /// </summary>
        public int initItems
        {
            get
            {
                return _initItems;
            }
            set
            {
                _initItems = value;
            }
        }
        /// <summary>
        /// 解决方案
        /// </summary>
        public BaseMaker baseMaker { get; private set; }
        /// <summary>
        /// 反射创建解决方案
        /// </summary>
        public void Awake()
        {
            baseMaker = TypesObtainer<BaseMaker>.CreateInstanceByName(_scrollMaker);
        }
        /// <summary>
        /// 启动解决方案
        /// </summary>
        public void Start()
        {          
            baseMaker?.OnStart(transform);
        }
        /// <summary>
        /// 持续更新物品
        /// </summary>
        public void Update()
        {
            if(_isUpdateEnable) 
                baseMaker?.OnUpdate();
        }
        /// <summary>
        /// 销毁解决方案
        /// </summary>
        public void OnDestroy()
        {
            baseMaker?.OnDestroy();
            StopAllCoroutines();
        }
    }
}