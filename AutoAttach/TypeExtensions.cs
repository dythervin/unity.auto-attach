using System;

namespace Dythervin.AutoAttach
{
    public static class TypeExtensions
    {
        public static bool ImplementsOrInherits(this Type type, Type to) => to.IsAssignableFrom(type);
        
        public static bool Instantiatable(this Type type)
        {
            return !type.IsInterface && !type.IsGenericTypeDefinition && !type.IsAbstract;
        }

        public static bool TryGetBaseGeneric(this Type type, out Type baseGenericType)
        {
            do
            {
                baseGenericType = type.BaseType;
                if (baseGenericType == null)
                    return false;
            } while (!baseGenericType.IsGenericType);

            return true;
        }
    }
}