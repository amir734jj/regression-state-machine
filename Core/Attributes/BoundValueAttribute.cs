using System;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class BoundValueAttribute : Attribute
    {
        public string Name { get; }

        public BoundValueAttribute(string name)
        {
            Name = name;
        }
    }
}