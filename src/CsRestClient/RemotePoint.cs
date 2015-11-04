using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Net;
using System.IO;

namespace CsRestClient
{
    public class RemotePoint
    {
        public string host { get; private set; }
        public Config config { get; private set; }

        private RemotePoint(string host, Config config = null)
        {
            this.host = host;

            if (config == null)
                this.config = Config.defaults;
            else
                this.config = config;
        }

        public static object RPCCall(string host, Type type, MethodInfo method, object[] args)
        {
            var request = new HttpRequest(host, type, method, args);
            var response = request.GetResponse();

            Console.WriteLine(response.statusCode);
            return response.body;
        }

        private static TypeBuilder CreateType(Type intf)
        {
            var implName = intf.Name + "Impl";

            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName(implName),
                    AssemblyBuilderAccess.Run);
            var moduleBuilder =
                assemblyBuilder.DefineDynamicModule("Module");
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

            return typeBuilder;
        }

        private static MethodInfo m = null;
        public static T Create<T>(string host)
        {
            var typeBuilder = CreateType(typeof(T));

            /* black magic */
            foreach (var method in typeof(T).GetMethods())
            {
                m = method;
                var paramTypes =
                    method.GetParameters().Select(m => m.ParameterType).ToArray();
                var methodBuilder = typeBuilder.DefineMethod(
                    method.Name,
                    MethodAttributes.Public |
                    MethodAttributes.Virtual |
                    MethodAttributes.NewSlot |
                    MethodAttributes.HideBySig |
                    MethodAttributes.Final,
                    method.ReturnType,
                    paramTypes);
                var ilGen = methodBuilder.GetILGenerator();

                /* args... -> object[] */
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

                ilGen.Emit(OpCodes.Ldloc, args);

                /* performs proxy call */
                ilGen.Emit(
                    OpCodes.Call,
                    typeof(RemotePoint).GetMethod(
                        "RPCCall",
                        BindingFlags.Static | BindingFlags.Public));
                /* return value of `RPCCall` will be automatically passed to caller,
                   but it needs to be unboxed to original type before returning. */
                Console.WriteLine(method.ReturnType.IsValueType);
                ilGen.Emit(OpCodes.Castclass, method.ReturnType);
                //ilGen.Emit(OpCodes.Ldobj, method.ReturnType);
                ilGen.Emit(OpCodes.Ret);

                typeBuilder.DefineMethodOverride(
                    methodBuilder,
                    typeof(T).GetMethod(method.Name));
            }

            Type type = typeBuilder.CreateType();
            object obj = Activator.CreateInstance(type);

            return (T)obj;
        }
    }
}
