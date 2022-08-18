using System;
using System.Diagnostics;
using System.Reflection;
using Dythervin.Core.Utils;
using UnityEngine;

namespace Dythervin.AutoAttach
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional(Symbols.UNITY_EDITOR)]
    public class AttachAttribute : PropertyAttribute
    {
        public readonly bool readOnly;
        public Attach Type { get; protected set; }
        private bool _initialized;

        public AttachAttribute(Attach type = Attach.Default, bool readOnly = true)
        {
            this.Type = type;
            this.readOnly = readOnly;
        }

        public AttachAttribute(bool readOnly)
        {
            this.readOnly = readOnly;
        }

        public virtual void BeforeSet(object context) { }
        public virtual void AfterSet(object context) { }

        public virtual void Init(Type context) { }
    }

    public class AttachDynamicAttribute : AttachAttribute
    {
        private MethodInfo _methodInfo;
        private FieldInfo _fieldInfo;
        private readonly string _getterName;

        public AttachDynamicAttribute(string attachTypeGetter, bool readOnly = true) : base(Attach.Default, readOnly)
        {
            _getterName = attachTypeGetter;
        }

        private static readonly object[] Buffer = new object[1];

        public override void BeforeSet(object context)
        {
            base.BeforeSet(context);
            Buffer[0] = context;
            Type =  _fieldInfo != null ? (Attach)_fieldInfo.GetValue(context) : (Attach)_methodInfo.Invoke(context, Buffer);
        }

        public override void Init(Type type)
        {
            base.Init(type);
            _fieldInfo = type.GetField(_getterName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            if (_fieldInfo == null)
                _methodInfo = type.GetMethod(_getterName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null,
                    CallingConventions.Any, Array.Empty<Type>(), null);
        }
    }

    public class AttachOrAddAttribute : AttachAttribute
    {
        public AttachOrAddAttribute(bool readOnly = true) : base(readOnly) { }
    }

    /// <summary>
    /// ShortHand for Attach(Attach.Child)
    /// </summary>
    public class AttachChild : AttachAttribute
    {
        /// <summary>
        /// ShortHand for Attach(Attach.Child)
        /// </summary>
        public AttachChild(bool readOnly = true) : base(Attach.Child, readOnly) { }
    }

    /// <summary>
    /// ShortHand for Attach(Attach.Parent)
    /// </summary>
    public class AttachParent : AttachAttribute
    {
        /// <summary>
        /// ShortHand for Attach(Attach.Parent)
        /// </summary>
        public AttachParent(bool readOnly = true) : base(Attach.Parent, readOnly) { }
    }

    /// <summary>
    /// ShortHand for Attach(Attach.Scene)
    /// </summary>
    public class AttachScene : AttachAttribute
    {
        /// <summary>
        /// ShortHand for Attach(Attach.Scene)
        /// </summary>
        public AttachScene(bool readOnly = true) : base(Attach.Scene, readOnly) { }
    }
}