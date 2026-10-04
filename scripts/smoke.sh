#!/usr/bin/env bash
#
# Smoke test tren API DANG CHAY voi SQL SERVER THAT.
#
# Ly do ton tai: test tich hop chay tren SQLite in-memory cho nhanh, nen moi khac biet
# giua hai provider deu lot luoi. Da tung lot mot loi that: transaction SERIALIZABLE
# dung chung voi EnableRetryOnFailure khien PUT /api/admin/users tra 500 tren SQL Server
# trong khi 107 test SQLite van xanh.
#
# Chay:  bash scripts/smoke.sh
# Yeu cau: API dang chay (mac dinh http://localhost:5080) va da seed tai khoan admin.
set -uo pipefail

BASE="${SMOKE_BASE_URL:-http://localhost:5080}"
ADMIN_EMAIL="${SMOKE_ADMIN_EMAIL:-admin@nhagiakim.local}"
ADMIN_PASSWORD="${SMOKE_ADMIN_PASSWORD:-Admin@12345}"

PASS=0
FAIL=0

# check <mo-ta> <ma-mong-doi> <ma-thuc-te>
check() {
  if [ "$2" = "$3" ]; then
    printf '  PASS  %-58s %s\n' "$1" "$3"
    PASS=$((PASS + 1))
  else
    printf '  FAIL  %-58s mong doi %s, nhan %s\n' "$1" "$2" "$3"
    FAIL=$((FAIL + 1))
  fi
}

status() { curl -s -o /dev/null -w '%{http_code}' "$@"; }

json_field() { python -c "import sys,json;print(json.load(sys.stdin)$1)"; }

echo "=== Smoke test tren $BASE (SQL Server that) ==="

if ! curl -s -o /dev/null --max-time 5 "$BASE/api/public/landing"; then
  echo "KHONG KET NOI DUOC toi $BASE - API chua chay?"
  exit 1
fi

# ---------- Cong khai ----------
echo
echo "[1] Endpoint cong khai"
check "GET /api/public/landing" 200 "$(status "$BASE/api/public/landing")"

ORDER_BODY='{"customerName":"Smoke Test","phone":"0966000111","address":"1 Duong Smoke, Quan 1, TPHCM","quantity":2,"paymentMethod":0,"note":null}'
ORDER_RESPONSE=$(curl -s -X POST "$BASE/api/orders" -H 'Content-Type: application/json' -d "$ORDER_BODY")
ORDER_CODE=$(echo "$ORDER_RESPONSE" | json_field "['orderCode']" 2>/dev/null || echo "")
if [ -n "$ORDER_CODE" ]; then
  printf '  PASS  %-58s %s\n' "POST /api/orders tao duoc don" "$ORDER_CODE"
  PASS=$((PASS + 1))
else
  printf '  FAIL  %-58s %s\n' "POST /api/orders tao duoc don" "$ORDER_RESPONSE"
  FAIL=$((FAIL + 1))
fi

check "POST /api/orders SDT sai -> 400" 400 \
  "$(status -X POST "$BASE/api/orders" -H 'Content-Type: application/json' \
     -d '{"customerName":"Smoke","phone":"901234567","address":"1 Duong Smoke, Quan 1, TPHCM","quantity":1,"paymentMethod":0}')"

# ---------- Dang nhap ----------
echo
echo "[2] Xac thuc"
TOKEN=$(curl -s -X POST "$BASE/api/auth/login" -H 'Content-Type: application/json' \
  -d "{\"email\":\"$ADMIN_EMAIL\",\"password\":\"$ADMIN_PASSWORD\"}" | json_field "['accessToken']" 2>/dev/null || echo "")

if [ -z "$TOKEN" ]; then
  echo "  FAIL  Khong dang nhap duoc bang $ADMIN_EMAIL - dung lai."
  exit 1
fi
printf '  PASS  %-58s %s ky tu\n' "POST /api/auth/login" "${#TOKEN}"
PASS=$((PASS + 1))

AUTH=(-H "Authorization: Bearer $TOKEN")

check "GET /api/admin/orders khong token -> 401" 401 "$(status "$BASE/api/admin/orders")"
check "login sai mat khau -> 401" 401 \
  "$(status -X POST "$BASE/api/auth/login" -H 'Content-Type: application/json' \
     -d "{\"email\":\"$ADMIN_EMAIL\",\"password\":\"sai-mat-khau\"}")"

