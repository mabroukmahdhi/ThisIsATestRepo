using System;
using System.Linq;
using System.Threading.Tasks;
using Something.Core.Models.Things;

namespace Something.Core.Brokers.Storages
{
    public partial interface IStorageBroker
    {
        ValueTask<Thing> InsertThingAsync(Thing Thing);
        IQueryable<Thing> SelectAllThings();
        ValueTask<Thing> SelectThingByIdAsync(Guid ThingId);
        ValueTask<Thing> UpdateThingAsync(Thing Thing);
        ValueTask<Thing> DeleteThingAsync(Thing Thing);
    }
}
