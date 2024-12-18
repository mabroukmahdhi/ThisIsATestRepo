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
        public async Task ShouldRetrieveThingByIdAsync()
        {
            // given
            Thing randomThing = CreateRandomThing();
            Thing inputThing = randomThing;
            Thing storageThing = randomThing;
            Thing expectedThing = storageThing.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(inputThing.Id))
                    .ReturnsAsync(storageThing);

            // when
            Thing actualThing =
                await this.ThingService.RetrieveThingByIdAsync(inputThing.Id);

            // then
            actualThing.Should().BeEquivalentTo(expectedThing);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(inputThing.Id),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}