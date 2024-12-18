using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class ThingDependencyValidationException : Xeption
    {
        public ThingDependencyValidationException(Xeption innerException)
            : base(message: "Thing dependency validation occurred, please try again.", innerException)
        { }
    }
}