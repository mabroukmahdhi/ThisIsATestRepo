using Xeptions;

namespace Something.Core.Models.Things.Exceptions
{
    public class NullThingException : Xeption
    {
        public NullThingException()
            : base(message: "Thing is null.")
        { }
    }
}