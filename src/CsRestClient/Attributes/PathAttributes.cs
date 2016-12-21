﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes.Request
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class Service : Attribute
    {
        public string path { get; }

        public Service(string path)
        {
            this.path = path;
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class Resource : Attribute
    {
        public string name { get; }

        public Resource(string name)
        {
            this.name = name;
        }
    }
}
