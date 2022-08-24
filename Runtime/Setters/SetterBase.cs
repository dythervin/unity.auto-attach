using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
#if ZENJECT
using Zenject;
#endif

namespace Dythervin.AutoAttach.Setters
{
    public abstract class SetterBase
    {
        private static readonly List<Object> Buffer = new List<Object>();
        public virtual int Order => 0;
        public abstract bool Compatible(Type value);

        public static Object GetComponent(Component target, object context, Type elementType, AttachAttribute attribute)
        {
            if (!attribute.IsFiltered)
                return GetComponent(target, elementType, attribute);

            var list = GetComponents(target, elementType, attribute.Type);
            for (int i = 0; i < list.Count; i++)
            {
                Object obj = list[i];
                if (attribute.Filter(context, obj))
                    return obj;
            }

            return null;
        }

        public static IReadOnlyList<Object> GetComponents(Component target, object context, Type elementType, AttachAttribute attribute)
        {
            Buffer.Clear();
            var raw = GetComponents(target, elementType, attribute.Type);
            if (!attribute.IsFiltered)
                return raw;

            for (int i = 0; i < raw.Count; i++)
            {
                Object value = raw[i];
                if (attribute.Filter(context, value))
                    Buffer.Add(value);
            }

            return Buffer;
        }

        [Obsolete]
        public virtual bool TrySetField(Component target, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            return false;
        }

        public virtual bool TrySetField(Component monoBehaviour,
            object context, object currentValue, Type fieldType, AttachAttribute attribute, out object newValue)
        {
            newValue = null;
            return false;
        }

        private static Object GetComponent(Component target, Type type, AttachAttribute attribute)
        {
            switch (attribute.Type)
            {
                case Attach.Child:
                    return target.GetComponentInChildren(type, true);
                case Attach.Parent:
                    return target.GetComponentInParent(type);
                case Attach.Scene:
                    return Object.FindObjectOfType(type, true);
#if ZENJECT
                case Attach.ZenjectContext:
                    return target.GetComponentInParent<Context>().GetComponentInChildren(type, true);
#endif

                case Attach.Default:
                default:
                    Component value = target.GetComponent(type);
                    if (attribute is AttachOrAddAttribute && !value)
                        value = target.gameObject.AddComponent(type);

                    return value;
            }
        }


        private static IReadOnlyList<Object> GetComponents(Component target, Type elementType, Attach attributeType)
        {
            switch (attributeType)
            {
                case Attach.Child:
                    return target.GetComponentsInChildren(elementType, true);
                case Attach.Parent:
                    return target.GetComponentsInParent(elementType, true);
                case Attach.Scene:
                    return Object.FindObjectsOfType(elementType, true);
#if ZENJECT
                case Attach.ZenjectContext:
                    return target.GetComponentInParent<Context>().GetComponentsInChildren(elementType, true);
#endif
                default:
                    return target.GetComponents(elementType);
            }
        }
    }
}