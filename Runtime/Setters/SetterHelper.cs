using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Setters
{
    public static class SetterHelper
    {
        private static readonly List<Object> Buffer = new List<Object>();

        public static Object GetComponent(Component target, object context, Type elementType, AttachAttribute attribute)
        {
            if (!attribute.IsFiltered)
                return GetComponent(target, elementType, attribute);

            var list = GetComponents(target, elementType, attribute.Type, attribute.includeDisabled);
            for (int i = 0; i < list.Count; i++)
            {
                Object obj = list[i];
                if (attribute.Filter(context, obj))
                    return obj;
            }

            return null;
        }

        public static IReadOnlyList<Object> GetComponents(Component target, object context, Type elementType,
            AttachAttribute attribute)
        {
            Buffer.Clear();
            var raw = GetComponents(target, elementType, attribute.Type, attribute.includeDisabled);
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
                    return target.GetComponentInParent<Zenject.Context>().GetComponentInChildren(type, true);
#endif

                case Attach.Default:
                default:
                    Component value = target.GetComponent(type);
                    if (attribute is AttachOrAddAttribute && !value)
                        value = target.gameObject.AddComponent(type);

                    return value;
            }
        }

        private static IReadOnlyList<Object> GetComponents(Component target, Type elementType, Attach attributeType,
            bool includeDisabled)
        {
            switch (attributeType)
            {
                case Attach.Child:
                    return target.GetComponentsInChildren(elementType, includeDisabled);

                case Attach.Parent:
                    return target.GetComponentsInParent(elementType, includeDisabled);

                case Attach.Scene:
                    return Object.FindObjectsOfType(elementType, includeDisabled);
#if ZENJECT
                case Attach.ZenjectContext:
                    Zenject.Context context = target.GetComponentInParent<Zenject.Context>();
                    return context != null ?
                        context.GetComponentsInChildren(elementType, includeDisabled) :
                        Array.Empty<Object>();
#endif
                default:
                    return target.GetComponents(elementType);
            }
        }
    }
}