using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Editor.Setters
{
    public class ArraySetter : AutoSetter
    {
        public override int Order => -100;

        public override bool Compatible(Type value)
        {
            return value.IsArray;
        }

        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public override bool TrySetField(Component target, FieldInfo fieldInfo, AttachAttribute attribute)
        {
            Type elementType = fieldInfo.FieldType.GetElementType();
            IReadOnlyList<Object> componentArray = attribute.type switch
            {
                Attach.Children => target.GetComponentsInChildren(elementType),
                Attach.Parent => target.GetComponentsInParent(elementType),
                Attach.Scene => Object.FindObjectsOfType(elementType),
                _ => target.GetComponents(elementType)
            };

            Array prevArray = (Array)fieldInfo.GetValue(target);
            
            Array array = prevArray != null && prevArray.Length == componentArray.Count
                ? prevArray
                : Array.CreateInstance(elementType, componentArray.Count);
            bool newValues = false;
            for (int i = 0; i < array.Length; i++)
            {
                Object value = componentArray[i];
                if (ReferenceEquals(array.GetValue(i), value))
                    continue;

                array.SetValue(componentArray[i], i);
                newValues = true;
            }

            if (!newValues)
                return false;

            if (prevArray != array)
                fieldInfo.SetValue(target, array);

            return true;
        }
    }
}