#!/usr/bin/env bash
# =====================================================================
#  setup.sh — إعداد كامل لـ RentalsApi على Ubuntu Server بأمر واحد
#  يثبّت .NET 9، يبني المشروع، ينشئ خدمة systemd دائمة، ويفتح المنفذ.
#  الاستخدام:   bash setup.sh
# =====================================================================
set -e

APP_DIR="/opt/rentalsapi"
PORT="5080"
DOTNET="$HOME/.dotnet/dotnet"

echo "==> [1/6] تحديث النظام..."
sudo apt-get update
sudo DEBIAN_FRONTEND=noninteractive apt-get upgrade -y

echo "==> [2/6] تثبيت .NET 9 SDK..."
if [ ! -x "$DOTNET" ]; then
  sudo apt-get install -y curl
  curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  /tmp/dotnet-install.sh --channel 9.0
fi
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$PATH:$HOME/.dotnet"
grep -q 'DOTNET_ROOT' ~/.bashrc || echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
grep -q '\.dotnet' ~/.bashrc       || echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
echo "    إصدار .NET: $($DOTNET --version)"

echo "==> [3/6] نشر المشروع إلى $APP_DIR ..."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"
"$DOTNET" publish -c Release -o /tmp/rentalsapi-publish
sudo mkdir -p "$APP_DIR"
sudo cp -r /tmp/rentalsapi-publish/. "$APP_DIR"/
sudo chown -R "$USER":"$USER" "$APP_DIR"

echo "==> [4/6] إنشاء خدمة systemd..."
sudo tee /etc/systemd/system/rentalsapi.service >/dev/null <<EOF
[Unit]
Description=Rentals API
After=network.target

[Service]
WorkingDirectory=$APP_DIR
ExecStart=$DOTNET $APP_DIR/RentalsApi.dll
Restart=always
RestartSec=5
Environment=PORT=$PORT
Environment=DOTNET_ROOT=$HOME/.dotnet
User=$USER

[Install]
WantedBy=multi-user.target
EOF

echo "==> [5/6] تشغيل الخدمة..."
sudo systemctl daemon-reload
sudo systemctl enable --now rentalsapi

echo "==> [6/6] فتح المنفذ $PORT في الجدار الناري..."
sudo ufw allow "$PORT"/tcp >/dev/null 2>&1 || true

IP=$(hostname -I | awk '{print $1}')
echo ""
echo "============================================================"
echo "✅ تم! السيرفر يعمل الآن كخدمة دائمة."
echo "   من نفس الشبكة:   http://$IP:$PORT/manage"
echo "   حالة الخدمة:     sudo systemctl status rentalsapi"
echo "   السجل المباشر:   sudo journalctl -u rentalsapi -f"
echo ""
echo "   للوصول من أي شبكة (4G/WiFi مختلف): ثبّت ngrok — راجع"
echo "   الجزء 8 في UBUNTU_SERVER_SETUP.md"
echo "============================================================"
