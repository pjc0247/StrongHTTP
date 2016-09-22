using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using CsRestClient.Utility;

// TODO : 
//      * RuntimeAssemblyBuilder를 좀더 범용적인 클래스로 변경
//        (RemotePoint과의 의존성 제거)
//      * nameof()
namespace CsRestClient
{
    internal class RuntimeAssemblyBuilder
    {
        private static Thread worker { get; set; }

        private static BlockingCollection<Task> taskQ { get; set; }

        private static AssemblyBuilder assemblyBuilder { get; set; }
        private static ModuleBuilder moduleBuilder { get; set; }

        static RuntimeAssemblyBuilder()
        {
            taskQ = new BlockingCollection<Task>();

            assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName("CsRestClient_Assembly"),
                    AssemblyBuilderAccess.Run);
            moduleBuilder =
                assemblyBuilder.DefineDynamicModule("CsRestClientModule");

            worker = new Thread(Worker);
            worker.Start();
        }

        /// <summary>
        /// 어셈블리를 다루는 작업은 모두 이 스레드에서 작업한다.
        /// </summary>
        private static void Worker()
        {
            while (true)
            {
                var task = taskQ.Take();

                task.RunSynchronously();
            }
        }
        /// <summary>
        /// 어셈블리 작업 스레드에서 실행할 작업을 올려놓는다.
        /// </summary>
        /// <typeparam name="T">리턴 타입</typeparam>
        /// <param name="task">실행할 작업</param>
        /// <returns>Task`T</returns>
        private static Task<T> Enqueue<T>(Func<T> task)
        {
            var asyncTask = new Task<T>(task);
            taskQ.Add(asyncTask);
            return asyncTask;
        }

        public static Task<Type> CreateImplAsync<T>(string host)
        {
            return Enqueue(() =>
            {
                return Create<T>(host);
            });
        }

