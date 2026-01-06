using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Carmasters.Core.Application.Database;
using Carmasters.Core.Application.Documentation;
using Carmasters.Core.Application.Errors;
using Carmasters.Core.Application.Extensions.Builder;
using Carmasters.Core.Application.Extensions.DependencyInjection;
using Carmasters.Core.Application.Printing;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Core.Repository.Postgres;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate.Engine;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.
    WebHost.
    UseContentRoot(Directory.GetCurrentDirectory()).
    UseWebRoot("wwwroot").
    UseStaticWebAssets();
 

builder.Configuration.AddJsonFile("appsettings.Secrets.json", false);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// Add services to the container.

builder.Services.
 AddAutoMapperToApp()
.AddPersistanceServices(builder.Configuration)
.AddScoped<ITemplateService, RazorViewsTemplateService>()
.AddScoped<IPdfGenerator, PdfGenerator>()
.AddScoped<PricingFooterHtmlGenerator>()
.AddScoped<PricingBodyHtmlGenerator>()
.AddScoped<IPricingSender, PricingPdfMailSender>()
.AddSingleton<ISmtpClientFactory, SmtpClientFactory>()
.AddDemoSetupServices()
.AddCorsToApp(builder.Configuration)
.AddControllersWithViewsToApp()
.AddHealthChecks().Services
.AddSwaggerToApp()
.AddJwtAuthenticationToApp(builder.Configuration)
.AddHttpContextAccessor()
.AddDistributedMemoryCache()
.AddApplicationOptions(builder.Configuration)
.AddExceptionHandler<JsonExceptionHandler>()
.AddTenantConfigurationServices();

builder.Services.AddSingleton<RateLimitStrategyFactory>();

var app = builder.Build();
app.MapStaticAssets();
app.UseAuthentication();
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        await Task.CompletedTask; //JsonExceptionHandler  wont run without this
    });
});


app.UseNHibernate();
app.UseCors("DefaultPolicy");
app.UseMiddleware<DbConnectionScopeMiddleware>();

/*By default, an ASP.NET Core app doesn't provide a status code page for HTTP error status codes, such as 404 - Not Found. When the app sets an HTTP 400-599 error status code that doesn't have a body, it returns the status code and an empty response body. To enable default text-only handlers for common error status codes,*/
app.UseStatusCodePages(); 
app.UseRouting();
app.UseStaticFiles();
app.UseRateLimiting();
 
//await app.PreparePuppeteerAsync(app.Environment.ContentRootPath); //TODO cant download browser online every startup
 
app.MapControllers();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseSwagger();
app.UseSwaggerUI(c =>
{
	var js = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documentation", "SwaggerJwtInetercept.js")).ReplaceLineEndings(" ");
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "CarCare API V1");
	c.RoutePrefix = string.Empty;
	c.EnablePersistAuthorization();
	c.UseRequestInterceptor(js); 
});

app.UseAuthorization();
app.Run();

 