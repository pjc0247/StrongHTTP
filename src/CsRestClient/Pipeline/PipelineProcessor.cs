using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace CsRestClient.Pipeline
{
    using Attributes;
    using Utility;
    
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="INameProcessor"/>
    /// <seealso cref="IParameterProcessor"/>
    internal static class PipelineProcessor
    {
        /// <summary>
        /// 요청에 적용되어야 하는 파이프라인 프로세서 목록을 가져온다.
        /// </summary>
        /// <typeparam name="T">프로세서 타입</typeparam>
        /// <param name="request">조회할 HttpRequest</param>
        /// <returns>프로세서 목록</returns>
        private static IEnumerable<T> GetProcessors<T>(this HttpRequest request)
        {
            var selfTypes = typeof(PipelineProcessor).Assembly.GetTypes();
            var referenceTypes = Config.pipelineLookups.SelectMany(x => x.GetTypes());
            var entryTypes = Assembly.GetEntryAssembly().GetTypes();

            return referenceTypes
                .Union(entryTypes)
                .Union(selfTypes)
                .Where(m => m.GetInterface(typeof(T).Name) != null)
                .Where(m => {
                    var attr = m.GetCustomAttribute<ProcessorTarget>();
                    if (attr == null) return true;
                    if(attr.targets.Contains(request.type)) return true;
                    return attr.targets
                        .Where(n => request.type.InheritsFrom(n))
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
