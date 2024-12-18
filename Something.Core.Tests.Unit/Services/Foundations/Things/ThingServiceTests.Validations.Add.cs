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
        public async Task ShouldThrowValidationExceptionOnAddIfThingIsNullAndLogItAsync()
        {
            // given
            Thing nullThing = null;

            var nullThingException =
                new NullThingException();

            var expectedThingValidationException =
                new ThingValidationException(nullThingException);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(nullThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(() =>
                    addThingTask.AsTask());

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfThingIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            var invalidThing = new Thing
            {
                // TODO:  Add default values for your properties i.e. Name = invalidText
            };

            var invalidThingException =
                new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.Id),
                values: "Id is required");

            //invalidThingException.AddData(
            //    key: nameof(Thing.Name),
            //    values: "Text is required");

            // TODO: Add or remove data here to suit the validation needs for the Thing model

            invalidThingException.AddData(
                key: nameof(Thing.CreatedDate),
                values: "Date is required");

            invalidThingException.AddData(
                key: nameof(Thing.CreatedByUserId),
                values: "Id is required");

            invalidThingException.AddData(
                key: nameof(Thing.UpdatedDate),
                values: "Date is required");

            invalidThingException.AddData(
                key: nameof(Thing.UpdatedByUserId),
                values: "Id is required");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(invalidThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(() =>
                    addThingTask.AsTask());

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCreateAndUpdateDatesIsNotSameAndLogItAsync()
        {
            // given
            int randomNumber = GetRandomNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomThing(randomDateTimeOffset);
            Thing invalidThing = randomThing;

            invalidThing.UpdatedDate =
                invalidThing.CreatedDate.AddDays(randomNumber);

            var invalidThingException = new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.UpdatedDate),
                values: $"Date is not the same as {nameof(Thing.CreatedDate)}");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(invalidThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(() =>
                    addThingTask.AsTask());

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCreateAndUpdateUserIdsIsNotSameAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomThing(randomDateTimeOffset);
            Thing invalidThing = randomThing;
            invalidThing.UpdatedByUserId = Guid.NewGuid();

            var invalidThingException =
                new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.UpdatedByUserId),
                values: $"Id is not the same as {nameof(Thing.CreatedByUserId)}");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(invalidThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(() =>
                    addThingTask.AsTask());

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(MinutesBeforeOrAfter))]
        public async Task ShouldThrowValidationExceptionOnAddIfCreatedDateIsNotRecentAndLogItAsync(
            int minutesBeforeOrAfter)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            DateTimeOffset invalidDateTime =
                randomDateTimeOffset.AddMinutes(minutesBeforeOrAfter);

            Thing randomThing = CreateRandomThing(invalidDateTime);
            Thing invalidThing = randomThing;

            var invalidThingException =
                new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.CreatedDate),
                values: "Date is not recent");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(invalidThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(() =>
                    addThingTask.AsTask());

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}