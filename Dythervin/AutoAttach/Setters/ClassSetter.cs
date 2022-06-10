using System;
using System.Collections;
using System.Reflection;
using Dythervin.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Setters
{
    public class ClassSetter : SetterBase
    {
        public override int Order => 10000;

        public override bool Compatible(Type value)
        {
            return value.IsClass && !value.ImplementsOrInherits(typeof(ICollection));
        }

        public override bool TrySetField(Component target, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            if (fieldInfo.FieldType.ImplementsOrInherits(typeof(Object)))
            {
                var obj = (Object)fieldInfo.GetValue(target);
                if (obj)
                    return false;
            }
            else if (fieldInfo.GetValue(target) != null)
            {
                return false;
            }

            Object value = GetComponent(target, fieldInfo.FieldType, attribute);
            fieldInfo.SetValue(target, value);
            return true;
        }
    }
}