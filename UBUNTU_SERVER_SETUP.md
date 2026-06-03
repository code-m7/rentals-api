# 🐧 تشغيل RentalsApi على Ubuntu Server داخل VirtualBox

دليل كامل لتشغيل واجهة الإعلانات على سيرفر Ubuntu خاص بك، بدون أي خدمة سحابية.
بعد الانتهاء ستضيف/تعدّل اللوكيشن من صفحة `/manage`، ويقرأها تطبيقك المحمول.

---

## الجزء 1: إنشاء الجهاز الافتراضي في VirtualBox

1. حمّل **Ubuntu Server 24.04 LTS** من: https://ubuntu.com/download/server
2. في VirtualBox: **New** →
   - Type: Linux، Version: Ubuntu (64-bit)
   - الذاكرة (RAM): **2048 MB** أو أكثر
   - القرص: **15 GB** (ديناميكي)
3. قبل التشغيل: **Settings → Network → Adapter 1**
   - اختر **Bridged Adapter** (مهم: ليحصل السيرفر على عنوان IP من نفس راوتر WiFi،
     فيصل إليه جوالك مباشرة).
4. شغّل الـ VM، وثبّت Ubuntu Server (اقبل الافتراضيات، وفعّل **Install OpenSSH server**
   لتتمكن من الدخول من ويندوز لاحقًا).

---

## الجزء 2: تثبيت .NET 9 على Ubuntu

بعد الدخول للسيرفر (مباشرة أو عبر SSH)، نفّذ:

```bash
# تحديث النظام
sudo apt-get update && sudo apt-get upgrade -y

# تثبيت .NET 9 SDK عبر سكربت مايكروسوفت الرسمي (يعمل على أي إصدار Ubuntu)
sudo apt-get install -y curl
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

# جعل أمر dotnet متاحًا دائمًا
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

dotnet --version   # يجب أن يطبع 9.x.x
```

---

## الجزء 3: نقل المشروع إلى السيرفر

اختر إحدى الطريقتين:

**أ) عبر Git (إن كان المشروع على GitHub):**
```bash
sudo apt-get install -y git
git clone <رابط-مستودعك>.git ~/RentalsApi
```

**ب) عبر SCP من ويندوز** (في PowerShell على جهازك، بدّل `IP` بعنوان الـVM):
```powershell
scp -r C:\Users\s\Desktop\test\RentalsApi  USERNAME@IP:/home/USERNAME/RentalsApi
```

---

## الجزء 4: بناء وتشغيل تجريبي

```bash
cd ~/RentalsApi
dotnet publish -c Release -o /opt/rentalsapi
cd /opt/rentalsapi
dotnet RentalsApi.dll
```

تظهر رسالة: `Now listening on: http://0.0.0.0:5080`
اضغط `Ctrl+C` لإيقافه — سنجعله خدمة دائمة في الخطوة التالية.

---

## الجزء 5: جعله خدمة دائمة (systemd) تعمل بعد كل إقلاع

أنشئ ملف الخدمة:

```bash
sudo nano /etc/systemd/system/rentalsapi.service
```

الصق التالي (بدّل `USERNAME` باسم مستخدمك):

```ini
[Unit]
Description=Rentals API
After=network.target

[Service]
WorkingDirectory=/opt/rentalsapi
ExecStart=/home/USERNAME/.dotnet/dotnet /opt/rentalsapi/RentalsApi.dll
Restart=always
RestartSec=5
Environment=PORT=5080
Environment=DOTNET_ROOT=/home/USERNAME/.dotnet

[Install]
WantedBy=multi-user.target
```

