using System.Collections.Immutable;
using FamilyHubs.SharedKernel.Razor.Errors;
using ErrorDictionary = System.Collections.Immutable.ImmutableDictionary<int, FamilyHubs.SharedKernel.Razor.Errors.Error>;

namespace FamilyHubs.RequestForSupport.Web.Errors;

public static class PossibleErrors
{
    public static readonly ErrorDictionary All = ImmutableDictionary
            .Create<int, Error>()
            .Add(ErrorId.SelectAResponse, "accept-request", "You must select a response")
            .Add(ErrorId.EnterReasonForDeclining, "decline-reason", "Enter a reason for declining")
            .Add(ErrorId.ReasonForDecliningTooLong, "decline-reason", "Reason for declining must be 500 characters or less")
        ;
}