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

        public override bool TrySetField(Component target, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            if (fieldInfo.FieldType.ImplementsOrInherits(typeof(Object)))
            {
                var obj = (Object)fieldInfo.GetValue(target);
                if (obj)
                    return false;
            }
            else if (fieldInfo.GetValue(target) != null)
                return false;

            Object value = GetComponent(target.gameObject, fieldInfo.FieldType, attribute);
            fieldInfo.SetValue(target, value);
            return true;
        }

        private static Object GetComponent(GameObject target, Type type, AttachAttribute attribute)
        {
            switch (attribute.type)
            {
                case Attach.Children:
                    return target.GetComponentInChildren(type);
                case Attach.Parent:
                    return target.GetComponentInParent(type);
                case Attach.Scene:
                    return Object.FindObjectOfType(type);

                case Attach.Default:
                default:
                    Component value = target.GetComponent(type);
                    if (attribute is AttachOrAddAttribute && !value)
                        value = target.gameObject.AddComponent(type);

                    return value;
            }
        }
    }
}