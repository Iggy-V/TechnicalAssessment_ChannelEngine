using TechnicalAssessment_ChannelEngine.Models;
using TechnicalAssessment_ChannelEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Configuration.AddUserSecrets<Program>();

// Register the ChannelEngineKey configuration section (API key dependency injection)
builder.Services.Configure<ChannelEngineKey>(
    builder.Configuration.GetSection("ChannelEngine"));

builder.Services.AddHttpClient<IOrderClient, OrderService>();

builder.Services.AddScoped<ChannelEngineInterface, ChannelEngineService>();

var app = builder.Build();
// Register the ChannelEngineService as a service
//builder.Services.AddHttpClient<ChannelEngineInterface, ChannelEngineService>();
//var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
