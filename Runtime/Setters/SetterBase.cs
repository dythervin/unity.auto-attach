using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if ZENJECT
using Zenject;
#endif
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Setters
{
    public abstract class SetterBase
    {
        public virtual int Order => 0;
        public abstract bool Compatible(Type value);

        [Obsolete]
        public virtual bool TrySetField(Component target, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            return false;
        }

        public virtual bool TrySetField(Component monoBehaviour, object context, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            return false;
        }


        public static IReadOnlyList<Object> GetComponents(Component target, Type elementType, Attach attributeType)
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

        public static Object GetComponent(Component target, Type type, AttachAttribute attribute)
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
    }
}