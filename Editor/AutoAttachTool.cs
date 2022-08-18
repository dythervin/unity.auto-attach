using System;
using System.Collections.Generic;
using Dythervin.AutoAttach.Setters;
using Dythervin.Core.Editor;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Editor
{
    [EditorTool(nameof(AutoAttachTool), typeof(MonoBehaviour))]
    internal partial class AutoAttachTool : EditorTool
    {
        private const string EditorPrefsKey = "AutoAttachToolEnabled";
        private const string MenuItemName = "Tools/Dythervin/AutoAttach";

        private static readonly Dictionary<Type, FieldData[]> Cache =
            new Dictionary<Type, FieldData[]>();

        private static readonly List<SetterBase> Setters = new List<SetterBase>();

        public static int MaxDepth => 3;

        public static readonly EditorPrefBool IsEnabled = new EditorPrefBool(EditorPrefsKey, true);


        private void OnEnable()
        {
            if (!IsEnabled || Application.isPlaying)
                return;

            foreach (Object o in targets)
                Set((MonoBehaviour)o);
        }

        [MenuItem(MenuItemName)]
        private static void Toggle()
        {
            IsEnabled.Value = !IsEnabled;
        }

        [MenuItem(MenuItemName, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuItemName, IsEnabled);
            return true;
        }


        private static bool Set(Component monoBehaviour)
        {
            if (!Cache.TryGetValue(monoBehaviour.GetType(), out var values))
                return false;

            int set = 0;
            foreach (FieldData data in values)
            {
                Type fieldType = data.Field.FieldType;

                if (!FindSetter(fieldType, out SetterBase setter))
                    continue;

                object context = data.GetContext(monoBehaviour);

                data.attribute.BeforeSet(context);
                if (setter.TrySetField(monoBehaviour, context, data.Field, data.attribute))
                    set++;
                data.attribute.AfterSet(context);
            }

            if (set > 0)
                EditorUtility.SetDirty(monoBehaviour);

            return set > 0;
        }

        private static bool FindSetter(Type type, out SetterBase setter)
        {
            foreach (SetterBase autoSetter in Setters)
            {
                if (autoSetter.Compatible(type))
                {
                    setter = autoSetter;
                    return true;
                }
            }

            setter = null;
            return false;
        }
    }
}