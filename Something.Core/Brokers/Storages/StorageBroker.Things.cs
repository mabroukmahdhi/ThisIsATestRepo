using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Something.Core.Models.Things;

namespace Something.Core.Brokers.Storages
{
    public partial class StorageBroker
    {
        public DbSet<Thing> Things { get; set; }

        public async ValueTask<Thing> InsertThingAsync(Thing Thing)
        {
            EntityEntry<Thing> ThingEntityEntry =
                await Things.AddAsync(Thing);

            await SaveChangesAsync();

            return ThingEntityEntry.Entity;
        }

        public IQueryable<Thing> SelectAllThings() => this.Things;

        public async ValueTask<Thing> SelectThingByIdAsync(Guid ThingId) =>
            await Things.FindAsync(ThingId);

        public async ValueTask<Thing> UpdateThingAsync(Thing Thing)
        {
            EntityEntry<Thing> ThingEntityEntry =
                Things.Update(Thing);

            await SaveChangesAsync();

            return ThingEntityEntry.Entity;
        }

        public async ValueTask<Thing> DeleteThingAsync(Thing Thing)
        {
            EntityEntry<Thing> ThingEntityEntry =
                Things.Remove(Thing);

            await SaveChangesAsync();

            return ThingEntityEntry.Entity;
        }
    }
}
