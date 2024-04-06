using System.ComponentModel;
using System.Reflection;
using CodeFirstExample.Application.Services;
using CodeFirstExample.Domain.DataContext;
using CodeFirstExample.Domain.Entities;
using CodeFirstExample.Infrastructure.DataContext;
using CodeFirstExample.SwaggerReDoc;
using Microsoft.OpenApi.Models;
using TripleSix.Core.Appsettings;
using TripleSix.Core.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

IConfiguration configuration = new ConfigurationBuilder()
               .AddJsonFile(Path.Combine("Appsettings", "appsettings.json"), true)
               .AddJsonFile(Path.Combine("Appsettings", $"appsettings.{envName}.json"), true)
               .AddEnvironmentVariables()
               .AddCommandLine(args)
               .Build();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerV2(new SwaggerAppsetting(configuration));

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Code First Example", Version = "v1" });
//});
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<IStudentService, StudentServices>();
builder.Services.AddScoped<IDepartmentService, DepartmentServices>();
builder.Services.AddScoped<IGradeServices, GradeServices>();

var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Code First Example"));

//app.UseReDocUI(configuration);
app.UseReDocUIV2(new SwaggerAppsetting(configuration));

//app.UseReDoc(c =>
//{
//    c.RoutePrefix = "swagger";
//    c.DocumentTitle = "Code First Example";
//    c.SpecUrl = "/swagger/v1/swagger.json";
//});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
