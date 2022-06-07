using System;
using System.Diagnostics;
using UnityEngine;

namespace Dythervin.AutoAttach
{
    public enum Attach : byte
    {
        Default = 0,
        Children = 1,
        Parent = 2,
        Scene = 3
    }

    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class AttachAttribute : PropertyAttribute
    {
        public readonly Attach type;
        public readonly bool readOnly;

        public AttachAttribute(Attach type = Attach.Default, bool readOnly = true)
        {
            this.type = type;
            this.readOnly = readOnly;
        }
    }
}