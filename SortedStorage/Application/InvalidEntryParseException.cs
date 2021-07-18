using System;
using System.Runtime.Serialization;

namespace SortedStorage.Application
{
    [Serializable]
    internal class InvalidEntryParseException : Exception
    {
        public InvalidEntryParseException()
        {
        }

        public InvalidEntryParseException(string message) : base(message)
        {
        }

        public InvalidEntryParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidEntryParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}