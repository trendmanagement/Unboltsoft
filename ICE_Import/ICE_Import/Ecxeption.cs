using System;
using System.Runtime.Serialization;

namespace ICE_Import
{
    [Serializable]
    internal class Ecxeption : Exception
    {
        public Ecxeption()
        {
        }

        public Ecxeption(string message) : base(message)
        {
        }

        public Ecxeption(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected Ecxeption(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}