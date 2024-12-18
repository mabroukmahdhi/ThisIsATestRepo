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
        public async Task ShouldModifyThingAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomModifyThing(randomDateTimeOffset);
            Thing inputThing = randomThing;
            Thing storageThing = inputThing.DeepClone();
            storageThing.UpdatedDate = randomThing.CreatedDate;
            Thing updatedThing = inputThing;
            Thing expectedThing = updatedThing.DeepClone();
            Guid ThingId = inputThing.Id;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(ThingId))
                    .ReturnsAsync(storageThing);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateThingAsync(inputThing))
                    .ReturnsAsync(updatedThing);

            // when
            Thing actualThing =
                await this.ThingService.ModifyThingAsync(inputThing);

            // then
            actualThing.Should().BeEquivalentTo(expectedThing);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(inputThing.Id),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateThingAsync(inputThing),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}