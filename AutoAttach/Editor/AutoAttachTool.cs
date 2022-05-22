using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Editor
{
    [EditorTool(nameof(AutoAttachTool), typeof(MonoBehaviour))]
    internal class AutoAttachTool : EditorTool
    {
        private static readonly Dictionary<Type, Dictionary<FieldInfo, AutoAttachAttribute>> Cache =
            new Dictionary<Type, Dictionary<FieldInfo, AutoAttachAttribute>>();

        private static readonly List<AutoSetter> Setters = new List<AutoSetter>();

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

        private static void AddSymbol()
        {
            const string define = "AUTO_ATTACH";
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (defines.Contains(define))
                return;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, $"{defines};{define}");
        }


        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnCompile()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()
                .Where(type => type.Instantiatable() && !type.IsEnum && !type.IsPrimitive)).ToArray();


            Setters.AddRange(allTypes.Where(type => type.ImplementsOrInherits(typeof(AutoSetter))).Select(type => (AutoSetter)Activator.CreateInstance(type)));
            Setters.Sort((a, b) => a.Order.CompareTo(b.Order));

            Dictionary<FieldInfo, AutoAttachAttribute> values = null;
            foreach (Type type in allTypes)
            {
                if (type.ImplementsOrInherits(typeof(MonoBehaviour)))
                {
                    values ??= new Dictionary<FieldInfo, AutoAttachAttribute>();
                    if (!Fill(type, values))
                        continue;

                    Cache.Add(type, values);
                    values = null;
                }
            }

            AddSymbol();
        }

        private static bool Fill(IReflect type, IDictionary<FieldInfo, AutoAttachAttribute> values)
        {
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (fieldInfo.IsNotSerialized
                    || fieldInfo.FieldType.IsValueType && !FindSetter(fieldInfo.FieldType, out _)
                    || fieldInfo.IsPrivate && fieldInfo.GetCustomAttribute<SerializeField>() == null)
                    continue;

                var attribute = fieldInfo.GetCustomAttribute<AutoAttachAttribute>();
                if (attribute == null)
                    continue;

                values.Add(fieldInfo, attribute);
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

                if (!FindSetter(fieldType, out AutoSetter setter))
                    continue;

                if (setter.TrySetField(monoBehaviour, fieldInfo, data.Value))
                    set++;
            }

            if (set > 0)
                EditorUtility.SetDirty(monoBehaviour);

            return set > 0;
        }

        private static bool FindSetter(Type type, out AutoSetter setter)
        {
            foreach (AutoSetter autoSetter in Setters)
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