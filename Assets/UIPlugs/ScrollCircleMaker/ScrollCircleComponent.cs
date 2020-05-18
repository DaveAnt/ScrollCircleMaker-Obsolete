//------------------------------------------------------------
// ScrollCircleMaker v1.0
// Copyright © 2020 DaveAnt. All rights reserved.
// Homepage: https://dagamestudio.top/
// Github: https://github.com/DaveAnt/ScollCircleMaker
//------------------------------------------------------------
using System;
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
        public int Width;
        public int Height;

        public SizeInt(int Width, int Height)
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
    }

    public class ScrollCircleComponent : MonoBehaviour
    {
        //参数面板
        [SerializeField]
        private string _scrollMaker;
        [SerializeField]
        private GameObject _baseItem;
        [SerializeField]
        private int _baseHelperIdx;
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
        private bool _isUpdateEnable, _isCircleEnable, _isSlideEnable = true;
        [SerializeField]
        private int _limitNum;
        [SerializeField]
        private int _dataIdx = 0, _itemIdx = 0;
        [SerializeField]
        private int _maxItems, _initItems=-1;

        public GameObject baseItem
        {
            get {
                return _baseItem;
            }
        }

        public ScrollType scrollType
        {
            get {
                return _scrollType;
            }
        }

        public ScrollDir scrollDir
        {
            get {
                return _scrollDir;
            }
        }

        public ScrollSort scrollSort
        {
            get {
                return _scrollSort;
            }
        }

        public float refreshRatio
        {
            get {
                return _refreshRatio;
            }
        }

        public int autoMoveRatio
        {
            get {
                return _autoMoveRatio;
            }
        }

        public int LeftExt
        {
            get {
                return _padding.left;
            }
        }

        public int RightExt
        {
            get {
                return _padding.right;
            }
        }

        public int TopExt
        {
            get {
                return _padding.top;
            }
        }

        public int BottomExt
        {
            get {
                return _padding.bottom;
            }
        }

        public int WidthExt
        {
            get {
                return _spacing.x;
            }
        }

        public int HeightExt
        {
            get {
                return _spacing.y;
            }
        }


        public bool isCircleEnable
        {
            get {
                return _isCircleEnable;
            }
        }

        public bool isSlideEnable
        {
            get {
                return _isSlideEnable;
            }
        }

        public int limitNum
        {
            get {
                return _limitNum;
            }
        }

        public int itemIdx
        {
            get {
                return _itemIdx;
            }
            set {
                _itemIdx = value;
            }
        }

        public int dataIdx
        {
            get {
                return _dataIdx;
            }
            set {
                _dataIdx = value;
            }
        }

        public int maxItems
        {
            get {
                return _maxItems;
            }
            set {
                _maxItems = value;
            }
        }

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

        public BaseMaker baseMaker { get; private set; }

        public void Awake()
        {
            baseMaker = TypesObtainer<BaseMaker>.CreateInstanceByName(_scrollMaker);
        }

        public void Start()
        {          
            baseMaker?.OnStart(transform);
        }

        public void Update()
        {
            if(_isUpdateEnable) baseMaker?.OnUpdate();
        }

        public void OnDestroy()
        {
            baseMaker?.OnDestroy();
        }

    }
}