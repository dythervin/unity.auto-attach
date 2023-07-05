using System;
using System.Collections;
using Dythervin.Core.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Setters
{
    public class ClassSetter : SetterBase
    {
        public override int Order => 10000;

        public override bool Compatible(Type value)
        {
            return value.IsClass && !value.Implements(typeof(ICollection));
        }

        public override bool TrySetField(Component target, object context, object currentValue, Type fieldType, AttachAttribute attribute, out object newValue)
        {
            if (!attribute.isReadOnly && currentValue != null)
            {
                if (currentValue is Object obj)
                {
                    if (obj)
                    {
                        newValue = null;
                        return false;
                    }
                }
                else
                {
                    newValue = null;
                    return false;
                }
            }

            newValue = SetterHelper.GetComponent(target, context, fieldType, attribute);
            return currentValue != newValue;
        }
    }
}