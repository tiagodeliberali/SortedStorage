﻿namespace SortedStorage.Application;

using System;
using System.Runtime.Serialization;

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