using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Something.Core.Models.Things;
using Something.Core.Models.Things.Exceptions;
using Xunit;

namespace Something.Core.Tests.Unit.Services.Foundations.Things
{
    public partial class ThingServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidThingId = Guid.Empty;

            var invalidThingException =
                new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.Id),
                values: "Id is required");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            // when
            ValueTask<Thing> retrieveThingByIdTask =
                this.ThingService.RetrieveThingByIdAsync(invalidThingId);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    retrieveThingByIdTask.AsTask);

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByIdIfThingIsNotFoundAndLogItAsync()
        {
            //given
            Guid someThingId = Guid.NewGuid();
            Thing noThing = null;

            var notFoundThingException =
                new NotFoundThingException(someThingId);

            var expectedThingValidationException =
                new ThingValidationException(notFoundThingException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noThing);

            //when
            ValueTask<Thing> retrieveThingByIdTask =
                this.ThingService.RetrieveThingByIdAsync(someThingId);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    retrieveThingByIdTask.AsTask);

            //then
            actualThingValidationException.Should().BeEquivalentTo(expectedThingValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}