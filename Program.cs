using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RentalsApi.Data;
using RentalsApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== الخدمات =====
// إذا وُجد رابط Postgres (على السحابة عبر متغيّر البيئة DATABASE_URL) نستخدمه،
// وإلا نستخدم SQLite محليًا. هكذا نفس الكود يعمل محليًا وعلى Render/Neon.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var pgConnection = RentalsApi.DbConfig.BuildNpgsqlConnectionString(databaseUrl);

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (!string.IsNullOrWhiteSpace(pgConnection))
        opt.UseNpgsql(pgConnection);                       // السحابة (Neon)
    else
        opt.UseSqlite(builder.Configuration.GetConnectionString("Db")
                      ?? "Data Source=rentals.db");        // محليًا
});

builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// CORS: السماح للتطبيق المحمول وللمتصفح بالاستدعاء
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rentals API",
        Version = "v1",
        Description = "واجهة برمجية لإدارة إعلانات الإيجارات. " +
                      "استخدم Swagger UI أدناه لإضافة/تعديل/حذف الإعلانات."
    });
});

var app = builder.Build();

// ===== تطبيق الترحيلات تلقائيًا عند الإقلاع =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ===== Swagger متاح دائمًا (لأن هذه هي الواجهة الإدارية) =====
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rentals API v1");
    c.RoutePrefix = "swagger"; // متاح على /swagger
});

app.UseCors();

// إعادة توجيه الجذر إلى صفحة الإدارة (أسهل من Swagger للعربي)
app.MapGet("/", () => Results.Redirect("/manage"));

// ===== صفحة إدارة عربية: إضافة/حذف/عرض بدون مشكلة انقلاب النص =====
app.MapGet("/manage", () => Results.Content(RentalsApi.ManagePage.Html, "text/html; charset=utf-8"));

// ===== Endpoints =====

// قراءة كل الإعلانات (التطبيق المحمول يقرأ من هنا)
app.MapGet("/listings", async (AppDbContext db) =>
    await db.Listings.OrderByDescending(l => l.Featured)
                     .ThenBy(l => l.Price).ToListAsync())
   .WithName("GetListings")
;

// قراءة إعلان واحد
app.MapGet("/listings/{id:int}", async (int id, AppDbContext db) =>
    await db.Listings.FindAsync(id) is { } l ? Results.Ok(l) : Results.NotFound())
   .WithName("GetListing")
;

// إنشاء إعلان جديد
app.MapPost("/listings", async (Listing input, AppDbContext db) =>
{
    input.Id = 0;
    db.Listings.Add(input);
    await db.SaveChangesAsync();
    return Results.Created($"/listings/{input.Id}", input);
})
.WithName("CreateListing")
;

// تحديث إعلان
app.MapPut("/listings/{id:int}", async (int id, Listing input, AppDbContext db) =>
{
    var l = await db.Listings.FindAsync(id);
    if (l is null) return Results.NotFound();

    l.Title = input.Title;
    l.Type = input.Type;
    l.Price = input.Price;
    l.Location = input.Location;
    l.Description = input.Description;
    l.Phone = input.Phone;
    l.Bedrooms = input.Bedrooms;
    l.Bathrooms = input.Bathrooms;
    l.Area = input.Area;
    l.Featured = input.Featured;
    l.Emoji = input.Emoji;
    l.Latitude = input.Latitude;
    l.Longitude = input.Longitude;

    await db.SaveChangesAsync();
    return Results.Ok(l);
})
.WithName("UpdateListing")
;

// حذف إعلان
app.MapDelete("/listings/{id:int}", async (int id, AppDbContext db) =>
{
    var l = await db.Listings.FindAsync(id);
    if (l is null) return Results.NotFound();
    db.Listings.Remove(l);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteListing")
;

app.Run();
