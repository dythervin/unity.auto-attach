using System;
using System.Reflection;
using UnityEngine;

namespace Dythervin.AutoAttach
{
    public abstract class AutoSetter
    {
        public abstract bool Compatible(Type value);
        public virtual int Order => 0;
        public abstract bool TrySetField(Component target, FieldInfo fieldInfo, AutoAttachAttribute attribute);
    }
}