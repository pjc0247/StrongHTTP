using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Net;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

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

        public static object RPCCall(object obj, string host, Type type, MethodInfo method, object[] args)
        {
            var request = new HttpRequest(obj, host, type, method, args);
            var response = request.GetResponse();

            if (method.ReturnType == typeof(string))
                return response.body;
            else if (method.ReturnType == typeof(HttpResponse))
                return response;
            else
            {
                return JsonConvert.DeserializeObject(response.body, method.ReturnType);
            }
        }

        /*  TODO : IL-EMIT 사용하는 지저분한 코드 전부 오븐으로 교체
            
            https://github.com/pjc0247/Oven
         */
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

            foreach(var attr in intf.GetCustomAttributesData())
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
        public static T Create<T>(string host)
        {
            var typeBuilder = CreateType(typeof(T));

            foreach(var prop in GetProperties(typeof(T)))
            {
                var meta = typeBuilder.DefineField(
                    "_" + prop.Name, prop.PropertyType, FieldAttributes.Private);

                var methodBuilder = typeBuilder.DefineMethod(
                    "get_" + prop.Name,
                    MethodAttributes.Public |
                    MethodAttributes.Virtual |
                    MethodAttributes.NewSlot |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.Final,
                    prop.PropertyType,
                    null);
                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, meta);
                ilGen.Emit(OpCodes.Ret);

                methodBuilder = typeBuilder.DefineMethod(
                    "set_" + prop.Name,
                    MethodAttributes.Public |
                    MethodAttributes.Virtual |
                    MethodAttributes.NewSlot |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.Final,
                    null,
                    new Type[] { prop.PropertyType  });
                ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Stfld, meta);
                ilGen.Emit(OpCodes.Ret);
            }

            /* black magic */
            var methods = typeof(T).GetMethods().Where(m => m.IsSpecialName == false);
            foreach (var method in methods)
            {
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

                ilGen.Emit(OpCodes.Ldloc, args);

                /* performs proxy call */
                ilGen.Emit(
                    OpCodes.Call,
                    typeof(RemotePoint).GetMethod(
                        "RPCCall",
                        BindingFlags.Static | BindingFlags.Public));
                /* return value of `RPCCall` will be automatically passed to caller,
                   but it needs to be unboxed to original type before returning. */
                ilGen.Emit(OpCodes.Castclass, method.ReturnType);
                //ilGen.Emit(OpCodes.Ldobj, method.ReturnType);
                ilGen.Emit(OpCodes.Ret);

                typeBuilder.DefineMethodOverride(
                    methodBuilder,
                    typeof(T).GetMethod(
                        method.Name,
                        method.GetParameters().Select(m => m.ParameterType).ToArray()));
            }

            Type type = typeBuilder.CreateType();
            object obj = Activator.CreateInstance(type);

            return (T)obj;
        }
    }
}
