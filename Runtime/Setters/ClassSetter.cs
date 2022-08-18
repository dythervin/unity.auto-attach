using System;
using System.Collections;
using System.Reflection;
using Dythervin.Core.Extensions;
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

        public override bool TrySetField(Component target, object context, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            if (!attribute.readOnly)
            {
                if (fieldInfo.FieldType.ImplementsOrInherits(typeof(Object)))
                {
                    var obj = (Object)fieldInfo.GetValue(context);
                    if (obj)
                        return false;
                }
                else if (fieldInfo.GetValue(context) != null)
                {
                    return false;
                }
            }

            Object value = GetComponent(target, fieldInfo.FieldType, attribute);
            fieldInfo.SetValue(context, value);

            return true;
        }
    }
}