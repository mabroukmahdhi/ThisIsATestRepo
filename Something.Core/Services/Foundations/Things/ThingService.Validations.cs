using System;
using Something.Core.Models.Things;
using Something.Core.Models.Things.Exceptions;

namespace Something.Core.Services.Foundations.Things
{
    public partial class ThingService
    {
        private void ValidateThingOnAdd(Thing Thing)
        {
            ValidateThingIsNotNull(Thing);

            Validate(
                (Rule: IsInvalid(Thing.Id), Parameter: nameof(Thing.Id)),

                // TODO: Add any other required validation rules

                (Rule: IsInvalid(Thing.CreatedDate), Parameter: nameof(Thing.CreatedDate)),
                (Rule: IsInvalid(Thing.CreatedByUserId), Parameter: nameof(Thing.CreatedByUserId)),
                (Rule: IsInvalid(Thing.UpdatedDate), Parameter: nameof(Thing.UpdatedDate)),
                (Rule: IsInvalid(Thing.UpdatedByUserId), Parameter: nameof(Thing.UpdatedByUserId)),

                (Rule: IsNotSame(
                    firstDate: Thing.UpdatedDate,
                    secondDate: Thing.CreatedDate,
                    secondDateName: nameof(Thing.CreatedDate)),
                Parameter: nameof(Thing.UpdatedDate)),

                (Rule: IsNotSame(
                    firstId: Thing.UpdatedByUserId,
                    secondId: Thing.CreatedByUserId,
                    secondIdName: nameof(Thing.CreatedByUserId)),
                Parameter: nameof(Thing.UpdatedByUserId)),

                (Rule: IsNotRecent(Thing.CreatedDate), Parameter: nameof(Thing.CreatedDate)));
        }

        private void ValidateThingOnModify(Thing Thing)
        {
            ValidateThingIsNotNull(Thing);

            Validate(
                (Rule: IsInvalid(Thing.Id), Parameter: nameof(Thing.Id)),

                // TODO: Add any other required validation rules

                (Rule: IsInvalid(Thing.CreatedDate), Parameter: nameof(Thing.CreatedDate)),
                (Rule: IsInvalid(Thing.CreatedByUserId), Parameter: nameof(Thing.CreatedByUserId)),
                (Rule: IsInvalid(Thing.UpdatedDate), Parameter: nameof(Thing.UpdatedDate)),
                (Rule: IsInvalid(Thing.UpdatedByUserId), Parameter: nameof(Thing.UpdatedByUserId)),

                (Rule: IsSame(
                    firstDate: Thing.UpdatedDate,
                    secondDate: Thing.CreatedDate,
                    secondDateName: nameof(Thing.CreatedDate)),
                Parameter: nameof(Thing.UpdatedDate)),

                (Rule: IsNotRecent(Thing.UpdatedDate), Parameter: nameof(Thing.UpdatedDate)));
        }

        public void ValidateThingId(Guid ThingId) =>
            Validate((Rule: IsInvalid(ThingId), Parameter: nameof(Thing.Id)));

        private static void ValidateStorageThing(Thing maybeThing, Guid ThingId)
        {
            if (maybeThing is null)
            {
                throw new NotFoundThingException(ThingId);
            }
        }

        private static void ValidateThingIsNotNull(Thing Thing)
        {
            if (Thing is null)
            {
                throw new NullThingException();
            }
        }

        private static void ValidateAgainstStorageThingOnModify(Thing inputThing, Thing storageThing)
        {
            Validate(
                (Rule: IsNotSame(
                    firstDate: inputThing.CreatedDate,
                    secondDate: storageThing.CreatedDate,
                    secondDateName: nameof(Thing.CreatedDate)),
                Parameter: nameof(Thing.CreatedDate)),

                (Rule: IsNotSame(
                    firstId: inputThing.CreatedByUserId,
                    secondId: storageThing.CreatedByUserId,
                    secondIdName: nameof(Thing.CreatedByUserId)),
                Parameter: nameof(Thing.CreatedByUserId)),

                (Rule: IsSame(
                    firstDate: inputThing.UpdatedDate,
                    secondDate: storageThing.UpdatedDate,
                    secondDateName: nameof(Thing.UpdatedDate)),
                Parameter: nameof(Thing.UpdatedDate)));
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is required"
        };

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is required"
        };

        private static dynamic IsSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate == secondDate,
                Message = $"Date is the same as {secondDateName}"
            };

        private static dynamic IsNotSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate != secondDate,
                Message = $"Date is not the same as {secondDateName}"
            };

        private static dynamic IsNotSame(
            Guid firstId,
            Guid secondId,
            string secondIdName) => new
            {
                Condition = firstId != secondId,
                Message = $"Id is not the same as {secondIdName}"
            };

        private dynamic IsNotRecent(DateTimeOffset date) => new
        {
            Condition = IsDateNotRecent(date),
            Message = "Date is not recent"
        };

        private bool IsDateNotRecent(DateTimeOffset date)
        {
            DateTimeOffset currentDateTime =
                this.dateTimeBroker.GetCurrentDateTimeOffset();

            TimeSpan timeDifference = currentDateTime.Subtract(date);
            TimeSpan oneMinute = TimeSpan.FromMinutes(1);

            return timeDifference.Duration() > oneMinute;
        }

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidThingException = new InvalidThingException();

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidThingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidThingException.ThrowIfContainsErrors();
        }
    }
}