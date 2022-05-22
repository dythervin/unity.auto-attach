using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Dythervin.AutoAttach.Editor.Setters
{
    public class ListSetter : AutoSetter
    {
        public override int Order => -90;

        public override bool Compatible(Type value)
        {
            return value.ImplementsOrInherits(typeof(IList));
        }

        public override bool TrySetField(Component target, FieldInfo fieldInfo, AutoAttachAttribute attribute)
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

            while (listType.IsGenericTypeDefinition && listType.GetGenericTypeDefinition() != typeof(List<>))
            {
                listType.TryGetBaseGeneric(out listType);
            }

            Type elementType = listType.GenericTypeArguments[0];

            var array = attribute.type switch
            {
                AutoAttachType.Children => target.GetComponentsInChildren(elementType),
                AutoAttachType.Parent => target.GetComponentsInParent(elementType),
                _ => target.GetComponents(elementType)
            };
            bool newValues = false;
            for (int i = 0; i < array.Length; i++)
            {
                Component component = array[i];
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