using System;
using System.Linq;
using System.Threading.Tasks;
using Something.Core.Models.Things;

namespace Something.Core.Services.Foundations.Things
{
    public interface IThingService
    {
        ValueTask<Thing> AddThingAsync(Thing Thing);
        IQueryable<Thing> RetrieveAllThings();
        ValueTask<Thing> RetrieveThingByIdAsync(Guid ThingId);
        ValueTask<Thing> ModifyThingAsync(Thing Thing);
        ValueTask<Thing> RemoveThingByIdAsync(Guid ThingId);
    }
}