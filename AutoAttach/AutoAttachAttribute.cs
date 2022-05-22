using System;

namespace Dythervin.AutoAttach
{
    public enum AutoAttachType : byte
    {
        Default = 0,
        Children = 1,
        Parent = 2
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class AutoAttachAttribute : Attribute
    {
        public readonly AutoAttachType type;

        public AutoAttachAttribute(AutoAttachType type = AutoAttachType.Default)
        {
            this.type = type;
        }
    }
}