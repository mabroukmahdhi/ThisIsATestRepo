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
        public async Task ShouldThrowValidationExceptionOnRemoveIfIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidThingId = Guid.Empty;

            var invalidThingException =
                new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.Id),
                values: "Id is required");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            // when
            ValueTask<Thing> removeThingByIdTask =
                this.ThingService.RemoveThingByIdAsync(invalidThingId);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    removeThingByIdTask.AsTask);

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}