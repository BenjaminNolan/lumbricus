using System;

namespace TwoWholeWorms.Lumbricus.Shared.Exceptions
{
    
    public class LumbricusException : Exception
    {
        
        public LumbricusException(string message, Exception innerException) : base(message, innerException)
        {
            // …
        }

    }

}

