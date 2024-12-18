using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Moq;
using Something.Core.Models.Things;
using Something.Core.Models.Things.Exceptions;
using Xunit;

namespace Something.Core.Tests.Unit.Services.Foundations.Things
{
    public partial class ThingServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveByIdIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedThingStorageException =
                new FailedThingStorageException(sqlException);

            var expectedThingDependencyException =
                new ThingDependencyException(failedThingStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Thing> retrieveThingByIdTask =
                this.ThingService.RetrieveThingByIdAsync(someId);

            ThingDependencyException actualThingDependencyException =
                await Assert.ThrowsAsync<ThingDependencyException>(
                    retrieveThingByIdTask.AsTask);

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
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedThingServiceException =
                new FailedThingServiceException(serviceException);

            var expectedThingServiceException =
                new ThingServiceException(failedThingServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Thing> retrieveThingByIdTask =
                this.ThingService.RetrieveThingByIdAsync(someId);

            ThingServiceException actualThingServiceException =
                await Assert.ThrowsAsync<ThingServiceException>(
                    retrieveThingByIdTask.AsTask);

            // then
            actualThingServiceException.Should()
                .BeEquivalentTo(expectedThingServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectThingByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

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