#!/bin/sh
# Generates the runtime SPA config consumed by index.html's <script src="/config.js">.
# APP_API_BASE_URL is empty by default => same-origin (nginx proxies the API), which keeps cookies simple.
set -e

: "${APP_API_BASE_URL:=}"

cat > /usr/share/nginx/html/config.js <<EOF
window.APP_CONFIG = { apiBaseUrl: "${APP_API_BASE_URL}" };
EOF

echo "Generated /config.js with apiBaseUrl='${APP_API_BASE_URL}'"
