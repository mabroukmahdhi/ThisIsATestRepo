using System;
using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class FailedThingServiceException : Xeption
    {
        public FailedThingServiceException(Exception innerException)
            : base(message: "Failed Thing service occurred, please contact support", innerException)
        { }
    }
}