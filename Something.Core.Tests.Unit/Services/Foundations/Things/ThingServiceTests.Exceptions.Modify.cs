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
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Thing randomThing = CreateRandomThing();
            SqlException sqlException = GetSqlException();

            var failedThingStorageException =
                new FailedThingStorageException(sqlException);

            var expectedThingDependencyException =
                new ThingDependencyException(failedThingStorageException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(sqlException);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(randomThing);

            ThingDependencyException actualThingDependencyException =
                await Assert.ThrowsAsync<ThingDependencyException>(
                    modifyThingTask.AsTask);

            // then
            actualThingDependencyException.Should()
                .BeEquivalentTo(expectedThingDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(randomThing.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedThingDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateThingAsync(randomThing),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async void ShouldThrowValidationExceptionOnModifyIfReferenceErrorOccursAndLogItAsync()
        {
            // given
            Thing someThing = CreateRandomThing();
            string randomMessage = GetRandomMessage();
            string exceptionMessage = randomMessage;

            var foreignKeyConstraintConflictException =
                new ForeignKeyConstraintConflictException(exceptionMessage);

            var invalidThingReferenceException =
                new InvalidThingReferenceException(foreignKeyConstraintConflictException);

            ThingDependencyValidationException expectedThingDependencyValidationException =
                new ThingDependencyValidationException(invalidThingReferenceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(foreignKeyConstraintConflictException);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(someThing);

            ThingDependencyValidationException actualThingDependencyValidationException =
                await Assert.ThrowsAsync<ThingDependencyValidationException>(
                    modifyThingTask.AsTask);

            // then
            actualThingDependencyValidationException.Should()
                .BeEquivalentTo(expectedThingDependencyValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(someThing.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(expectedThingDependencyValidationException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateThingAsync(someThing),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDatabaseUpdateExceptionOccursAndLogItAsync()
        {
            // given
            Thing randomThing = CreateRandomThing();
            var databaseUpdateException = new DbUpdateException();

            var failedThingStorageException =
                new FailedThingStorageException(databaseUpdateException);

            var expectedThingDependencyException =
                new ThingDependencyException(failedThingStorageException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(databaseUpdateException);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(randomThing);

            ThingDependencyException actualThingDependencyException =
                await Assert.ThrowsAsync<ThingDependencyException>(
                    modifyThingTask.AsTask);

            // then
            actualThingDependencyException.Should()
                .BeEquivalentTo(expectedThingDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(randomThing.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateThingAsync(randomThing),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDbUpdateConcurrencyErrorOccursAndLogAsync()
        {
            // given
            Thing randomThing = CreateRandomThing();
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedThingException =
                new LockedThingException(databaseUpdateConcurrencyException);

            var expectedThingDependencyValidationException =
                new ThingDependencyValidationException(lockedThingException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(databaseUpdateConcurrencyException);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(randomThing);

            ThingDependencyValidationException actualThingDependencyValidationException =
                await Assert.ThrowsAsync<ThingDependencyValidationException>(
                    modifyThingTask.AsTask);

            // then
            actualThingDependencyValidationException.Should()
                .BeEquivalentTo(expectedThingDependencyValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(randomThing.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateThingAsync(randomThing),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Thing randomThing = CreateRandomThing();
            var serviceException = new Exception();

            var failedThingServiceException =
                new FailedThingServiceException(serviceException);

            var expectedThingServiceException =
                new ThingServiceException(failedThingServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Throws(serviceException);

            // when
            ValueTask<Thing> modifyThingTask =
                this.ThingService.ModifyThingAsync(randomThing);

            ThingServiceException actualThingServiceException =
                await Assert.ThrowsAsync<ThingServiceException>(
                    modifyThingTask.AsTask);

            // then
            actualThingServiceException.Should()
                .BeEquivalentTo(expectedThingServiceException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(randomThing.Id),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingServiceException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateThingAsync(randomThing),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}