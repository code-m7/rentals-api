namespace RentalsApi;

/// <summary>
/// صفحة إدارة عربية (HTML) تُقدَّم على /manage.
/// الهدف: إضافة/حذف/عرض الإعلانات بحقول عربية طبيعية —
/// بدون لصق JSON في Swagger (الذي يسبب انقلاب النص العربي).
/// </summary>
public static class ManagePage
{
    public const string Html = """
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>إدارة الإعلانات</title>
<style>
  * { box-sizing: border-box; }
  body { font-family: system-ui, "Segoe UI", Tahoma, sans-serif; background:#0F1020; color:#e5e7eb; margin:0; padding:16px; }
  h1 { font-size:20px; margin:0 0 14px; }
  h3 { margin:0 0 8px; }
  .card { background:#1A1B2E; border:1px solid #2A2A40; border-radius:14px; padding:14px; margin-bottom:14px; }
  .grid { display:grid; grid-template-columns:1fr 1fr; gap:8px; }
  label { font-size:13px; color:#cbd5e1; display:block; margin-top:6px; }
  input, select, textarea { width:100%; background:#0F1020; color:#fff; border:1px solid #2A2A40; border-radius:8px; padding:9px; font-size:14px; margin-top:3px; }
  button { background:#7C3AED; color:#fff; border:none; border-radius:8px; padding:9px 16px; cursor:pointer; font-size:14px; }
  button:hover { filter:brightness(1.1); }
  button.danger { background:#ef4444; padding:6px 12px; }
  button.ghost { background:#2A2A40; }
  .bar { display:flex; gap:10px; align-items:center; flex-wrap:wrap; margin-bottom:10px; }
  table { width:100%; border-collapse:collapse; }
  td, th { padding:9px; border-bottom:1px solid #2A2A40; text-align:right; font-size:13px; }
  th { color:#9CA3AF; font-weight:600; }
  .muted { color:#9CA3AF; font-size:12px; }
  .ok { color:#4ADE80; font-size:14px; }
  .hint { background:#13233a; border:1px solid #1e3a5f; color:#bcd; border-radius:8px; padding:8px 10px; font-size:12px; margin:6px 0 10px; }
  a { color:#a78bfa; }
</style>
</head>
<body>
  <h1>🏠 إدارة إعلانات الإيجارات</h1>

  <div class="card">
    <h3>➕ إضافة إعلان جديد</h3>
    <div class="hint">اكتب العربي مباشرة في الحقول — ما ينقلب. الإحداثيات: انسخها من رابط قوقل مابس (lat, lng).</div>
    <div class="grid">
      <div><label>العنوان</label><input id="title" placeholder="شقة في المنطقة الغربية"></div>
      <div><label>الموقع</label><input id="location" placeholder="المنطقة الغربية - أبوظبي"></div>
      <div><label>النوع</label>
        <select id="type">
          <option value="Apartment">شقة</option>
          <option value="House">بيت / فيلا</option>
          <option value="Shop">محل</option>
        </select>
      </div>
      <div><label>السعر (د.إ / شهر)</label><input id="price" type="number" value="3500"></div>
      <div><label>الهاتف</label><input id="phone" placeholder="9715xxxxxxxx"></div>
      <div><label>الرمز (إيموجي)</label><input id="emoji" value="🏢"></div>
      <div><label>غرف النوم</label><input id="bedrooms" type="number" value="2"></div>
      <div><label>الحمّامات</label><input id="bathrooms" type="number" value="2"></div>
      <div><label>المساحة (م²)</label><input id="area" type="number" value="120"></div>
      <div><label>مميّز؟</label><select id="featured"><option value="false">لا</option><option value="true">نعم</option></select></div>
      <div><label>Latitude (خط العرض)</label><input id="latitude" type="number" step="any" placeholder="23.6320667"></div>
      <div><label>Longitude (خط الطول)</label><input id="longitude" type="number" step="any" placeholder="53.7001992"></div>
    </div>
    <label>الوصف</label>
    <textarea id="description" rows="2" placeholder="وصف مختصر للإعلان"></textarea>
    <div class="bar" style="margin-top:10px">
      <button onclick="addListing()">➕ أضف الإعلان</button>
      <span id="addMsg" class="ok"></span>
    </div>
  </div>

  <div class="card">
    <div class="bar">
      <h3 style="margin:0">📋 الإعلانات الحالية</h3>
      <button class="ghost" onclick="load()">🔄 Refresh</button>
      <span id="count" class="muted"></span>
      <a href="/swagger" style="margin-inline-start:auto">Swagger ↗</a>
    </div>
    <table>
      <thead><tr><th>#</th><th>العنوان</th><th>الموقع</th><th>السعر</th><th>الإحداثيات</th><th></th></tr></thead>
      <tbody id="rows"></tbody>
    </table>
  </div>

<script>
  async function load() {
    try {
      const r = await fetch('/listings');
      const data = await r.json();
      document.getElementById('count').textContent = data.length + ' إعلان';
      const rows = document.getElementById('rows');
      rows.innerHTML = '';
      data.forEach(function(l) {
        const tr = document.createElement('tr');
        tr.innerHTML =
          '<td>' + l.id + '</td>' +
          '<td>' + esc(l.title) + '</td>' +
          '<td>' + esc(l.location || '') + '</td>' +
          '<td>' + (l.price || 0).toLocaleString() + '</td>' +
          '<td class="muted">' + (l.latitude || '') + ', ' + (l.longitude || '') + '</td>' +
          '<td><button class="danger" onclick="del(' + l.id + ')">حذف</button></td>';
        rows.appendChild(tr);
      });
    } catch (e) { alert('تعذّر تحميل البيانات — تأكد أن السيرفر شغّال.'); }
  }
  function esc(s) {
    return String(s).replace(/[&<>"']/g, function(c) {
      return ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'})[c];
    });
  }
  function v(id) { return document.getElementById(id).value; }
  async function addListing() {
    const body = {
      title: v('title'), type: v('type'), price: Number(v('price')) || 0,
      location: v('location'), description: v('description'), phone: v('phone'),
      bedrooms: Number(v('bedrooms')) || 0, bathrooms: Number(v('bathrooms')) || 0,
      area: Number(v('area')) || 0, featured: v('featured') === 'true', emoji: v('emoji'),
      latitude: parseFloat(v('latitude')), longitude: parseFloat(v('longitude'))
    };
    if (!body.title.trim()) { alert('اكتب العنوان أولاً'); return; }
    if (isNaN(body.latitude) || isNaN(body.longitude)) { alert('اكتب الإحداثيات (latitude و longitude)'); return; }
    const r = await fetch('/listings', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json; charset=utf-8' },
      body: JSON.stringify(body)
    });
    if (r.ok) {
      const m = document.getElementById('addMsg');
      m.textContent = '✓ تمت الإضافة بنجاح';
      setTimeout(function(){ m.textContent = ''; }, 2500);
      ['title','location','phone','description','latitude','longitude'].forEach(function(id){ document.getElementById(id).value = ''; });
      load();
    } else {
      alert('فشلت الإضافة: ' + r.status);
    }
  }
  async function del(id) {
    if (!confirm('حذف الإعلان رقم ' + id + '؟')) return;
    const r = await fetch('/listings/' + id, { method: 'DELETE' });
    if (r.ok) load(); else alert('فشل الحذف: ' + r.status);
  }
  load();
</script>
</body>
</html>
""";
}
