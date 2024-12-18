using System;
using System.Linq;
using System.Threading.Tasks;
using Something.Core.Brokers.DateTimes;
using Something.Core.Brokers.Loggings;
using Something.Core.Brokers.Storages;
using Something.Core.Models.Things;

namespace Something.Core.Services.Foundations.Things
{
    public partial class ThingService : IThingService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public ThingService(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Thing> AddThingAsync(Thing Thing) =>
            TryCatch(async () =>
            {
                ValidateThingOnAdd(Thing);

                return await this.storageBroker.InsertThingAsync(Thing);
            });

        public IQueryable<Thing> RetrieveAllThings() =>
            TryCatch(() => this.storageBroker.SelectAllThings());

        public ValueTask<Thing> RetrieveThingByIdAsync(Guid ThingId) =>
            TryCatch(async () =>
            {
                ValidateThingId(ThingId);

                Thing maybeThing = await this.storageBroker
                    .SelectThingByIdAsync(ThingId);

                ValidateStorageThing(maybeThing, ThingId);

                return maybeThing;
            });

        public ValueTask<Thing> ModifyThingAsync(Thing Thing) =>
            TryCatch(async () =>
            {
                ValidateThingOnModify(Thing);

                Thing maybeThing =
                    await this.storageBroker.SelectThingByIdAsync(Thing.Id);

                ValidateStorageThing(maybeThing, Thing.Id);
                ValidateAgainstStorageThingOnModify(inputThing: Thing, storageThing: maybeThing);

                return await this.storageBroker.UpdateThingAsync(Thing);
            });

        public ValueTask<Thing> RemoveThingByIdAsync(Guid ThingId) =>
            TryCatch(async () =>
            {
                ValidateThingId(ThingId);

                Thing maybeThing = await this.storageBroker
                    .SelectThingByIdAsync(ThingId);

                ValidateStorageThing(maybeThing, ThingId);

                return await this.storageBroker.DeleteThingAsync(maybeThing);
            });
    }
}