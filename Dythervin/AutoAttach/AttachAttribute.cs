using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dythervin.AutoAttach
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class AttachAttribute : PropertyAttribute
    {
        public readonly bool readOnly;
        public readonly Attach type;
        private bool _initialized;

        public AttachAttribute(Attach type = Attach.Default, bool readOnly = true)
        {
            this.type = type;
            this.readOnly = readOnly;
        }
    }

    public class AttachOrAddAttribute : AttachAttribute
    {
        public AttachOrAddAttribute(bool readOnly = true) : base(readOnly: readOnly) { }
    }
}