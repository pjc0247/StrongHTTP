using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes
{
    public class ProcessorTarget : Attribute
    {
        public Type[] targets;

        public ProcessorTarget(Type[] targets)
        {
            this.targets = targets;
        }
    }
}
