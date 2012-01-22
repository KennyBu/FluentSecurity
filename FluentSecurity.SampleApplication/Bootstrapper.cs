using System.Security.Principal;
using System.Web;
using System.Web.Routing;
using FluentSecurity.Policy;
using FluentSecurity.SampleApplication.Controllers;
using FluentSecurity.SampleApplication.Models;

namespace FluentSecurity.SampleApplication
{
	public static class Bootstrapper
	{
		public static ISecurityConfiguration SetupFluentSecurity()
		{
			SecurityConfigurator.Configure(configuration =>
			{
				configuration.GetAuthenticationStatusFrom(Helpers.SecurityHelper.UserIsAuthenticated);
				configuration.GetRolesFrom(Helpers.SecurityHelper.UserRoles);

				configuration.For<HomeController>().Ignore();

				configuration.For<AccountController>(x => x.LogInAsAdministrator()).DenyAuthenticatedAccess();
				configuration.For<AccountController>(x => x.LogInAsPublisher()).DenyAuthenticatedAccess();
				configuration.For<AccountController>(x => x.LogOut()).DenyAnonymousAccess();

				configuration.For<ExampleController>(x => x.DenyAnonymousAccess()).DenyAnonymousAccess();
				configuration.For<ExampleController>(x => x.DenyAuthenticatedAccess()).DenyAuthenticatedAccess();

				configuration.For<ExampleController>(x => x.RequireAdministratorRole()).RequireRole(UserRole.Administrator);
				configuration.For<ExampleController>(x => x.RequirePublisherRole()).RequireRole(UserRole.Publisher);

				configuration.For<AdminController>().AddPolicy(new AdministratorPolicy());
				configuration.For<AdminController>(x => x.Delete()).DelegatePolicy("LocalOnlyPolicy",
					context => HttpContext.Current.Request.IsLocal
					);

				configuration.For<AdminController>(x => x.Test(0)).Ignore().AddPolicy(new RouteInfoPolicy());

				configuration.For<Areas.ExampleArea.Controllers.HomeController>().DenyAnonymousAccess();
				configuration.For<Areas.ExampleArea.Controllers.HomeController>(x => x.PublishersOnly()).RequireRole(UserRole.Publisher);
				configuration.For<Areas.ExampleArea.Controllers.HomeController>(x => x.AdministratorsOnly()).RequireRole(UserRole.Administrator);
			});
			return SecurityConfiguration.Current;
		}
	}

	public class RouteInfoPolicy : ISecurityPolicy
	{
		public PolicyResult Enforce(ISecurityContext context)
		{
			var routeValues = context.Data<RouteValueDictionary>();
			var id = routeValues["id"] != null ? int.Parse(routeValues["id"].ToString()) : 0;
			
			return id == 38
				? PolicyResult.CreateFailureResult(this, "Number was 38")
				: PolicyResult.CreateSuccessResult(this);
		}
	}

	public class CustomPolicy : SecurityPolicyBase<CustomSecurityContext>
	{
		public override PolicyResult Enforce(CustomSecurityContext context)
		{
			return context.Principal.Identity.Name == "Kristoffer" && context.CustomData == "ABC"
				? PolicyResult.CreateFailureResult(this, "Access denied")
				: PolicyResult.CreateSuccessResult(this);
		}
	}

	public class CustomSecurityContext : SecurityContextWrapper
	{
		public IPrincipal Principal { get; private set; }
		public string CustomData { get; private set; }

		public CustomSecurityContext(IPrincipal principal, ISecurityContext securityContext) : base(securityContext)
		{
			Principal = principal;
			CustomData = "ABC";
		}
	}
}