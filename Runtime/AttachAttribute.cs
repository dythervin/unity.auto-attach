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
        private static readonly Type[] TypeBuffer = { typeof(object) };
        public readonly bool isReadOnly;
        public readonly bool includeDisabled;
        private MethodInfo _filterMethodInfo;
        private bool _initialized;

        public Attach Type { get; protected set; }

        public bool IsFiltered => _filterMethodInfo != null;

        public string FilterMethodName { get; set; }

        public AttachAttribute(Attach type = Attach.Default, bool isReadOnly = true, bool includeDisabled = true)
        {
            this.includeDisabled = includeDisabled;
            Type = type;
            this.isReadOnly = isReadOnly;
        }

        public AttachAttribute(bool isReadOnly, bool includeDisabled = false)
        {
            this.isReadOnly = isReadOnly;
            this.includeDisabled = includeDisabled;
        }

        public virtual void AfterSet(object context)
        {
        }

        public virtual void BeforeSet(object context)
        {
        }

        public virtual bool Filter(object context, object obj)
        {
            if (_filterMethodInfo == null)
            {
                return true;
            }

            bool valid = (bool)_filterMethodInfo.Invoke(context, new object[] { obj });

            return valid;
        }

        public virtual void Init(Type context)
        {
            if (string.IsNullOrEmpty(FilterMethodName) || _filterMethodInfo != null)
            {
                return;
            }

            _filterMethodInfo = context.GetMethod(FilterMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
                BindingFlags.FlattenHierarchy, null, CallingConventions.Any, TypeBuffer, null);
        }
    }
}