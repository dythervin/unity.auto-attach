using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dythervin.Core.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Setters
{
    public class ListSetter : SetterBase
    {
        public override int Order => -90;

        public override bool Compatible(Type value)
        {
            return value.ImplementsOrInherits(typeof(IList));
        }

        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public override bool TrySetField(Component target, object context, object currentValue, Type fieldType, AttachAttribute attribute, out object newValue)
        {
            var list = (IList)currentValue;

            if (!attribute.readOnly && list != null && list.Count > 0 && ((IReadOnlyList<object>)list).Any(x => x != null))
            {
                newValue = null;
                return false;
            }

            if (list == null)
                list = (IList)Activator.CreateInstance(fieldType);

            newValue = list;

            Type listType = fieldType;

            while (!listType.IsGenericType)
            {
                listType.TryGetBaseGeneric(out listType);
            }

            Type elementType = listType.GenericTypeArguments[0];

            var array = GetComponents(target, context, elementType, attribute);
            bool newValues = false;
            for (int i = 0; i < array.Count; i++)
            {
                Object value = array[i];
                if (list.Count > i)
                {
                    if (ReferenceEquals(list[i], value))
                        continue;

                    list[i] = value;
                }
                else
                {
                    list.Add(value);
                }

                newValues = true;
            }

            return newValues;
        }
    }
}