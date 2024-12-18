using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class InvalidThingException : Xeption
    {
        public InvalidThingException()
            : base(message: "Invalid Thing. Please correct the errors and try again.")
        { }
    }
}