using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Data.SqlClient;
using Moq;
using Something.Core.Brokers.DateTimes;
using Something.Core.Brokers.Loggings;
using Something.Core.Brokers.Storages;
using Something.Core.Models.Things;
using Something.Core.Services.Foundations.Things;
using Tynamix.ObjectFiller;
using Xeptions;
using Xunit;

namespace Something.Core.Tests.Unit.Services.Foundations.Things
{
    public partial class ThingServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IThingService ThingService;

        public ThingServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.ThingService = new ThingService(
                storageBroker: this.storageBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

         private static IQueryable<Thing> CreateRandomThings()
        {
            return CreateThingFiller(GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                    .AsQueryable();
        }

        private static Thing CreateRandomModifyThing(DateTimeOffset dates)
        {
            int randomDaysInPast = GetRandomNegativeNumber();
            Thing randomThing = CreateRandomThing(dates);

            randomThing.CreatedDate =
                randomThing.CreatedDate.AddDays(randomDaysInPast);

            return randomThing;
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomMessage() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        public static TheoryData MinutesBeforeOrAfter()
        {
            int randomNumber = GetRandomNumber();
            int randomNegativeNumber = GetRandomNegativeNumber();

            return new TheoryData<int>
            {
                randomNumber,
                randomNegativeNumber
            };
        }

        private static SqlException GetSqlException() =>
            (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Thing CreateRandomThing() =>
            CreateThingFiller(dateTimeOffset: GetRandomDateTimeOffset()).Create();

        private static Thing CreateRandomThing(DateTimeOffset dateTimeOffset) =>
            CreateThingFiller(dateTimeOffset).Create();

        private static Filler<Thing> CreateThingFiller(DateTimeOffset dateTimeOffset)
        {
            Guid userId = Guid.NewGuid();
            var filler = new Filler<Thing>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnProperty(Thing => Thing.CreatedByUserId).Use(userId)
                .OnProperty(Thing => Thing.UpdatedByUserId).Use(userId);

            // TODO: Complete the filler setup e.g. ignore related properties etc...

            return filler;
        }
    }
}