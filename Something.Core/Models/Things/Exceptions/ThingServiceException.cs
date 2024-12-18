using System;
using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class ThingServiceException : Xeption
    {
        public ThingServiceException(Exception innerException)
            : base(message: "Thing service error occurred, contact support.", innerException)
        { }
    }
}