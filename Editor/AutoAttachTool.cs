using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dythervin.AutoAttach.Setters;
using Dythervin.Core;
using Dythervin.Core.Editor;
using Dythervin.Core.Extensions;
using Dythervin.Core.Utils;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Editor
{
    [EditorTool(nameof(AutoAttachTool), typeof(MonoBehaviour))]
    public class AutoAttachTool : EditorTool
    {
        private const string EditorPrefsKey = "AutoAttachToolEnabled";
        private const string EditorPrefsIncludeChildrenKey = "AutoAttachToolIncludeChildren";
        private const string MenuItemName = "Tools/Dythervin/AutoAttach";
        private const string MenuItemNameEnabled = MenuItemName + "/Enabled";
        private const string MenuItemNameEditorPrefsIncludeChildrenKey = MenuItemName + "/IncludeChildren";
        private static readonly Dictionary<Type, FieldData[]> Cache = new();
        private static readonly List<FieldData> FieldDataBuffer = new();
        private static ISetterBase[] _setters;
        public static readonly EditorPrefBool IsEnabled = new(EditorPrefsKey, true);
        public static readonly EditorPrefBool IncludeChildren = new(EditorPrefsIncludeChildrenKey, false);
        private static readonly HashSet<FieldInfo> Buffer = new();
        private static readonly List<MonoBehaviour> MonoBehaviourBuffer = new List<MonoBehaviour>();

        public static int MaxDepth => 7;

        private void OnEnable()
        {
            if (!IsEnabled || Application.isPlaying)
                return;

            foreach (Object o in targets)
            {
                Set((MonoBehaviour)o, IncludeChildren);
            }
        }

        [MenuItem(MenuItemNameEnabled)]
        private static void ToggleEnabled()
        {
            IsEnabled.Value = !IsEnabled;
        }

        [MenuItem(MenuItemNameEnabled, true)]
        private static bool ToggleEnabledValidate()
        {
            Menu.SetChecked(MenuItemNameEnabled, IsEnabled);
            return true;
        }

        [MenuItem(MenuItemNameEditorPrefsIncludeChildrenKey)]
        private static void ToggleIncludeChildren()
        {
            IncludeChildren.Value = !IncludeChildren;
        }

        [MenuItem(MenuItemNameEditorPrefsIncludeChildrenKey, true)]
        private static bool ToggleIncludeChildrenValidate()
        {
            Menu.SetChecked(MenuItemNameEditorPrefsIncludeChildrenKey, IncludeChildren);
            return true;
        }

        public static bool Set(MonoBehaviour monoBehaviour, bool includeChild)
        {
            Type monoBehaviourType = monoBehaviour.GetType();
            if (!Cache.TryGetValue(monoBehaviourType, out var values))
            {
                FieldDataBuffer.Clear();
                GetFieldsData(monoBehaviourType, FieldDataBuffer);
                values = FieldDataBuffer.ToArray();
                Cache.Add(monoBehaviourType, values);
                FieldDataBuffer.Clear();
            }

            bool isSet = false;
            if (values.Length > 0)
            {
                int set = 0;
                foreach (FieldData data in values)
                {
                    Type fieldType = data.Field.FieldType;

                    if (!FindSetter(fieldType, out ISetterBase setter))
                    {
                        continue;
                    }

                    object context = data.GetContext(monoBehaviour);

                    data.attribute.BeforeSet(context);
                    if (setter.TrySetField(monoBehaviour, context, data.Field.GetValue(context), data.Field.FieldType,
                            data.attribute, out object newValue))
                    {
                        data.Field.SetValue(context, newValue);
                        set++;
                    }

                    data.attribute.AfterSet(context);
                }

                if (set > 0)
                {
                    EditorUtility.SetDirty(monoBehaviour);
                }

                isSet = set > 0;
            }

            if (includeChild)
            {
                Transform transform = monoBehaviour.transform;
                int childCount = transform.childCount;
                if (childCount > 0)
                {
                    for (int i = 0; i < childCount; i++)
                    {
                        MonoBehaviourBuffer.Clear();
                        transform.GetChild(i).GetComponentsInChildren(MonoBehaviourBuffer);
                        foreach (MonoBehaviour childMono in MonoBehaviourBuffer)
                        {
                            isSet |= Set(childMono, false);
                        }

                        MonoBehaviourBuffer.Clear();
                    }
                }
            }

            return isSet;
        }

        private static bool FindSetter(Type type, out ISetterBase setter)
        {
            if (_setters == null)
            {
                setter = null;
                return false;
            }

            foreach (ISetterBase autoSetter in _setters)
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

        [DidReloadScripts]
        private static void OnCompile()
        {
            _setters = TypeHelper.AllTypes.Get(type => type.Instantiatable() && type.Implements(typeof(ISetterBase)))
                .Select(type => (ISetterBase)Activator.CreateInstance(type)).ToArray();

            Array.Sort(_setters, (a, b) => a.Order.CompareTo(b.Order));

            Symbols.AddSymbol("DYTHERVIN_AUTO_ATTACH");
        }

        private static void GetFieldsData(Type type, List<FieldData> values)
        {
            if (type == null || !type.Implements<MonoBehaviour>())
                return;

            Buffer.Clear();
            while (type != typeof(MonoBehaviour))
            {
                Fill(Buffer, type, values, 0);
                type = type.BaseType;
            }

            Buffer.Clear();
        }

        private static bool GetFieldsData(IEnumerable<FieldInfo> buffer, Type context, FieldInfo fieldInfo,
            List<FieldData> values)
        {
            if (fieldInfo.FieldType.IsValueType && !FindSetter(fieldInfo.FieldType, out _))
            {
                return false;
            }

            try
            {
                AttachAttribute attribute = fieldInfo.GetCustomAttribute<AttachAttribute>();
                if (attribute != null)
                {
                    values.Add(new FieldData(attribute, buffer.ToArray()));
                    attribute.Init(context);
                    return true;
                }
            }
            catch (AmbiguousMatchException)
            {
                DLogger.LogError($"[{context}] {fieldInfo.Name} has multiple Attach Attributes");
            }
            catch (Exception e)
            {
                DLogger.LogError($"[{context}]\n{e}");
            }

            return false;
        }

        private static void Fill(ISet<FieldInfo> buffer, Type type, List<FieldData> values, int depth)
        {
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                           BindingFlags.NonPublic))
            {
                if (fieldInfo.IsNotSerialized ||
                    (fieldInfo.IsPrivate && fieldInfo.GetCustomAttribute<SerializeField>() == null))
                {
                    continue;
                }

                buffer.Add(fieldInfo);

                if (!GetFieldsData(buffer, type, fieldInfo, values) && depth + 1 < MaxDepth &&
                    !fieldInfo.FieldType.Implements(typeof(Object)))
                {
                    Fill(buffer, fieldInfo.FieldType, values, depth + 1);
                }

                buffer.Remove(fieldInfo);
            }
        }
    }
}