using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = c =>
    {
        if (c.Exception is null)
            return;

        c.ProblemDetails = c.Exception switch
        {
            ValidationException ex => new ValidationProblemDetails(
                ex.Errors.GroupBy(x => x.PropertyName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(e => e.ErrorMessage).ToArray(),
                        StringComparer.OrdinalIgnoreCase
                    )
            )
            {
                Status = StatusCodes.Status400BadRequest,
            },
            UserNotFoundException ex => new ProblemDetails
            {
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Detail = "Access denied",
                Status = StatusCodes.Status403Forbidden,
            },
            _ => new ProblemDetails
            {
                Detail = "An unknown error has occurred",
                Status = StatusCodes.Status500InternalServerError,
            },
        };

        c.HttpContext.Response.StatusCode =
            c.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
