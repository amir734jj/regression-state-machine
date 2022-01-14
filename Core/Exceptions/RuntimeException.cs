using System;

namespace Core.Exceptions
{
    public class RuntimeException : Exception
    {
        public RuntimeException(string message): base(message)
        {
            
        }
    }
}