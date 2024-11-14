using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Global RateLimiting

//builder.Services.AddRateLimiter(p => 
//{
//    p.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>

//        RateLimitPartition.GetFixedWindowLimiter(
//            httpContext.User.Identity?.Name ??
//            httpContext.Request.Headers.Host.ToString(),
//            partition => new FixedWindowRateLimiterOptions
//            {
//                PermitLimit=3,
//                Window=TimeSpan.FromSeconds(5),
//                QueueLimit=1,
//                QueueProcessingOrder=QueueProcessingOrder.OldestFirst,

//            }));
//});

#endregion

#region RateLimiting Algorithms

//1-Fixed Window Limit
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

//2-Sliding Window Limit

builder.Services.AddRateLimiter(p => 
{
    p.AddSlidingWindowLimiter("Sliding", options => 
    {
        options.PermitLimit = 5;//تعداد درخواست
        options.Window =TimeSpan.FromSeconds(5);//زمان اجرای درخواست

        options.SegmentsPerWindow = 3;//زمان اجرا را به سه قسمت تقسیم میکنیم

        options.QueueLimit = 1;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

    });
});

//3-Token Bucket Limit
builder.Services.AddRateLimiter(p => 
{
    p.AddTokenBucketLimiter("tokenBucket", options =>
    {
        options.TokenLimit = 4;
        options.TokensPerPeriod = 2;//در هر دوره برای ما 2تاتوکن اضافه کند
        options.ReplenishmentPeriod=TimeSpan.FromSeconds(5);//در هر 5 ثانیه 2 تا توکن اضافه کن
        options.QueueLimit = 6;
    });
});

//4-Concurrncy Limitدر هر لحظه در این مثال 5تا درخواست داریم

builder.Services.AddRateLimiter(p =>
{
    p.AddConcurrencyLimiter("ConcurrncyLimit", options =>
    {
        options.PermitLimit = 5;
        options.QueueLimit = 1;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
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
//.RequireRateLimiting("MyRateFixed")//Fixed Window Limit
//.RequireRateLimiting("Sliding")//Sliding Window Limiter
//.RequireRateLimiting("tokenBucket")//Token Bucket Limiter
//.RequireRateLimiting("ConcurrncyLimit")//ConcurrncyLimiter
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
