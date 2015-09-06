using System;
using TwoWholeWorms.Lumbricus.Shared.Exceptions;

namespace TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Exceptions
{
    
    public class SetMugshotException : LumbricusException
    {
        
        public SetMugshotException(string message, Exception innerException) : base(message, innerException)
        {
            // …
        }

    }

}

