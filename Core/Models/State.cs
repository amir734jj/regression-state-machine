using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Attributes;

namespace Core.Models
{
    internal class State
    {
        public bool IsAsync { get; set; }
        
        public MethodInfo MethodInfo { get; set; }
        
        public Type ReturnType { get; set; }

        public Dictionary<ParameterInfo, List<GuardAttribute>> ParameterGuards { get; set; }
        
        public List<DeclarationAttribute> Declarations { get; set; }
        
        public Dictionary<ParameterInfo, BoundValueAttribute> BoundParameters { get; set; }
        
        public List<ParameterInfo> Parameters { get; set; }
        
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{ReturnType.Name} {Name}({string.Join(',', Parameters.Select(x => x.ParameterType.Name))})";
        }
    }
}