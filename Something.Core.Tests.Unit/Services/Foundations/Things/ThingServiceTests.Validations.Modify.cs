using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using Something.Core.Models.Things;
using Something.Core.Models.Things.Exceptions;
using Xunit;

namespace Something.Core.Tests.Unit.Services.Foundations.Things
{
    public partial class ThingServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfThingIsNullAndLogItAsync()
        {
            // given
            Thing nullThing = null;
            var nullThingException = new NullThingException();

            var expectedThingValidationException =
                new ThingValidationException(nullThingException);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(nullThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    modifyThingTask.AsTask);

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfThingIsInvalidAndLogItAsync(string invalidText)
        {
            // given 
            var invalidThing = new Thing
            {
                // TODO:  Add default values for your properties i.e. Name = invalidText
            };

            var invalidThingException = new InvalidThingException();

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
                values:
                new[] {
                    "Date is required",
                    $"Date is the same as {nameof(Thing.CreatedDate)}"
                });

            invalidThingException.AddData(
                key: nameof(Thing.UpdatedByUserId),
                values: "Id is required");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(invalidThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    modifyThingTask.AsTask);

            //then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsSameAsCreatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomThing(randomDateTimeOffset);
            Thing invalidThing = randomThing;
            var invalidThingException = new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.UpdatedDate),
                values: $"Date is the same as {nameof(Thing.CreatedDate)}");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(invalidThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    modifyThingTask.AsTask);

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(invalidThing.Id),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(MinutesBeforeOrAfter))]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsNotRecentAndLogItAsync(int minutes)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomThing(randomDateTimeOffset);
            randomThing.UpdatedDate = randomDateTimeOffset.AddMinutes(minutes);

            var invalidThingException =
                new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.UpdatedDate),
                values: "Date is not recent");

            var expectedThingValidatonException =
                new ThingValidationException(invalidThingException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                .Returns(randomDateTimeOffset);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(randomThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    modifyThingTask.AsTask);

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidatonException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidatonException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfThingDoesNotExistAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomModifyThing(randomDateTimeOffset);
            Thing nonExistThing = randomThing;
            Thing nullThing = null;

            var notFoundThingException =
                new NotFoundThingException(nonExistThing.Id);

            var expectedThingValidationException =
                new ThingValidationException(notFoundThingException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(nonExistThing.Id))
                .ReturnsAsync(nullThing);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                .Returns(randomDateTimeOffset);

            // when 
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(nonExistThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    modifyThingTask.AsTask);

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(nonExistThing.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageCreatedDateNotSameAsCreatedDateAndLogItAsync()
        {
            // given
            int randomNumber = GetRandomNegativeNumber();
            int randomMinutes = randomNumber;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomModifyThing(randomDateTimeOffset);
            Thing invalidThing = randomThing.DeepClone();
            Thing storageThing = invalidThing.DeepClone();
            storageThing.CreatedDate = storageThing.CreatedDate.AddMinutes(randomMinutes);
            storageThing.UpdatedDate = storageThing.UpdatedDate.AddMinutes(randomMinutes);
            var invalidThingException = new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.CreatedDate),
                values: $"Date is not the same as {nameof(Thing.CreatedDate)}");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(invalidThing.Id))
                .ReturnsAsync(storageThing);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                .Returns(randomDateTimeOffset);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(invalidThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    modifyThingTask.AsTask);

            // then
            actualThingValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(invalidThing.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogError(It.Is(SameExceptionAs(
                   expectedThingValidationException))),
                       Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedUserIdDontMacthStorageAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomModifyThing(randomDateTimeOffset);
            Thing invalidThing = randomThing.DeepClone();
            Thing storageThing = invalidThing.DeepClone();
            invalidThing.CreatedByUserId = Guid.NewGuid();
            storageThing.UpdatedDate = storageThing.CreatedDate;

            var invalidThingException = new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.CreatedByUserId),
                values: $"Id is not the same as {nameof(Thing.CreatedByUserId)}");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(invalidThing.Id))
                .ReturnsAsync(storageThing);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                .Returns(randomDateTimeOffset);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(invalidThing);

            ThingValidationException actualThingValidationException =
                await Assert.ThrowsAsync<ThingValidationException>(
                    modifyThingTask.AsTask);

            // then
            actualThingValidationException.Should().BeEquivalentTo(expectedThingValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(invalidThing.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogError(It.Is(SameExceptionAs(
                   expectedThingValidationException))),
                       Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageUpdatedDateSameAsUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Thing randomThing = CreateRandomModifyThing(randomDateTimeOffset);
            Thing invalidThing = randomThing;
            Thing storageThing = randomThing.DeepClone();

            var invalidThingException = new InvalidThingException();

            invalidThingException.AddData(
                key: nameof(Thing.UpdatedDate),
                values: $"Date is the same as {nameof(Thing.UpdatedDate)}");

            var expectedThingValidationException =
                new ThingValidationException(invalidThingException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(invalidThing.Id))
                .ReturnsAsync(storageThing);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(invalidThing);

            // then
            await Assert.ThrowsAsync<ThingValidationException>(
                modifyThingTask.AsTask);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(invalidThing.Id),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}