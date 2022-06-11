using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Setters
{
    public abstract class SetterBase
    {
        private static readonly List<Object> ListBuffer = new List<Object>();
        public virtual int Order => 0;
        public abstract bool Compatible(Type value);
        public abstract bool TrySetField(Component target, FieldInfo fieldInfo, AttachAttribute attribute);


        public static IReadOnlyList<Object> GetComponents(Component target, Type elementType, Attach attributeType)
        {
            return attributeType switch
            {
                Attach.Child => target.GetComponentsInChildren(elementType),
                Attach.Parent => target.GetComponentsInParent(elementType),
                Attach.Scene => (IReadOnlyList<Object>)Object.FindObjectsOfType(elementType),
                _ => target.GetComponents(elementType)
            };
        }

        public static Object GetComponent(Component target, Type type, AttachAttribute attribute)
        {
            switch (attribute.type)
            {
                case Attach.Child:
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