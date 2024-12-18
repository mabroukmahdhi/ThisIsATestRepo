using System;
using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class NotFoundThingException : Xeption
    {
        public NotFoundThingException(Guid ThingId)
            : base(message: $"Couldn't find Thing with ThingId: {ThingId}.")
        { }
    }
}