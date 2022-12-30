using WebProject.Data;
using WebProject.Models;
using WebProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProject.Policies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<WebProjectContext>(options =>

	options.UseSqlServer(builder.Configuration.GetConnectionString("WebProjectContext") ?? 
	throw new InvalidOperationException("Connection string 'WebProjectContext' not found.")));

builder.Services.AddIdentity<UserModel, IdentityRole>().AddEntityFrameworkStores<WebProjectContext>().AddDefaultTokenProviders();

//builder.Services.ConfigureApplicationCookie(options =>
//{

//});

builder.Services.AddTransient<IUserValidator<UserModel>, UserNamePolicies>();

builder.Services.Configure<IdentityOptions>(opts =>
{
	opts.User.RequireUniqueEmail = true;
});

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;

	var context = services.GetRequiredService<WebProjectContext>();
	context.Database.EnsureCreated();

	IServiceScopeFactory ServiceFactory = services.GetRequiredService<IServiceScopeFactory>();
	IHttpClientFactory ClientFactory = services.GetRequiredService<IHttpClientFactory>();
	UserManager<UserModel> UserManager = services.GetRequiredService<UserManager<UserModel>>();

	SocialMediaAlgorithm socialMedia = new(ClientFactory, UserManager, ServiceFactory);
	socialMedia.StartAsync(new CancellationToken());

	RandomPost randomPost = new(ClientFactory, ServiceFactory);
	randomPost.StartAsync(new CancellationToken());
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "Users", pattern: "{UserName}", defaults: new { controller = "User", action = "UserPage"});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=User}/{action=SearchUser}/{id?}");

//app.MapRazorPages();

app.Run();
