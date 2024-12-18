using System;
using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class LockedThingException : Xeption
    {
        public LockedThingException(Exception innerException)
            : base(message: "Locked Thing record exception, please try again later", innerException)
        {
        }
    }
}