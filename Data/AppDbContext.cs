using Microsoft.EntityFrameworkCore;
using RentalsApi.Models;

namespace RentalsApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Listing> Listings => Set<Listing>();

    // ملاحظة: أُزيلت المواقع التجريبية (HasData) — القاعدة تبدأ فارغة،
    // وتُضيف مواقعك الحقيقية من صفحة الإدارة /manage.
}
