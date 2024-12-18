using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using Something.Core.Models.Things;
using Xunit;

namespace Something.Core.Tests.Unit.Services.Foundations.Things
{
    public partial class ThingServiceTests
    {
        [Fact]
        public async Task ShouldRemoveThingByIdAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();
            Guid inputThingId = randomId;
            Thing randomThing = CreateRandomThing();
            Thing storageThing = randomThing;
            Thing expectedInputThing = storageThing;
            Thing deletedThing = expectedInputThing;
            Thing expectedThing = deletedThing.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(inputThingId))
                    .ReturnsAsync(storageThing);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteThingAsync(expectedInputThing))
                    .ReturnsAsync(deletedThing);

            // when
            Thing actualThing = await this.ThingService
                .RemoveThingByIdAsync(inputThingId);

            // then
            actualThing.Should().BeEquivalentTo(expectedThing);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(inputThingId),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteThingAsync(expectedInputThing),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}