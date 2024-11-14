using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



#region RateLimiting

builder.Services.AddRateLimiter(p => 
{
    p.AddFixedWindowLimiter("MyRateFixed", options =>
    {
        options.PermitLimit = 3;//تعداد در خواست 
        options.Window =TimeSpan.FromSeconds(5);//زمان اجرای درخواست ها
        options.QueueLimit = 1;//اگر درخواست ها بیش از حد بود اینجا تعدادی را در صف قرار بده
        options.QueueProcessingOrder=QueueProcessingOrder.OldestFirst;//خواندن درخواست ها که به صورت پیش فرض از قدیمی ترین درخواست خوانده میشود
    });

    //مدیریت کنیم وپیغام های خودمان را نشان دهیم onRejected را قرار ندهیم میتوانیم درخواست های بیش از حد را با استفاده از  QueueLimit اگر ما کلا
    //p.OnRejected = async (context, token) =>
    //{
    //    context.HttpContext.Response.StatusCode = 429;
    //    context.HttpContext.Response.WriteAsync("درخواست پر است",token);
    //};

});

#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//use rate limiter
app.UseRateLimiter();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireRateLimiting("MyRateFixed")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
