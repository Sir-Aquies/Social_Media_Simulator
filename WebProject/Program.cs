﻿using WebProject.Data;
using WebProject.Models;
using WebProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProject.Policies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

	//RandomUsers randomUsers = new(ClientFactory, UserManager, ServiceFactory);
	//randomUsers.StartAsync(new CancellationToken());

	//RandomPosts randomPost = new(ClientFactory, ServiceFactory);
	//randomPost.StartAsync(new CancellationToken());

	//RandomLikes randomLikes = new(ServiceFactory);
	//randomLikes.StartAsync(new CancellationToken());

	//RandomComments randomComments = new(ServiceFactory, ClientFactory);
	//randomComments.StartAsync(new CancellationToken());
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "Likes", 
	pattern: "{userName}/Likes",
	defaults: new { controller = "User", action = "LikedPosts" });

app.MapControllerRoute(
	name: "Comments",
	pattern: "{userName}/Comments",
	defaults: new { controller = "User", action = "CommentedPosts" });

app.MapControllerRoute(
	name: "Posts",
	pattern: "{username}/hop/{PostId}",
	defaults: new { controller = "User", action = "CompletePost" });

app.MapControllerRoute(name: "Users", pattern: "{userName}", defaults: new { controller = "User", action = "UserPage"});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=User}/{action=SearchUser}/{id?}");

app.Run();
