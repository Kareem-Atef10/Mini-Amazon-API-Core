
using Mini_Amazon_API_Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Mini_Amazon_API_Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<MDbcontext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("AMZ")));

            builder.Services.AddScoped<IPasswordHasher<Users>,
                PasswordHasher<Users>>();

            var app = builder.Build();

            // Global exception handling
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();

                    var logger = context.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    if (exceptionFeature?.Error != null)
                    {
                        logger.LogError(
                            exceptionFeature.Error,
                            "An unhandled exception occurred.");
                    }

                    context.Response.StatusCode =
                        StatusCodes.Status500InternalServerError;

                    var problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "An unexpected error occurred.",
                        Detail = "An error occurred while processing your request."
                    };

                    await context.Response.WriteAsJsonAsync(problem);
                });
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}