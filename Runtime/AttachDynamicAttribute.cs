using System;
using System.Diagnostics;
using System.Reflection;
using Dythervin.Core.Utils;

namespace Dythervin.AutoAttach
{
    [Conditional(Symbols.UNITY_EDITOR)]
    public class AttachDynamicAttribute : AttachAttribute
    {
        private static readonly object[] SingleArrayBuffer = new object[1];
        private readonly string _getterName;
        private FieldInfo _typeFieldInfo;
        private MethodInfo _typeMethodInfo;

        public AttachDynamicAttribute(string attachTypeGetter, bool isReadOnly = true) : base(Attach.Default,
            isReadOnly)
        {
            _getterName = attachTypeGetter;
        }

        public override void BeforeSet(object context)
        {
            base.BeforeSet(context);
            SingleArrayBuffer[0] = context;
            Type = _typeFieldInfo != null ?
                (Attach)_typeFieldInfo.GetValue(context) :
                (Attach)_typeMethodInfo.Invoke(context, SingleArrayBuffer);
        }

        public override void Init(Type type)
        {
            base.Init(type);
            _typeFieldInfo = type.GetField(_getterName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            if (_typeFieldInfo == null)
            {
                _typeMethodInfo = type.GetMethod(_getterName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null,
                    CallingConventions.Any, Array.Empty<Type>(), null);
            }
        }
    }
}