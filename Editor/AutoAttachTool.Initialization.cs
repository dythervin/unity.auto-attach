using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dythervin.AutoAttach.Setters;
using Dythervin.Core.Extensions;
using Dythervin.Core.Utils;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.AutoAttach.Editor
{
    internal partial class AutoAttachTool
    {
        [DidReloadScripts]
        private static void OnCompile()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()
                .Where(type => type.Instantiatable() && !type.IsEnum && !type.IsPrimitive)).ToArray();

            Dictionary<Type, List<FieldData>> cache =
                new Dictionary<Type, List<FieldData>>();


            Setters.AddRange(allTypes.Where(type => type.ImplementsOrInherits(typeof(SetterBase))).Select(type => (SetterBase)Activator.CreateInstance(type)));
            Setters.Sort((a, b) => a.Order.CompareTo(b.Order));
            var buffer = new HashSet<FieldInfo>();
            List<FieldData> values = null;
            foreach (Type type in allTypes)
            {
                if (type.ImplementsOrInherits(typeof(MonoBehaviour)))
                {
                    values ??= new List<FieldData>();
                    Fill(buffer, type, values);
                    if (values.Count == 0)
                        continue;

                    cache.Add(type, values);
                    values = null;
                }
            }

            foreach (var pair in cache)
                Cache[pair.Key] = pair.Value.ToArray();

            Symbols.AddSymbol("DYTHERVIN_AUTO_ATTACH");
        }

        private static void Fill(ISet<FieldInfo> buffer, Type type, List<FieldData> values)
        {
            while (type != typeof(MonoBehaviour))
            {
                Fill(buffer, type, values, 0);
                type = type.BaseType;
            }
        }

        private static bool Fill(IEnumerable<FieldInfo> buffer, Type context, FieldInfo fieldInfo, List<FieldData> values)
        {
            if (!fieldInfo.FieldType.IsValueType || FindSetter(fieldInfo.FieldType, out _))
            {
                try
                {
                    var attribute = fieldInfo.GetCustomAttribute<AttachAttribute>();
                    if (attribute != null)
                    {
                        values.Add(new FieldData(attribute, buffer.ToArray()));
                        attribute.Init(context);
                        return true;
                    }
                }
                catch (AmbiguousMatchException)
                {
                    Debug.LogError($"[{context}] {fieldInfo.Name} has multiple Attach Attributes");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{context}]\n{e}");
                }
            }

            return false;
        }

        private static void Fill(ISet<FieldInfo> buffer, Type type, List<FieldData> values, int depth)
        {
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (fieldInfo.IsNotSerialized || (fieldInfo.IsPrivate && fieldInfo.GetCustomAttribute<SerializeField>() == null))
                    continue;

                buffer.Add(fieldInfo);

                if (!Fill(buffer, type, fieldInfo, values) && depth + 1 < MaxDepth && !fieldInfo.FieldType.ImplementsOrInherits(typeof(Object)))
                    Fill(buffer, fieldInfo.FieldType, values, depth + 1);

                buffer.Remove(fieldInfo);
            }
        }
    }
}