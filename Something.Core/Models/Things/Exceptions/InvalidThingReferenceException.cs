using System;
using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class InvalidThingReferenceException : Xeption
    {
        public InvalidThingReferenceException(Exception innerException)
            : base(message: "Invalid Thing reference error occurred.", innerException) { }
    }
}