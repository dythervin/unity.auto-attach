using System;
using System.Reflection;
using UnityEngine;

namespace Dythervin.AutoAttach.Setters
{
    public abstract class SetterBase : ISetterBase
    {
        public virtual int Order => 0;

        public abstract bool Compatible(Type value);

        [Obsolete]
        public virtual bool TrySetField(Component target, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            return false;
        }

        public virtual bool TrySetField(Component monoBehaviour, object context, object currentValue, Type fieldType,
            AttachAttribute attribute, out object newValue)
        {
            newValue = null;
            return false;
        }
    }
}