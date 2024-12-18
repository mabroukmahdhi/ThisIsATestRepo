using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Something.Core.Models.Things;
using Something.Core.Models.Things.Exceptions;
using Xunit;

namespace Something.Core.Tests.Unit.Services.Foundations.Things
{
    public partial class ThingServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Thing someThing = CreateRandomThing();
            SqlException sqlException = GetSqlException();

            var failedThingStorageException =
                new FailedThingStorageException(sqlException);

            var expectedThingDependencyException =
                new ThingDependencyException(failedThingStorageException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(sqlException);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(someThing);

            ThingDependencyException actualThingDependencyException =
                await Assert.ThrowsAsync<ThingDependencyException>(
                    addThingTask.AsTask);

            // then
            actualThingDependencyException.Should()
                .BeEquivalentTo(expectedThingDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedThingDependencyException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfThingAlreadyExsitsAndLogItAsync()
        {
            // given
            Thing randomThing = CreateRandomThing();
            Thing alreadyExistsThing = randomThing;
            string randomMessage = GetRandomMessage();

            var duplicateKeyException =
                new DuplicateKeyException(randomMessage);

            var alreadyExistsThingException =
                new AlreadyExistsThingException(duplicateKeyException);

            var expectedThingDependencyValidationException =
                new ThingDependencyValidationException(alreadyExistsThingException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(duplicateKeyException);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(alreadyExistsThing);

            // then
            ThingDependencyValidationException actualThingDependencyValidationException =
                await Assert.ThrowsAsync<ThingDependencyValidationException>(
                    addThingTask.AsTask);

            actualThingDependencyValidationException.Should()
                .BeEquivalentTo(expectedThingDependencyValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingDependencyValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async void ShouldThrowValidationExceptionOnAddIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            Thing someThing = CreateRandomThing();
            string randomMessage = GetRandomMessage();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidThingReferenceException =
                new InvalidThingReferenceException(foreignKeyConstraintConflictException);

            var expectedThingValidationException =
                new ThingDependencyValidationException(invalidThingReferenceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(foreignKeyConstraintConflictException);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(someThing);

            // then
            ThingDependencyValidationException actualThingDependencyValidationException =
                await Assert.ThrowsAsync<ThingDependencyValidationException>(
                    addThingTask.AsTask);

            actualThingDependencyValidationException.Should()
                .BeEquivalentTo(expectedThingValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(someThing),
                    Times.Never());

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            Thing someThing = CreateRandomThing();

            var databaseUpdateException =
                new DbUpdateException();

            var failedThingStorageException =
                new FailedThingStorageException(databaseUpdateException);

            var expectedThingDependencyException =
                new ThingDependencyException(failedThingStorageException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(databaseUpdateException);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(someThing);

            ThingDependencyException actualThingDependencyException =
                await Assert.ThrowsAsync<ThingDependencyException>(
                    addThingTask.AsTask);

            // then
            actualThingDependencyException.Should()
                .BeEquivalentTo(expectedThingDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingDependencyException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Thing someThing = CreateRandomThing();
            var serviceException = new Exception();

            var failedThingServiceException =
                new FailedThingServiceException(serviceException);

            var expectedThingServiceException =
                new ThingServiceException(failedThingServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(serviceException);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.AddThingAsync(someThing);

            ThingServiceException actualThingServiceException =
                await Assert.ThrowsAsync<ThingServiceException>(
                    addThingTask.AsTask);

            // then
            actualThingServiceException.Should()
                .BeEquivalentTo(expectedThingServiceException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingServiceException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}