ثم فعّله:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now rentalsapi
sudo systemctl status rentalsapi   # يجب أن يظهر active (running)
```

---

## الجزء 6: فتح المنفذ في الجدار الناري

```bash
sudo ufw allow 5080/tcp
sudo ufw enable      # اختياري إن لم يكن مفعّلًا
```

---

## الجزء 7: معرفة عنوان السيرفر وربط التطبيق

اعرف عنوان IP للـ VM:

```bash
ip -4 addr show | grep inet
```

ستجد عنوانًا مثل `192.168.1.50`. جرّب من متصفح جهازك:

```
http://192.168.1.50:5080/manage
```

إذا ظهرت صفحة الإدارة → كل شيء يعمل! 🎉

أخيرًا في التطبيق المحمول، عدّل الرابط:
`C:\Users\s\Desktop\my-app\Services\ListingService.cs`

```csharp
public const string RemoteUrl = "http://192.168.1.50:5080/listings";
```

وأعد بناء التطبيق:
```powershell
cd C:\Users\s\Desktop\my-app
dotnet build HelloMaui.csproj -f net9.0-android
```

> ✋ ملاحظة: الرابط `192.168.x.x` يعمل فقط عندما يكون **الجوال على نفس WiFi** السيرفر.
> للوصول من **أي شبكة** (WiFi مختلف، أو بيانات 4G/5G) اتبع الجزء 8 أدناه.

---

## الجزء 8: الوصول من أي شبكة عبر نفق ngrok (تجربة حقيقية)

هذا يعطي السيرفر رابطًا عامًّا على الإنترنت (`https://...`) يصل إليه جوالك من أي مكان.
ميزة إضافية: لا يحتاج تغيير شبكة VirtualBox — يعمل حتى مع NAT.

### 1) أنشئ حسابًا مجانيًا
- سجّل في https://ngrok.com (مجاني).
- من لوحة التحكم انسخ **Authtoken** الخاص بك.
- ومن **Domains** احصل على **نطاق ثابت مجاني** (نطاق واحد مجاني لكل حساب)،
  مثل: `your-name.ngrok-free.app` — يجعل الرابط ثابتًا فلا تعيد بناء التطبيق كل مرة.

### 2) ثبّت ngrok على السيرفر
```bash
curl -sSL https://ngrok-agent.s3.amazonaws.com/ngrok.asc \
  | sudo tee /etc/apt/trusted.gpg.d/ngrok.asc >/dev/null
echo "deb https://ngrok-agent.s3.amazonaws.com buster main" \
  | sudo tee /etc/apt/sources.list.d/ngrok.list
sudo apt-get update && sudo apt-get install -y ngrok

# اربط حسابك (بدّل التوكن بتوكنك)
ngrok config add-authtoken YOUR_AUTHTOKEN
```

### 3) شغّل النفق (بدّل النطاق بنطاقك الثابت)
```bash
ngrok http --url=your-name.ngrok-free.app 5080
```

يبقى يعمل ويظهر لك الرابط العام. جرّبه من جوالك على **بيانات 4G** (أطفئ WiFi):
```
https://your-name.ngrok-free.app/manage
```

### 4) اجعل النفق دائمًا (خدمة systemd)
حتى يعمل بعد كل إقلاع بدون أن تبقي النافذة مفتوحة:
```bash
sudo nano /etc/systemd/system/ngrok.service
```
```ini
[Unit]
Description=ngrok tunnel
After=network.target rentalsapi.service

[Service]
ExecStart=/usr/local/bin/ngrok http --url=your-name.ngrok-free.app 5080
Restart=always
RestartSec=5
User=USERNAME

[Install]
WantedBy=multi-user.target
```
```bash
sudo systemctl daemon-reload
sudo systemctl enable --now ngrok
```

### 5) وجّه التطبيق المحمول للرابط الثابت
في `C:\Users\s\Desktop\my-app\Services\ListingService.cs`:
```csharp
public const string RemoteUrl = "https://your-name.ngrok-free.app/listings";
```
أعد بناء التطبيق مرة واحدة، وبعدها يعمل من أي شبكة. 🎉

---

## 🔧 أوامر صيانة مفيدة

| الغرض | الأمر |
|------|------|
| عرض سجل السيرفر المباشر | `sudo journalctl -u rentalsapi -f` |
| إعادة التشغيل | `sudo systemctl restart rentalsapi` |
| الإيقاف | `sudo systemctl stop rentalsapi` |
| نسخة احتياطية للبيانات | `cp /opt/rentalsapi/rentals.db ~/rentals-backup.db` |

> 💾 بياناتك كلها في ملف واحد: `/opt/rentalsapi/rentals.db`. انسخه لتحتفظ بنسخة احتياطية.

---

## ❓ مشاكل شائعة

| المشكلة | الحل |
|--------|------|
| لا يفتح الرابط من الجوال | تأكد أن محوّل الشبكة **Bridged** وأن الجوال على نفس WiFi |
| `dotnet: command not found` في الخدمة | تأكد أن مسار `ExecStart` يشير لمكان dotnet الصحيح (`which dotnet`) |
| المنفذ محجوب | `sudo ufw allow 5080/tcp` |
| الخدمة لا تعمل | `sudo journalctl -u rentalsapi -n 50` لرؤية الخطأ |
