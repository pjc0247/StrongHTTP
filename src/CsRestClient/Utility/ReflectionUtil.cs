using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Utility
{
    static class ReflectionUtil
    {
        /// <summary>
        /// 내부 타입을 반환한다.
        /// Task[int] -> int
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type Unwrap(this Type type)
        {
            if (type.IsTaskWrapped())
                return type.GetGenericArguments()[0];
            return type;
        }

        public static bool IsTaskWrapped(this Type type)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Task<>))
                return true;
            return false;
        }

        public static bool InheritsFrom(this Type type, Type baseType)
        {
            var currentType = type;
            while (currentType != null)
            {
                if (currentType.GetInterface(baseType.Name) != null)
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }
    }
}
