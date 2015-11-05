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
        private static IEnumerable<T> GetProcessors<T>(this HttpRequest request)
        {
            return Assembly.GetEntryAssembly().GetTypes()
                .Where(m => m.GetInterface(nameof(T)) != null)
                .Where(m => m.GetCustomAttribute<ProcessorTarget>()?.targets.Contains(request.type) ?? true)
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
        public static void ExecuteNameProcessors(this HttpRequest request, ref string apiName)
        {
            foreach (var processor in request.GetProcessors<INameProcessor>())
            {
                apiName = processor.OnResource(apiName);
            }
        }
    }
}
