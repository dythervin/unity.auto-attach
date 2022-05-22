using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Editor.Setters
{
    public class ClassSetter : AutoSetter
    {
        public override int Order => 10000;
        public override bool Compatible(Type value)
        {
            return value.IsClass && !value.ImplementsOrInherits(typeof(ICollection));
        }

        public override bool TrySetField(Component target, FieldInfo fieldInfo, AutoAttachAttribute attribute)
        {
            if (fieldInfo.FieldType.ImplementsOrInherits(typeof(Object)))
            {
                var obj = (Object)fieldInfo.GetValue(target);
                if (obj)
                    return false;
            }
            else if (fieldInfo.GetValue(target) != null)
                return false;

            Component value = attribute.type switch
            {
                AutoAttachType.Children => target.GetComponentInChildren(fieldInfo.FieldType),
                AutoAttachType.Parent => target.GetComponentInParent(fieldInfo.FieldType),
                _ => target.GetComponent(fieldInfo.FieldType)
            };
            fieldInfo.SetValue(target, value);

            return true;
        }
    }
}