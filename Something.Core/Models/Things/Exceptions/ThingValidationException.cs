using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class ThingValidationException : Xeption
    {
        public ThingValidationException(Xeption innerException)
            : base(message: "Thing validation errors occurred, please try again.",
                  innerException)
        { }
    }
}