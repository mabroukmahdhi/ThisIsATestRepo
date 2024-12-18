using System;
using System.Threading.Tasks;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Thing randomThing = CreateRandomThing();
            SqlException sqlException = GetSqlException();

            var failedThingStorageException =
                new FailedThingStorageException(sqlException);

            var expectedThingDependencyException =
                new ThingDependencyException(failedThingStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(randomThing.Id))
                    .Throws(sqlException);

            // when
            ValueTask<Thing> addThingTask =
                this.ThingService.RemoveThingByIdAsync(randomThing.Id);

            ThingDependencyException actualThingDependencyException =
                await Assert.ThrowsAsync<ThingDependencyException>(
                    addThingTask.AsTask);

            // then
            actualThingDependencyException.Should()
                .BeEquivalentTo(expectedThingDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(randomThing.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedThingDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationOnRemoveIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid someThingId = Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedThingException =
                new LockedThingException(databaseUpdateConcurrencyException);

            var expectedThingDependencyValidationException =
                new ThingDependencyValidationException(lockedThingException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<Thing> removeThingByIdTask =
                this.ThingService.RemoveThingByIdAsync(someThingId);

            ThingDependencyValidationException actualThingDependencyValidationException =
                await Assert.ThrowsAsync<ThingDependencyValidationException>(
                    removeThingByIdTask.AsTask);

            // then
            actualThingDependencyValidationException.Should()
                .BeEquivalentTo(expectedThingDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteThingAsync(It.IsAny<Thing>()),
                    Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRemoveWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
            Guid someThingId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedThingStorageException =
                new FailedThingStorageException(sqlException);

            var expectedThingDependencyException =
                new ThingDependencyException(failedThingStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Thing> deleteThingTask =
                this.ThingService.RemoveThingByIdAsync(someThingId);

            ThingDependencyException actualThingDependencyException =
                await Assert.ThrowsAsync<ThingDependencyException>(
                    deleteThingTask.AsTask);

            // then
            actualThingDependencyException.Should()
                .BeEquivalentTo(expectedThingDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedThingDependencyException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someThingId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedThingServiceException =
                new FailedThingServiceException(serviceException);

            var expectedThingServiceException =
                new ThingServiceException(failedThingServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Thing> removeThingByIdTask =
                this.ThingService.RemoveThingByIdAsync(someThingId);

            ThingServiceException actualThingServiceException =
                await Assert.ThrowsAsync<ThingServiceException>(
                    removeThingByIdTask.AsTask);

            // then
            actualThingServiceException.Should()
                .BeEquivalentTo(expectedThingServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingServiceException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}