# Rentals API (لتطبيق عقارات مدينتي)

واجهة برمجية بسيطة (ASP.NET Core 9 + SQLite + Swagger) لإدارة إعلانات الإيجار.
التطبيق المحمول (HelloMaui / my-app) يقرأ منها قائمة الإعلانات ويظهرها كنقاط حمراء على الخريطة.

## ✅ التشغيل المحلي

```powershell
cd C:\Users\s\Desktop\RentalsApi
dotnet run
```

يفتح المتصفح تلقائيًا على: **http://localhost:5080/swagger**

من Swagger UI:
- `GET /listings` → اعرض جميع الإعلانات
- `POST /listings` → اضغط "Try it out" ثم عبّئ النموذج وانقر "Execute"
- `PUT /listings/{id}` → عدّل إعلانًا موجودًا
- `DELETE /listings/{id}` → احذف إعلانًا

> القاعدة (SQLite) تُحفظ في الملف `rentals.db` بنفس المجلد.

## 🧪 اختبار سريع بالمتصفح

افتح: http://localhost:5080/listings → ترى مصفوفة JSON بالإعلانات.

## 🚀 نشر مجاني على Render.com (بدون بطاقة ائتمانية)

1. أنشئ حسابًا على https://render.com (مجاني، يكفي إيميل).
2. ارفع مجلد `RentalsApi` كاملًا إلى مستودع GitHub خاص أو عام.
3. في لوحة Render:
   - **New +** → **Web Service**
   - اختر المستودع.
   - **Runtime**: Docker
   - **Region**: Frankfurt (أقرب لدبي)
   - **Plan**: Free
   - اضغط **Create Web Service**
4. بعد ~5 دقائق سيُعطيك Render رابطًا مثل:
   ```
   https://your-api.onrender.com
   ```
5. افتح: `https://your-api.onrender.com/swagger` ← لوحة Swagger جاهزة.

## 🔗 ربط التطبيق المحمول بالـ API

في `C:\Users\s\Desktop\my-app\Services\ListingService.cs`، غيّر:

```csharp
public const string RemoteUrl =
    "https://your-api.onrender.com/listings";
```

أعد بناء التطبيق:
```powershell
cd C:\Users\s\Desktop\my-app
dotnet build HelloMaui.csproj -f net9.0-android
```

كل إعلان تضيفه من Swagger مع `Latitude` و `Longitude` سيظهر فورًا في تبويب
"الخريطة" داخل التطبيق كنقطة حمراء.

## 📍 طريقة الحصول على إحداثيات أي بناية

1. افتح https://www.google.com/maps
2. ابحث عن البناية أو انقر بزر اليمين على موقعها.
3. ستظهر إحداثيات مثل: `24.4764, 54.3705`
   - الرقم الأول = `Latitude`
   - الرقم الثاني = `Longitude`
4. ضعها في Swagger عند إنشاء الإعلان.

## ⚠️ ملاحظات

- خطة Render المجانية "تنام" بعد 15 دقيقة من عدم الاستخدام، فيستغرق الطلب الأول
  بعد النوم ~30 ثانية. التطبيق يعرض النسخة المحفوظة محليًا أثناء ذلك.
- ملف SQLite على Render الخطة المجانية **مؤقت** (يُمسح عند إعادة النشر).
  للحفاظ على البيانات بشكل دائم: استخدم Postgres المجاني من Render، أو ارفع
  النسخة الاحتياطية يدويًا.
