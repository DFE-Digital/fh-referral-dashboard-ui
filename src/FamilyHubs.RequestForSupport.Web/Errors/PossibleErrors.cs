using System.Collections.Immutable;
using FamilyHubs.SharedKernel.Razor.ErrorNext;

namespace FamilyHubs.RequestForSupport.Web.Errors;

public static class PossibleErrors
{
    public static readonly ImmutableDictionary<int, PossibleError> All = ImmutableDictionary
            .Create<int, PossibleError>()
            .Add(ErrorId.SelectAResponse, "You must select a response")
            .Add(ErrorId.EnterReasonForDeclining, "Enter a reason for declining")
            .Add(ErrorId.ReasonForDecliningTooLong, "Reason for declining must be 500 characters or less")
        ;
}
