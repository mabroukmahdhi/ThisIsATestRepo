using System.Linq;
using FluentAssertions;
using Moq;
using Something.Core.Models.Things;
using Xunit;

namespace Something.Core.Tests.Unit.Services.Foundations.Things
{
    public partial class ThingServiceTests
    {
        [Fact]
        public void ShouldReturnThings()
        {
            // given
            IQueryable<Thing> randomThings = CreateRandomThings();
            IQueryable<Thing> storageThings = randomThings;
            IQueryable<Thing> expectedThings = storageThings;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllThings())
                    .Returns(storageThings);

            // when
            IQueryable<Thing> actualThings =
                this.ThingService.RetrieveAllThings();

            // then
            actualThings.Should().BeEquivalentTo(expectedThings);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllThings(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}