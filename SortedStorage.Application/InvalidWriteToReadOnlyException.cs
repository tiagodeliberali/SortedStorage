using System;
using System.Runtime.Serialization;

namespace SortedStorage.Application
{
    [Serializable]
    internal class InvalidWriteToReadOnlyException : Exception
    {
        public InvalidWriteToReadOnlyException()
        {
        }

        public InvalidWriteToReadOnlyException(string message) : base(message)
        {
        }

        public InvalidWriteToReadOnlyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidWriteToReadOnlyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}