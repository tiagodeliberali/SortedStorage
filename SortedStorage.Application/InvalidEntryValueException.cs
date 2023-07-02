namespace SortedStorage.Application;

using System;
using System.Runtime.Serialization;

public class InvalidEntryValueException : Exception
{
    public InvalidEntryValueException()
    {
    }

    public InvalidEntryValueException(string message) : base(message)
    {
    }

    public InvalidEntryValueException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidEntryValueException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}