        private static TypeBuilder CreateType(Type intf)
        {
            var implName = intf.Name + "Impl";
            var typeBuilder = moduleBuilder.DefineType(
                implName,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null,
                new Type[] { intf });

            foreach (var attr in intf.GetCustomAttributesData())
            {
                var customAttrBuilder = new CustomAttributeBuilder(
                    attr.Constructor,
                    attr.ConstructorArguments.Select(m => m.Value).ToArray());
                typeBuilder.SetCustomAttribute(customAttrBuilder);
            }

            var meta = typeBuilder.DefineField(
                "metaData", typeof(Dictionary<string, object>), FieldAttributes.Public);
            ConstructorBuilder ctor =
                typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    Type.EmptyTypes);
            ILGenerator ilGen =
                ctor.GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Newobj,
                typeof(Dictionary<string, object>).GetConstructor(new Type[] { }));
            ilGen.Emit(OpCodes.Stfld, meta);
            ilGen.Emit(OpCodes.Ret);

            return typeBuilder;
        }

        /// <summary>
        /// 주어진 인터페이스와, 해당 인터페이스의 상위 인터페이스를
        /// 모두 순회하면서 public 프로퍼티 목록을 가져온다.
        /// </summary>
        /// <param name="intf">프로퍼티 목록을 가져올 인터페이스 타입</param>
        /// <returns>프로퍼티 목록</returns>
        private static List<PropertyInfo> GetProperties(Type intf)
        {
            List<PropertyInfo> props = new List<PropertyInfo>();
            HashSet<Type> processed = new HashSet<Type>();
            var q = new Queue<Type>();
            
            q.Enqueue(intf);
            while (q.Count > 0)
            {
                var v = q.Dequeue();

                processed.Add(v);
                foreach (var i in v.GetInterfaces())
                {
                    if (processed.Contains(i))
                        continue;

                    q.Enqueue(i);
                }

                props.AddRange(v.GetProperties());
            }

            return props.Distinct().ToList();
        }

        /// <summary>
        /// 동기 HTTP 콜을 수행하는 메소드를 구현한다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="host"></param>
        /// <param name="prefix"></param>
        /// <param name="method"></param>
        /// <param name="typeBuilder"></param>
        /// <param name="fieldBuilder">
        /// 캡처를 사용하는 경우, 캡처 필드.
        /// 캡처는 Task`T 리턴 메소드를 구현할 때 사용된다.
        /// </param>
        /// <returns>생성된 메소드</returns>
        private static MethodBuilder CreateSyncMethod<T>(
            string host, string prefix, MethodInfo method, TypeBuilder typeBuilder,
            FieldBuilder fieldBuilder)
        {
            var paramTypes =
                method.GetParameters().Select(m => m.ParameterType).ToArray();
            var returnType = method.ReturnType;

            if (fieldBuilder != null)
            {
                returnType = method.ReturnType.GetGenericArguments()[0];
                paramTypes = new Type[] { };
            }

            var methodBuilder =
                typeBuilder.CreateMethod(prefix + method.Name,
                returnType, paramTypes);
            var ilGen = methodBuilder.GetILGenerator();

            /* args... -> object[] */
            int argc = 0;
            LocalBuilder args = null;
            LocalBuilder typeInfo = null;
            LocalBuilder methodInfo = null;

            if (fieldBuilder == null)
            {
                args = ilGen.DeclareLocal(typeof(object[]));
                typeInfo = ilGen.DeclareLocal(typeof(Type));
                methodInfo = ilGen.DeclareLocal(typeof(MethodInfo));

                ilGen.Emit(OpCodes.Ldc_I4, paramTypes.Length);
                ilGen.Emit(OpCodes.Newarr, typeof(object));
                ilGen.Emit(OpCodes.Stloc, args);

                foreach (var param in method.GetParameters())
                {
                    ilGen.Emit(OpCodes.Ldloc, args);
                    ilGen.Emit(OpCodes.Ldc_I4, argc);
                    ilGen.Emit(OpCodes.Ldarg, argc + 1);
                    if (paramTypes[argc].IsValueType)
                        ilGen.Emit(OpCodes.Box, paramTypes[argc]);
                    ilGen.Emit(OpCodes.Stelem_Ref);

                    argc++;
                }
            }

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldstr, host);

            var getTypeFromHandle =
                typeof(Type).GetMethod("GetTypeFromHandle");
            var getMethodFromHandle =
                typeof(MethodBase).GetMethod(
                    "GetMethodFromHandle",
                    new[] { typeof(RuntimeMethodHandle) });
            ilGen.Emit(OpCodes.Ldtoken, typeof(T));
            ilGen.Emit(OpCodes.Call, getTypeFromHandle);
            ilGen.Emit(OpCodes.Castclass, typeof(Type));
            ilGen.Emit(OpCodes.Ldtoken, method);
            ilGen.Emit(OpCodes.Call, getMethodFromHandle);
            ilGen.Emit(OpCodes.Castclass, typeof(MethodInfo));

            if (fieldBuilder == null)
                ilGen.Emit(OpCodes.Ldloc, args);
            else
            {
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, fieldBuilder);
            }

            /* performs proxy call */
            ilGen.Emit(
                OpCodes.Call,
                typeof(RemotePoint).GetMethod(
                    nameof(RemotePoint.RPCCall),
                    BindingFlags.Static | BindingFlags.Public));
            /* return value of `RPCCall` will be automatically passed to caller,
               but it needs to be unboxed to original type before returning. */
            if (returnType.IsValueType)
                ilGen.Emit(OpCodes.Unbox_Any, returnType);
            else
                ilGen.Emit(OpCodes.Castclass, returnType);
            ilGen.Emit(OpCodes.Ret);

            return methodBuilder;
        }
        private static Type Create<T>(string host)
        {
            var typeBuilder = CreateType(typeof(T));

            foreach (var prop in GetProperties(typeof(T)))
            {
                typeBuilder.CreateProperty(
                    prop.Name, prop.PropertyType,
                    prop.CanRead, prop.CanWrite);
            }

            /* black magic */
            var methods = typeof(T).GetMethods().Where(m => m.IsSpecialName == false);
            foreach (var method in methods)
            {
                MethodBuilder methodBuilder = null;

                // AsyncMethod interface
                if (method.ReturnType.IsTaskWrapped())
                {
                    // 하는 일
                    // - sync 메소드를 만들고 Task.Run으로 연결한다.
                    var captureField = typeBuilder.DefineField(
                        "_" + method.Name, typeof(object[]), FieldAttributes.Private);

                    var syncMethod = CreateSyncMethod<T>(host, "sync_", method, typeBuilder, captureField);
                    var taskRunMethod = typeof(Task)
                        .GetMethods()
                        .Where(x => x.Name == nameof(Task.Run))
                        .Where(x => x.ReturnType.IsGenericType)
                        .Where(x => x.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                        .First()
                        .MakeGenericMethod(method.ReturnType.GetGenericArguments());

                    var paramTypes =
                        method.GetParameters().Select(m => m.ParameterType).ToArray();
                    methodBuilder = typeBuilder.CreateMethod(
                        method.Name,
                        method.ReturnType, paramTypes);
                    var ilGen = methodBuilder.GetILGenerator();

                    int argc = 0;
                    var args = ilGen.DeclareLocal(typeof(object[]));
                    var typeInfo = ilGen.DeclareLocal(typeof(Type));
                    var methodInfo = ilGen.DeclareLocal(typeof(MethodInfo));

                    ilGen.Emit(OpCodes.Ldc_I4, paramTypes.Length);
                    ilGen.Emit(OpCodes.Newarr, typeof(object));
                    ilGen.Emit(OpCodes.Stloc, args);

                    foreach (var param in method.GetParameters())
                    {
                        ilGen.Emit(OpCodes.Ldloc, args);
                        ilGen.Emit(OpCodes.Ldc_I4, argc);
                        ilGen.Emit(OpCodes.Ldarg, argc + 1);
                        if (paramTypes[argc].IsValueType)
                            ilGen.Emit(OpCodes.Box, paramTypes[argc]);
                        ilGen.Emit(OpCodes.Stelem_Ref);

                        argc++;
                    }

                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldloc, args);
                    ilGen.Emit(OpCodes.Stfld, captureField);

                    var genericParams = method.GetParameters().Select(x => x.ParameterType).ToList();
                    genericParams.Add(method.ReturnType.GetGenericArguments()[0]);
                    Type funcType = typeof(Func<>);

                    var getTypeFromHandle =
                        typeof(Type).GetMethod("GetTypeFromHandle");

                    ilGen.Emit(OpCodes.Ldtoken, typeof(Func<>).MakeGenericType(
                        method.ReturnType.GetGenericArguments()[0]));
                    ilGen.Emit(OpCodes.Call, getTypeFromHandle);

                    ilGen.Emit(OpCodes.Ldarg_0); // typeof(object) for 'CreateDelegate'

                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Call, typeof(object).GetMethod("GetType"));

                    ilGen.Emit(OpCodes.Ldstr, "sync_" + method.Name);
                    ilGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetMethod", new Type[] { typeof(string) }));

                    ilGen.Emit(OpCodes.Call, typeof(Delegate).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object), typeof(MethodInfo) }));
                    ilGen.Emit(OpCodes.Castclass, typeof(Func<>).MakeGenericType(
                        method.ReturnType.GetGenericArguments()[0]));

                    ilGen.Emit(OpCodes.Call, taskRunMethod);
                    ilGen.Emit(OpCodes.Castclass, method.ReturnType);

                    ilGen.Emit(OpCodes.Ret);
                }
                // SyncMethod interface
                else
                    methodBuilder = CreateSyncMethod<T>(host, "", method, typeBuilder, null);

                typeBuilder.DefineMethodOverride(
                    methodBuilder,
                    typeof(T).GetMethod(
                        method.Name,
                        method.GetParameters().Select(m => m.ParameterType).ToArray()));
            }

            return typeBuilder.CreateType();
        }
    }
}