# ---------- Quan tri ----------
echo
echo "[3] Admin API"
check "GET /api/admin/orders" 200 "$(status "${AUTH[@]}" "$BASE/api/admin/orders")"
check "GET /api/admin/book" 200 "$(status "${AUTH[@]}" "$BASE/api/admin/book")"

# M5 - thieu validator thi day la 500
check "PUT /api/admin/book body rong -> 400 (khong phai 500)" 400 \
  "$(status -X PUT "$BASE/api/admin/book" -H 'Content-Type: application/json' "${AUTH[@]}" -d '{}')"

# M9 - key setting ngoai whitelist
check "PUT /api/admin/settings key la -> 400" 400 \
  "$(status -X PUT "$BASE/api/admin/settings" -H 'Content-Type: application/json' "${AUTH[@]}" \
     -d '{"items":[{"key":"smtp.password","value":"bimat","description":null}]}')"

# M8 - tat quyen sach cuoi cung se lam landing chet
BOOK=$(curl -s "${AUTH[@]}" "$BASE/api/admin/book")
BOOK_OFF=$(echo "$BOOK" | python -c "
import sys,json
b=json.load(sys.stdin)
b.pop('id',None); b.pop('updatedAt',None); b['isActive']=False
print(json.dumps(b))")
check "PUT /api/admin/book tat sach cuoi cung -> 400" 400 \
  "$(status -X PUT "$BASE/api/admin/book" -H 'Content-Type: application/json' "${AUTH[@]}" -d "$BOOK_OFF")"
check "landing van song sau do" 200 "$(status "$BASE/api/public/landing")"

# ---------- Vong doi tai khoan: day la duong tung tra 500 ----------
echo
echo "[4] Vong doi tai khoan (duong SERIALIZABLE + EnableRetryOnFailure)"
SMOKE_USER="smoke-$(date +%s)@nhagiakim.local"
CREATED=$(curl -s -X POST "$BASE/api/admin/users" -H 'Content-Type: application/json' "${AUTH[@]}" \
  -d "{\"email\":\"$SMOKE_USER\",\"password\":\"Smoke@12345\",\"fullName\":\"Smoke\",\"role\":\"Staff\"}")
USER_ID=$(echo "$CREATED" | json_field "['id']" 2>/dev/null || echo "")

if [ -z "$USER_ID" ]; then
  printf '  FAIL  %-58s %s\n' "POST /api/admin/users" "$CREATED"
  FAIL=$((FAIL + 1))
else
  printf '  PASS  %-58s id=%s\n' "POST /api/admin/users" "$USER_ID"
  PASS=$((PASS + 1))

  STAFF_TOKEN=$(curl -s -X POST "$BASE/api/auth/login" -H 'Content-Type: application/json' \
    -d "{\"email\":\"$SMOKE_USER\",\"password\":\"Smoke@12345\"}" | json_field "['accessToken']" 2>/dev/null || echo "")
  STAFF_AUTH=(-H "Authorization: Bearer $STAFF_TOKEN")

  check "Staff GET /api/admin/orders -> 200" 200 "$(status "${STAFF_AUTH[@]}" "$BASE/api/admin/orders")"
  check "Staff PUT /api/admin/book -> 403" 403 \
    "$(status -X PUT "$BASE/api/admin/book" -H 'Content-Type: application/json' "${STAFF_AUTH[@]}" -d "$BOOK")"

  # Chinh la call tung tra 500 vi transaction khong di qua execution strategy.
  check "PUT /api/admin/users/{id} khoa tai khoan -> 200" 200 \
    "$(status -X PUT "$BASE/api/admin/users/$USER_ID" -H 'Content-Type: application/json' "${AUTH[@]}" \
       -d '{"fullName":"Smoke","role":"Staff","isActive":false,"newPassword":null}')"

  # F-1 tu buoc 8: khoa tai khoan phai thu hoi phien NGAY.
  check "token cu cua tai khoan da khoa -> 401" 401 "$(status "${STAFF_AUTH[@]}" "$BASE/api/admin/orders")"

  check "DELETE /api/admin/users/{id} -> 200" 200 \
    "$(status -X DELETE "$BASE/api/admin/users/$USER_ID" "${AUTH[@]}")"
fi

# ---------- Ket qua ----------
echo
echo "=== PASS: $PASS   FAIL: $FAIL ==="
[ "$FAIL" -eq 0 ] || exit 1
echo "SMOKE PASSED"
