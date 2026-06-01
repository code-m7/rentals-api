namespace RentalsApi;

/// <summary>
/// يحوّل رابط قاعدة بيانات Postgres (مثل الذي يعطيه Neon) إلى صيغة Npgsql.
/// يقبل صيغتين:
///   1) رابط URI:  postgresql://user:pass@host/dbname?sslmode=require
///   2) صيغة Npgsql جاهزة فيها "Host=" → تُستخدم كما هي.
/// يُرجع null إذا لم يوجد رابط (فنرجع لـ SQLite محليًا).
/// </summary>
public static class DbConfig
{
    public static string? BuildNpgsqlConnectionString(string? databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
            return null;

        // لو الرابط أصلاً بصيغة Npgsql (يحتوي Host=) نستخدمه مباشرة
        if (databaseUrl.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            return databaseUrl;

        if (!databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return null;

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        return $"Host={uri.Host};Port={port};Database={database};" +
               $"Username={username};Password={password};" +
               "SSL Mode=Require;Trust Server Certificate=true";
    }
}
