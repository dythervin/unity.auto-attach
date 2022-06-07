using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Editor.Setters
{
    public class ListSetter : AutoSetter
    {
        public override int Order => -90;

        public override bool Compatible(Type value)
        {
            return value.ImplementsOrInherits(typeof(IList));
        }

        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public override bool TrySetField(Component target, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            bool setValue;
            IList list = (IList)fieldInfo.GetValue(target);

            if (list == null)
            {
                list = (IList)Activator.CreateInstance(fieldInfo.FieldType);
                setValue = true;
            }
            else
                setValue = false;

            Type listType = fieldInfo.FieldType;

            while (!listType.IsGenericType)
            {
                listType.TryGetBaseGeneric(out listType);
            }

            Type elementType = listType.GenericTypeArguments[0];

            IReadOnlyList<Object> array = attribute.type switch
            {
                Attach.Children => target.GetComponentsInChildren(elementType),
                Attach.Parent => target.GetComponentsInParent(elementType),
                Attach.Scene => Object.FindObjectsOfType(elementType),
                _ => target.GetComponents(elementType)
            };
            bool newValues = false;
            for (int i = 0; i < array.Count; i++)
            {
                Object component = array[i];
                if (list.Count > i)
                {
                    if (ReferenceEquals(list[i], component))
                        continue;

                    newValues = true;
                    list[i] = component;
                }
                else
                {
                    list.Add(component);
                    newValues = true;
                }
            }

            if (!newValues)
                return false;

            if (setValue)
                fieldInfo.SetValue(target, list);

            return true;
        }
    }
}