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

            Component value = GetComponent(target.gameObject, fieldInfo.FieldType, attribute);
            fieldInfo.SetValue(target, value);
            return true;
        }

        private static Component GetComponent(GameObject target, Type type, AutoAttachAttribute attribute)
        {
            switch (attribute.type)
            {
                case AutoAttachType.Children:
                    return target.GetComponentInChildren(type);
                case AutoAttachType.Parent:
                    return target.GetComponentInParent(type);

                case AutoAttachType.Default:
                default:
                    Component value = target.GetComponent(type);
                    if (attribute is AutoAddAttribute && !value)
                        value = target.gameObject.AddComponent(type);

                    return value;
            }
        }
    }
}