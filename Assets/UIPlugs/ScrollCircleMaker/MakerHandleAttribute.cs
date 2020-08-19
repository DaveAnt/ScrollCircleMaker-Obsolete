using System;

namespace UIPlugs.ScrollCircleMaker
{
    public enum MakerHandle
    {
        CustomRectCircleHelper,
        MultipleRectCircleHelper,
        SingleRectCircleHelper,
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MakerHandleAttribute:Attribute
    {
        public MakerHandle makerHandle;

        public MakerHandleAttribute(MakerHandle makerHandle)
        {
            this.makerHandle = makerHandle;
        }
    }
}
