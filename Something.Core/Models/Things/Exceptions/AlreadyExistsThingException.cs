using System;
using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class AlreadyExistsThingException : Xeption
    {
        public AlreadyExistsThingException(Exception innerException)
            : base(message: "Thing with the same Id already exists.", innerException)
        { }
    }
}