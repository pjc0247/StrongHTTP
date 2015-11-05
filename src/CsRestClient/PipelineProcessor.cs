using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace CsRestClient
{
    using CsRestClient.Attributes;

    internal static class PipelineProcessor
    {
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

        private static IEnumerable<T> GetProcessors<T>(this HttpRequest request)
        {
            return Assembly.GetEntryAssembly().GetTypes()
                .Union(Assembly.GetCallingAssembly().GetTypes())
                .Where(m => m.GetInterface(typeof(T).Name) != null)
                .Where(m => {
                var attr = m.GetCustomAttribute<ProcessorTarget>();
                if (attr == null) return true;
                if(attr.targets.Contains(request.type)) return true;
                return attr.targets
                    .Where(n => InheritsFrom(request.type, n))
                    .Count() > 0;
                })
                .OrderBy(m => m.GetCustomAttribute<ProcessorOrder>()?.order ?? 0)
                .Select(m => (T)Activator.CreateInstance(m));
        }
        public static void ExecuteParameterProcessors(this HttpRequest request)
        {
            foreach (var processor in request.GetProcessors<IParameterProcessor>())
            {
                processor.OnParameter(request.api, request.method, request.parameterData);
            }
        }
        public static void ExecuteRequestProcessors(this HttpRequest request)
        {
            foreach (var processor in request.GetProcessors<IRequestProcessor>())
            {
                processor.OnRequest(request.api, request);
            }
        }
        public static void ExecuteResourceNameProcessors(this HttpRequest request, ref string apiName)
        {
            foreach (var processor in request.GetProcessors<INameProcessor>())
            {
                apiName = processor.OnResource(request.api, apiName);
            }
        }
        public static void ExecuteParameterNameProcessors(this HttpRequest request)
        {
            foreach (var processor in request.GetProcessors<INameProcessor>())
            {
                foreach (var param in request.parameterData)
                    param.name = processor.OnParameter(request.api, param);
            }
        }
    }
}
