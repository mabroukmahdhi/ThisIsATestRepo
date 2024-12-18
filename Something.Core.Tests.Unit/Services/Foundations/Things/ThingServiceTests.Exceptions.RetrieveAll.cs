using System;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Moq;
using Something.Core.Models.Things.Exceptions;
using Xunit;

namespace Something.Core.Tests.Unit.Services.Foundations.Things
{
    public partial class ThingServiceTests
    {
        [Fact]
        public void ShouldThrowCriticalDependencyExceptionOnRetrieveAllWhenSqlExceptionOccursAndLogIt()
        {
            // given
            SqlException sqlException = GetSqlException();

            var failedStorageException =
                new FailedThingStorageException(sqlException);

            var expectedThingDependencyException =
                new ThingDependencyException(failedStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllThings())
                    .Throws(sqlException);

            // when
            Action retrieveAllThingsAction = () =>
                this.ThingService.RetrieveAllThings();

            ThingDependencyException actualThingDependencyException =
                Assert.Throws<ThingDependencyException>(retrieveAllThingsAction);

            // then
            actualThingDependencyException.Should()
                .BeEquivalentTo(expectedThingDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllThings(),
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
        public void ShouldThrowServiceExceptionOnRetrieveAllIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string exceptionMessage = GetRandomMessage();
            var serviceException = new Exception(exceptionMessage);

            var failedThingServiceException =
                new FailedThingServiceException(serviceException);

            var expectedThingServiceException =
                new ThingServiceException(failedThingServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllThings())
                    .Throws(serviceException);

            // when
            Action retrieveAllThingsAction = () =>
                this.ThingService.RetrieveAllThings();

            ThingServiceException actualThingServiceException =
                Assert.Throws<ThingServiceException>(retrieveAllThingsAction);

            // then
            actualThingServiceException.Should()
                .BeEquivalentTo(expectedThingServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllThings(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedThingServiceException))),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}