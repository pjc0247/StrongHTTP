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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CsRestClient
{
    using Utility;
    using Attributes;
    using Attributes.Response;

    public class RemotePoint
    {
        public string host { get; private set; }
        public Config config { get; private set; }

        private RemotePoint(string host, Config config = null)
        {
            this.host = host;

            //if (config == null)
                //this.config = Config.defaults;
            //else
              //  this.config = config;
        }

        public static object RPCCall(object obj, string host, Type type, MethodInfo method, object[] args)
        {
            var request = new HttpRequest(obj, host, type, method, args);
            var response = request.GetResponse();
            var returnType = method.ReturnType.Unwrap();
            object value = null;

            if (ProcessResponseAttributes(method, response, out value))
                return value;

            if (returnType == typeof(string))
                return response.body;
            else if (returnType == typeof(HttpResponse))
                return response;
            else
            {
                return JsonConvert.DeserializeObject(response.body, returnType);
            }
        }

        private static bool ProcessResponseAttributes(
            MethodInfo method, HttpResponse response,
            out object value)
        {
            var returnType = method.ReturnType.Unwrap();

            var jsonPathAttr = method.GetCustomAttribute<JsonPathAttribute>();
            if (jsonPathAttr != null)
            {
                JToken jobj = null;
                if (jsonPathAttr.isArray)
                    jobj = JArray.Parse(response.body);
                else
                    jobj = JObject.Parse(response.body);

                value = jobj.SelectToken(jsonPathAttr.jsonPath).ToObject(returnType);
                return true;
            }

            var statusCodeAttr = method.GetCustomAttribute<StatusCodeAttribute>();
            if (statusCodeAttr != null)
            {
                if (returnType == typeof(string))
                    value = response.statusCode.ToString();
                else if (returnType == typeof(int))
                    value = response.statusCode;
                else
                    throw new InvalidCastException("StatusCodeAttribute::ret type != int or string");

                return true;
            }

            value = null;
            return false;
        }

        /*  TODO : IL-EMIT 사용하는 지저분한 코드 전부 오븐으로 교체
         *  TODO : AsyncOven 생성
            
            https://github.com/pjc0247/Oven
         */
        private static TypeBuilder CreateType(Type intf)
        {
            var implName = intf.Name + "Impl";

            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName("CsRestClient_" + implName),
                    AssemblyBuilderAccess.Run);
            var moduleBuilder =
                assemblyBuilder.DefineDynamicModule("CsRestClientModule");
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
                    "RPCCall",
                    BindingFlags.Static | BindingFlags.Public));
            /* return value of `RPCCall` will be automatically passed to caller,
               but it needs to be unboxed to original type before returning. */
            ilGen.Emit(OpCodes.Castclass, returnType);
            ilGen.Emit(OpCodes.Ret);

            return methodBuilder;
        }
        public static T Create<T>(string host)
        {
            var typeBuilder = CreateType(typeof(T));

            foreach(var prop in GetProperties(typeof(T)))
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
                    var captureField = typeBuilder.DefineField(
                        "_" + method.Name, typeof(object[]), FieldAttributes.Private);

                    var syncMethod = CreateSyncMethod<T>(host, "sync_", method, typeBuilder, captureField);
                    var taskRunMethod = typeof(Task)
                        .GetMethods()
                        .Where(x => x.Name == "Run")
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

            Type type = typeBuilder.CreateType();
            object obj = Activator.CreateInstance(type);

            return (T)obj;
        }
    }
}
