# 🚀 كيف تستخدم Swagger محليًا لإضافة إعلانات وتظهر في تطبيقك

## الخطوة 1: شغّل السيرفر

```powershell
cd C:\Users\s\Desktop\RentalsApi
dotnet run
```

سيفتح المتصفح تلقائيًا على: **http://localhost:5080/swagger**

✅ من هنا تقدر تضيف/تعدّل/تحذف الإعلانات.

---

## الخطوة 2: اعرف عنوان كمبيوترك على الشبكة

في PowerShell نفّذ:

```powershell
ipconfig | Select-String "IPv4"
```

ستحصل على رقم مثل: `192.168.1.45` — هذا عنوان كمبيوترك.

> **مهم**: جوالك والكمبيوتر لازم يكونوا على نفس شبكة WiFi.

---

## الخطوة 3: حدّث رابط API في التطبيق

افتح `C:\Users\s\Desktop\my-app\Services\ListingService.cs` وغيّر الرابط:

```csharp
public const string RemoteUrl = "http://192.168.1.45:5080/listings";
//                                       ↑ ضع IP كمبيوترك هنا
```

ثم أعد بناء التطبيق:

```powershell
cd C:\Users\s\Desktop\my-app
dotnet build HelloMaui.csproj -f net9.0-android
```

---

## الخطوة 4: استخدم Swagger لإضافة إعلان بإحداثيات

1. افتح `http://localhost:5080/swagger` في المتصفح.
2. اضغط على **`POST /listings`** → **`Try it out`**.
3. عبّئ النموذج:
   ```json
   {
     "title": "شقة جديدة في المرور",
     "type": "Apartment",
     "price": 3800,
     "location": "حي المرور - أبوظبي",
     "description": "شقة فاخرة بإطلالة مفتوحة",
     "phone": "971501234567",
     "bedrooms": 2,
     "bathrooms": 2,
     "area": 120,
     "featured": true,
     "emoji": "🏢",
     "latitude": 24.4670,
     "longitude": 54.3741
   }
   ```
4. اضغط **`Execute`** ← يظهر `201 Created`.

---

## الخطوة 5: شغّل التطبيق وشوف النقطة الحمراء

- افتح التطبيق على جوالك.
- اذهب لتبويب **"الخريطة"**.
- اسحب لأسفل في تبويب "الإعلانات" لإجبار التحديث.
- ستظهر نقطة حمراء جديدة في موقع الإحداثيات اللي أدخلتها.

---

## 📍 كيف تحصل على إحداثيات أي مبنى في الإمارات؟

1. افتح https://maps.google.com
2. ابحث عن العنوان أو اسحب الخريطة للمكان المطلوب.
3. **انقر بزر الفأرة الأيمن** على المبنى → ستظهر الإحداثيات في الأعلى:
   ```
   24.4670, 54.3741
   ```
4. الرقم الأول = `latitude`، الثاني = `longitude`.
5. ضعها في نموذج Swagger.

> ⚠️ **الخريطة في التطبيق مقيّدة بحدود الإمارات** — أي إحداثيات خارج الإمارات
> (مثل السعودية أو عُمان) لن تُعرض كنقطة.

---

## 🌐 بديل: استخدم ngrok لتسهيل الوصول (بدون IP)

لو ما تبي تتعامل مع IPs، ngrok يعطيك رابط HTTPS عام مجاني:

```powershell
# نزّل ngrok من https://ngrok.com/download (مجاني، يحتاج تسجيل)
# في terminal جديد، السيرفر شغّال على 5080:
ngrok http 5080
```

سيعطيك رابط مثل: `https://abc123.ngrok-free.app`

ضع هذا الرابط في `RemoteUrl`:

```csharp
public const string RemoteUrl = "https://abc123.ngrok-free.app/listings";
```

✅ يعمل من أي شبكة (حتى لو الجوال على بيانات الجوال 4G/5G).

---

## ❓ مشاكل شائعة

| المشكلة | الحل |
|--------|------|
| "Connection refused" من الجوال | تأكد جوالك وكمبيوترك على نفس WiFi |
| لا يظهر شيء في الخريطة | اضغط "اسحب لأسفل" في تبويب الإعلانات لإجبار التحميل |
| Windows Firewall يحجب | شغّل: `New-NetFirewallRule -DisplayName "Rentals API" -Direction Inbound -Port 5080 -Protocol TCP -Action Allow` كـ Administrator |
| النقطة لا تظهر مع وجود إحداثيات | تأكد أنها داخل حدود الإمارات (22.5-26.5 شمال، 51.5-56.5 شرق) |
