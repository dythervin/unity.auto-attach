using System;
using UnityEngine;

namespace Dythervin.AutoAttach.Setters
{
    public interface ISetterBase
    {
        int Order { get; }

        bool Compatible(Type type);

        bool TrySetField(Component monoBehaviour, object context, object getValue, Type fieldFieldType, AttachAttribute dataAttribute, out object newValue);
    }
}