using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class ThingDependencyException : Xeption
    {
        public ThingDependencyException(Xeption innerException) :
            base(message: "Thing dependency error occurred, contact support.", innerException)
        { }
    }
}