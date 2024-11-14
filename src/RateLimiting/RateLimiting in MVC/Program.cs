using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region Rate Limiting

//1-Fixed Window Limit
builder.Services.AddRateLimiter(p =>
{
    p.AddFixedWindowLimiter("MyRateFixed", options =>
    {
        options.PermitLimit = 3;//تعداد در خواست 
        options.Window = TimeSpan.FromSeconds(10);//زمان اجرای درخواست ها
       // options.QueueLimit = 1;//اگر درخواست ها بیش از حد بود اینجا تعدادی را در صف قرار بده
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;//خواندن درخواست ها که به صورت پیش فرض از قدیمی ترین درخواست خوانده میشود
    });

});

#endregion


var app = builder.Build();

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

//قرار بگیرد Routingبعد از میانافزار مربوط به  RateLimiting باید میان افزار مربوط به 
app.UseRateLimiter();

app.Run();
