using System;
using System.Reflection;
using UnityEngine;

namespace Dythervin.AutoAttach.Editor.Setters
{
    public class ArraySetter : AutoSetter
    {
        public override int Order => -100;
        public override bool Compatible(Type value)
        {
            return value.IsArray;
        }

        public override bool TrySetField(Component target, FieldInfo fieldInfo, AutoAttachAttribute attribute)
        {
            Type elementType = fieldInfo.FieldType.GetElementType();
            var componentArray = attribute.type switch
            {
                AutoAttachType.Children => target.GetComponentsInChildren(elementType),
                AutoAttachType.Parent => target.GetComponentsInParent(elementType),
                _ => target.GetComponents(elementType)
            };

            var prevArray = (Array)fieldInfo.GetValue(target);


            var array = prevArray != null && prevArray.Length == componentArray.Length
                ? prevArray
                : Array.CreateInstance(elementType, componentArray.Length);
            bool newValues = false;
            for (int i = 0; i < array.Length; i++)
            {
                Component value = componentArray[i];
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