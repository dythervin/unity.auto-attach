using System;
using System.Diagnostics;
using System.Reflection;
using Dythervin.Core.Utils;
using UnityEngine;

namespace Dythervin.AutoAttach
{
    [AttributeUsage(AttributeTargets.Field), Conditional(Symbols.UNITY_EDITOR)]
    public class AttachAttribute : PropertyAttribute
    {
        private static readonly object[] Buffer = new object[1];
        private static readonly Type[] TypeBuffer = { typeof(object) };
        public readonly bool readOnly;
        private MethodInfo _filterMethodInfo;
        private bool _initialized;

        public AttachAttribute(Attach type = Attach.Default, bool readOnly = true)
        {
            Type = type;
            this.readOnly = readOnly;
        }

        public AttachAttribute(bool readOnly)
        {
            this.readOnly = readOnly;
        }

        public string FilterMethodName { get; set; }
        public bool IsFiltered => _filterMethodInfo != null;

        public Attach Type { get; protected set; }

        public virtual void AfterSet(object context) { }

        public virtual void BeforeSet(object context) { }

        public virtual bool Filter(object context, object obj)
        {
            if (_filterMethodInfo == null)
                return true;

            Buffer[0] = obj;
            bool valid = (bool)_filterMethodInfo.Invoke(context, Buffer);
            Buffer[0] = null;

            return valid;
        }

        public virtual void Init(Type context)
        {
            if (string.IsNullOrEmpty(FilterMethodName) || _filterMethodInfo != null)
                return;

            _filterMethodInfo = context.GetMethod(FilterMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null,
                CallingConventions.Any, TypeBuffer, null);
        }
    }

    public class AttachDynamicAttribute : AttachAttribute
    {
        private static readonly object[] Buffer = new object[1];
        private readonly string _getterName;
        private FieldInfo _typeFieldInfo;
        private MethodInfo _typeMethodInfo;

        public AttachDynamicAttribute(string attachTypeGetter, bool readOnly = true) : base(Attach.Default, readOnly)
        {
            _getterName = attachTypeGetter;
        }

        public override void BeforeSet(object context)
        {
            base.BeforeSet(context);
            Buffer[0] = context;
            Type = _typeFieldInfo != null ? (Attach)_typeFieldInfo.GetValue(context) : (Attach)_typeMethodInfo.Invoke(context, Buffer);
        }

        public override void Init(Type type)
        {
            base.Init(type);
            _typeFieldInfo = type.GetField(_getterName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            if (_typeFieldInfo == null)
                _typeMethodInfo = type.GetMethod(_getterName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null,
                    CallingConventions.Any, Array.Empty<Type>(), null);
        }
    }

    public class AttachOrAddAttribute : AttachAttribute
    {
        public AttachOrAddAttribute(bool readOnly = true) : base(readOnly) { }
    }
}