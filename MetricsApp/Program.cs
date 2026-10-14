using MetricsApp.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//1. Centralized Error Handler
app.UseExceptionHandler("/error");

//2. Timing Middleware Handler
app.UseMiddleware<RequestTimingMiddleware>();


//3. Routing & Endpoints
app.MapGet("/", () => "Hello from Metrics App!");

//4. Route that throws an Error
app.MapGet("/boom", () =>
{
    throw new InvalidOperationException("BOOM! Testing the error handler.");
});



//5. Metrics endpoint to return the last 20 metrics
app.MapGet("/metrics", () =>
{
    var metrics = RequestTimingMiddleware.Snapshot();
    return Results.Json(metrics, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
});


//6. Error Endpoint for Exception Handler
app.Map("/error", (HttpContext ctx) =>
{
    return Results.Problem(
        title: "An error occured while processing your request.",
        detail: "Something went wrong.",
        statusCode: StatusCodes.Status500InternalServerError);
});

app.Run();
