﻿namespace SortedStorage.Application;

using System;
using System.Runtime.Serialization;

[Serializable]
public class MoreThanOneOfTypeException : Exception
{
    public MoreThanOneOfTypeException()
    {
    }

    public MoreThanOneOfTypeException(string message) : base(message)
    {
    }

    public MoreThanOneOfTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected MoreThanOneOfTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}