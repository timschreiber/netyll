using System;

namespace Netyll.Logic.Exceptions
{
    [Serializable]
    public class PageProcessingException : Exception
    {
        internal PageProcessingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
