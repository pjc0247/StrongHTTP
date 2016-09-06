using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace CsRestClient
{
    internal class RuntimeAssemblyPool
    {
        private static object syncObject { get; set; } = new object();
        private static Dictionary<string, object> typeCache { get; set; }

        static RuntimeAssemblyPool()
        {
            typeCache = new Dictionary<string, object>();
        }

        private static string GetKey<T>(string host)
        {
            return $"{typeof(T).FullName}+{host}";
        }
        public static Type GetType<T>(string host)
        {
            var key = GetKey<T>(host);
            object value = null;

            lock (syncObject)
            {
                if (typeCache.ContainsKey(key))
                    value = typeCache[key];
                else
                    typeCache[key] = new SpinWait();
            }

            // 이미 생성됨, 캐시 사용
            if (value is Type)
                return (Type)value;
            // 값이 곧 만들어짐
            else if (value is SpinWait)
            {
                var slock = (SpinWait)value;
                while (true)
                {
                    slock.SpinOnce();

                    lock (syncObject)
                    {
                        value = typeCache[key];
                    }

                    if (value is Type)
                        return (Type)value;
                }
            }
            // 첫번째 실행, 만들어야 함
            else if (value == null)
            {
                var type = RuntimeAssemblyBuilder.CreateImplAsync<T>(host).Result;

                lock (syncObject)
                {
                    typeCache[key] = type;
                }

                return type;
            }
            else
            {
                throw new InvalidOperationException(value.ToString());
            }
        }

        public static Task<Type> GetTypeAsync<T>(string host)
        {
            // FAST-SUCCESS
            var key = GetKey<T>(host);
            lock (syncObject)
            {
                if (typeCache.ContainsKey(key))
                {
                    var value = typeCache[key];

                    if (value is Type)
                        return Task.FromResult((Type)typeCache[key]);
                }
            }

            return Task.Run(() =>
            {
                return GetType<T>(host);
            });
        }
    }
}
