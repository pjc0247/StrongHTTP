using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Net;
using System.IO;

namespace JsonRPC
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class Service : Attribute
    {
        public string path { get; private set; }

        public Service(string path)
        {
            this.path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HttpMethod : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Suffix : Attribute
    {
    }

    [Service("math")]
    public interface Sample
    {
        int Sum(int a, int b);
    }

    public static class CaseConv
    {
        private static string MakeDelimString(this string target, string delim)
        {
            return target.
                Select((m, i) => i > 0 && char.IsUpper(m) ? delim + m.ToString() : m.ToString())
                .Aggregate((a, b) => a + b)
                .ToLower();
        }
        public static string ToSnakeCase(this string target)
        {
            return target.MakeDelimString("_");
        }
        public static string ToUriPath(this string target)
        {
            return target.MakeDelimString("/");
        }
    }

    public class Config
    {
        public delegate string UriProcessor(Type type, MethodInfo method);

        public UriProcessor uriProcessor { get; set; }

        public static Config defaults {
            get
            {
                var config = new Config();
                return config;
            }
        }
    }
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

        private static string BuildURI(string host, Type type, MethodInfo method, object[] args)
        {
            var serviceName = type.GetCustomAttribute<Service>()?.path ?? type.Name;
            var apiName = method.Name;
            var suffix = "";

            foreach(var param in method.GetParameters())
            {
                var attr = param.GetCustomAttribute<Suffix>();

                if(attr != null)
                {
                    if (suffix.Length == 0)
                        suffix = "/";

                    suffix += args[param.Position].ToString();
                }
            }

            return $"{host}/{serviceName}/{apiName}{suffix}";
        }
        public static object RPCCall(string host, Type type, MethodInfo method, object[] args)
        {
            Console.WriteLine(type);
            Console.WriteLine(method); ;

            Console.WriteLine(BuildURI(host, type, method, args));
            foreach (var a in args)
            {
                Console.WriteLine(a);
            }

            var uri = BuildURI(host, type, method, args);
            var req = HttpWebRequest.Create(uri);
            var resp = req.GetResponse();

            var reader = new StreamReader(resp.GetResponseStream());
            Console.WriteLine(reader.ReadToEnd());

            return (object)2;
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
            foreach(var method in typeof(T).GetMethods())
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
                ilGen.Emit(OpCodes.Unbox, method.ReturnType);
                ilGen.Emit(OpCodes.Ldobj, method.ReturnType);
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

    public class Program
    {
        public static int Sum(int a,int b)
        {
            Console.WriteLine(a);
            Console.WriteLine(b);
            return a + b;
        }
        static void Main(string[] args)
        {
            Console.WriteLine(
                RemotePoint.Create<Sample>("http://wiki.nzincorp.com").Sum(1, 2));
        }
    }
}
