using DailyRoutine.Data;
using DailyRoutine.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
}

// DI 컨테이너가 ApplicationDbContext를 생성할 때 사용할 MySQL 연결을 설정한다.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));

// HTTP 요청마다 RoutineService 인스턴스를 하나 생성하고 같은 요청 안에서 공유한다.
// 생성자에 필요한 ApplicationDbContext도 DI 컨테이너가 주입한다.
builder.Services.AddScoped<RoutineService>();

// Controller와 View를 사용하는 MVC 기능을 DI 컨테이너에 등록한다.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
