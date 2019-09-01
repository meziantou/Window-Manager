using System;
using System.Runtime.Serialization;

namespace WindowManager.Core
{
    public sealed class HotKeyAleadyInUseException : Exception
    {
        public HotKeyAleadyInUseException(string message) : base(message)
        {
        }

        public HotKeyAleadyInUseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private HotKeyAleadyInUseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HotKeyAleadyInUseException()
        {
        }
    }
}