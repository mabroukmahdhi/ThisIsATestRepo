using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Something.Core.Models.Things;
using Something.Core.Models.Things.Exceptions;
using Xeptions;

namespace Something.Core.Services.Foundations.Things
{
    public partial class ThingService
    {
        private delegate ValueTask<Thing> ReturningThingFunction();
        private delegate IQueryable<Thing> ReturningThingsFunction();

        private async ValueTask<Thing> TryCatch(ReturningThingFunction returningThingFunction)
        {
            try
            {
                return await returningThingFunction();
            }
            catch (NullThingException nullThingException)
            {
                throw CreateAndLogValidationException(nullThingException);
            }
            catch (InvalidThingException invalidThingException)
            {
                throw CreateAndLogValidationException(invalidThingException);
            }
            catch (SqlException sqlException)
            {
                var failedThingStorageException =
                    new FailedThingStorageException(sqlException);

                throw CreateAndLogCriticalDependencyException(failedThingStorageException);
            }
            catch (NotFoundThingException notFoundThingException)
            {
                throw CreateAndLogValidationException(notFoundThingException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsThingException =
                    new AlreadyExistsThingException(duplicateKeyException);

                throw CreateAndLogDependencyValidationException(alreadyExistsThingException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalidThingReferenceException =
                    new InvalidThingReferenceException(foreignKeyConstraintConflictException);

                throw CreateAndLogDependencyValidationException(invalidThingReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedThingException = new LockedThingException(dbUpdateConcurrencyException);

                throw CreateAndLogDependencyValidationException(lockedThingException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedThingStorageException =
                    new FailedThingStorageException(databaseUpdateException);

                throw CreateAndLogDependencyException(failedThingStorageException);
            }
            catch (Exception exception)
            {
                var failedThingServiceException =
                    new FailedThingServiceException(exception);

                throw CreateAndLogServiceException(failedThingServiceException);
            }
        }

        private IQueryable<Thing> TryCatch(ReturningThingsFunction returningThingsFunction)
        {
            try
            {
                return returningThingsFunction();
            }
            catch (SqlException sqlException)
            {
                var failedThingStorageException =
                    new FailedThingStorageException(sqlException);
                throw CreateAndLogCriticalDependencyException(failedThingStorageException);
            }
            catch (Exception exception)
            {
                var failedThingServiceException =
                    new FailedThingServiceException(exception);

                throw CreateAndLogServiceException(failedThingServiceException);
            }
        }

        private ThingValidationException CreateAndLogValidationException(Xeption exception)
        {
            var ThingValidationException =
                new ThingValidationException(exception);

            this.loggingBroker.LogError(ThingValidationException);

            return ThingValidationException;
        }

        private ThingDependencyException CreateAndLogCriticalDependencyException(Xeption exception)
        {
            var ThingDependencyException = new ThingDependencyException(exception);
            this.loggingBroker.LogCritical(ThingDependencyException);

            return ThingDependencyException;
        }

        private ThingDependencyValidationException CreateAndLogDependencyValidationException(Xeption exception)
        {
            var ThingDependencyValidationException =
                new ThingDependencyValidationException(exception);

            this.loggingBroker.LogError(ThingDependencyValidationException);

            return ThingDependencyValidationException;
        }

        private ThingDependencyException CreateAndLogDependencyException(
            Xeption exception)
        {
            var ThingDependencyException = new ThingDependencyException(exception);
            this.loggingBroker.LogError(ThingDependencyException);

            return ThingDependencyException;
        }

        private ThingServiceException CreateAndLogServiceException(
            Xeption exception)
        {
            var ThingServiceException = new ThingServiceException(exception);
            this.loggingBroker.LogError(ThingServiceException);

            return ThingServiceException;
        }
    }
}