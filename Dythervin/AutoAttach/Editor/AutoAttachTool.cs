#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dythervin.AutoAttach.Setters;
using Dythervin.Utils;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Editor
{
    [EditorTool(nameof(AutoAttachTool), typeof(MonoBehaviour))]
    internal class AutoAttachTool : EditorTool
    {
        private static readonly Dictionary<Type, Dictionary<FieldInfo, AttachAttribute>> Cache = new Dictionary<Type, Dictionary<FieldInfo, AttachAttribute>>();

        private static readonly List<SetterBase> Setters = new List<SetterBase>();

        private const string EditorPrefsKey = "AutoAttachToolEnabled";
        private const string MenuItemName = "Tools/Dythervin/AutoAttach";

        public static bool IsEnabled
        {
            get => EditorPrefs.GetBool(EditorPrefsKey, true);
            set => EditorPrefs.SetBool(EditorPrefsKey, value);
        }

        [MenuItem(MenuItemName)]
        private static void Toggle()
        {
            IsEnabled = !IsEnabled;
        }

        [MenuItem(MenuItemName, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuItemName, IsEnabled);
            return true;
        }


        private void OnEnable()
        {
            if (!IsEnabled)
                return;

            foreach (Object o in targets)
                Set((MonoBehaviour)o);
        }


        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnCompile()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()
                .Where(type => type.Instantiatable() && !type.IsEnum && !type.IsPrimitive)).ToArray();


            Setters.AddRange(allTypes.Where(type => type.ImplementsOrInherits(typeof(SetterBase))).Select(type => (SetterBase)Activator.CreateInstance(type)));
            Setters.Sort((a, b) => a.Order.CompareTo(b.Order));

            Dictionary<FieldInfo, AttachAttribute> values = null;
            foreach (Type type in allTypes)
            {
                if (type.ImplementsOrInherits(typeof(MonoBehaviour)))
                {
                    values ??= new Dictionary<FieldInfo, AttachAttribute>();
                    if (!Fill(type, values))
                        continue;

                    Cache.Add(type, values);
                    values = null;
                }
            }

            Symbols.AddSymbol("AUTO_ATTACH");
        }

        private static bool Fill(Type type, IDictionary<FieldInfo, AttachAttribute> values)
        {
            while (type != typeof(MonoBehaviour))
            {
                foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (fieldInfo.IsNotSerialized
                        || fieldInfo.FieldType.IsValueType && !FindSetter(fieldInfo.FieldType, out _)
                        || fieldInfo.IsPrivate && fieldInfo.GetCustomAttribute<SerializeField>() == null)
                        continue;

                    try
                    {
                        fieldInfo.GetCustomAttributes<AttachAttribute>();
                        var attribute = fieldInfo.GetCustomAttribute<AttachAttribute>();
                        if (attribute == null)
                            continue;
                        values.Add(fieldInfo, attribute);
                    }

                    catch (AmbiguousMatchException)
                    {
                        Debug.LogError($"{type} {fieldInfo.Name} has multiple Attach Attributes");
                    }
                }

                type = type.BaseType;
            }

            return values.Count > 0;
        }


        private static bool Set(MonoBehaviour monoBehaviour)
        {
            if (!Cache.TryGetValue(monoBehaviour.GetType(), out var values))
                return false;

            int set = 0;
            foreach (var data in values)
            {
                FieldInfo fieldInfo = data.Key;

                Type fieldType = fieldInfo.FieldType;

                if (!FindSetter(fieldType, out SetterBase setter))
                    continue;

                if (setter.TrySetField(monoBehaviour, fieldInfo, data.Value))
                    set++;
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
#endif