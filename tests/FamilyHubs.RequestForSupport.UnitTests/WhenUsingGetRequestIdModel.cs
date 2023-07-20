using FamilyHubs.RequestForSupport.Web.Pages.Vcs;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingGetRequestIdModel
{
    [Fact]
    public void ThenRequestIdIsSet()
    {
        var model = new GetRequestIdModel();

        // Act
        model.OnGet(1);

        Assert.Equal(1, model.RequestId);
    }
}