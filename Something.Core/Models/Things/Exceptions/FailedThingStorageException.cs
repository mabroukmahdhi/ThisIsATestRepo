using System;
using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class FailedThingStorageException : Xeption
    {
        public FailedThingStorageException(Exception innerException)
            : base(message: "Failed Thing storage error occurred, contact support.", innerException)
        { }
    }
}