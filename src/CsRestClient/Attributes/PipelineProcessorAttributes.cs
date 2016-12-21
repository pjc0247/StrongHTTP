using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes
{
    public class ProcessorTarget : Attribute
    {
        public Type[] targets { get; }

        public ProcessorTarget(Type target)
        {
            this.targets = new Type[] { target };
        }
        public ProcessorTarget(Type[] targets)
        {
            this.targets = targets;
        }
    }

    public class ProcessorOrder : Attribute
    {
        public int order { get; }

        public ProcessorOrder(int order)
        {
            this.order = order;
        }
    }
}
