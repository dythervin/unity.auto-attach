#if UNITY_EDITOR
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Setters
{
    public class ArraySetter : SetterBase
    {
        public override int Order => -100;

        public override bool Compatible(Type value)
        {
            return value.IsArray;
        }


        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public override bool TrySetField(Component target, object context, object currentValue, Type fieldType, AttachAttribute attribute, out object newValue)
        {
            Type elementType = fieldType.GetElementType();
            var componentArray = GetComponents(target, context, elementType, attribute);
            var prevArray = (Array)currentValue;

            if (!attribute.readOnly && prevArray != null && prevArray.Length > 0 && ((object[])prevArray).Any(x => x != null))
            {
                newValue = null;
                return false;
            }

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

            newValue = array;
            return newValues;
        }
    }
}
#endif