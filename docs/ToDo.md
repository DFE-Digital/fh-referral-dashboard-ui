# ToDo

* header too wide

* update connect site with latest shared and swap pagination for new one

* sign-in expiry doesn't work

* 404 page throws this error:

ArgumentException: An item with the same key has already been added. Key: BearerToken
System.Collections.Generic.Dictionary<TKey, TValue>.TryInsert(TKey key, TValue value, InsertionBehavior behavior)
System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey key, TValue value)
FamilyHubs.SharedKernel.Identity.Authentication.AccountMiddlewareBase.SetBearerToken(HttpContext httpContext)
FamilyHubs.SharedKernel.Identity.Authentication.Stub.StubAccountMiddleware.InvokeAsync(HttpContext context)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Builder.StatusCodePagesExtensions+<>c__DisplayClass7_0+<<CreateHandler>b__0>d.MoveNext()
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
NetEscapades.AspNetCore.SecurityHeaders.SecurityHeadersMiddleware.Invoke(HttpContext context) in SecurityHeadersMiddleware.cs
Serilog.AspNetCore.RequestLoggingMiddleware.Invoke(HttpContext httpContext)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

* if signed in as someone without permissions, can't sign out

* clicking column header should sort by that column

* pick up standard gulpfile from the npm package? also tsconfig?

* stories: health check, error handling, move shared razor library into sharedkernel,
spike: create npm with styles & scripts - add nuget package into npm??
, security headers, app insights, telemetry pii redactor, google analytics, cookie banner & cookie page functionality

* error pages into rcl with auto routing to them

* move rcl into sharedkernel?
