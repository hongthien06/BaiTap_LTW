#!/usr/bin/env bash
# Buoc 6 - Automated Gates. KHONG duoc co LLM trong file nay.
# Chay: bash scripts/gates.sh   (ket qua day ra stdout, luu lai lam bang chung)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

# API dang chay se khoa file DLL -> build that bai voi loi kho hieu. Bao som.
if tasklist 2>/dev/null | grep -qi "NhaGiaKim.Api.exe"; then
  echo "GATE FAIL: NhaGiaKim.Api dang chay va khoa file DLL. Dung server roi chay lai."
  exit 1
fi

echo "=== [1/7] dotnet build ==="
dotnet build BaiTapLTW.sln --nologo -warnaserror

echo "=== [2/7] dotnet format (chi kiem tra, khong sua) ==="
dotnet format BaiTapLTW.sln --verify-no-changes --no-restore || {
  echo "GATE FAIL: code chua dung dinh dang. Chay 'dotnet format' roi commit lai."
  exit 1
}

echo "=== [3/7] dotnet test ==="
dotnet test BaiTapLTW.sln --nologo --no-build

echo "=== [4/7] frontend typecheck ==="
npm --prefix frontend run typecheck

echo "=== [5/7] frontend lint ==="
npm --prefix frontend run lint

echo "=== [6/7] frontend test ==="
npm --prefix frontend run test:run

echo "=== [7/7] frontend build ==="
npm --prefix frontend run build

echo
echo "=== Kiem tra diff: co test nao bi xoa / skip / noi long khong? ==="
BASE="${GATE_BASE_REF:-origin/main}"
if git rev-parse --verify --quiet "$BASE" >/dev/null; then
  echo "--- File thay doi so voi $BASE ---"
  git diff --name-only "$BASE"...HEAD

  echo "--- Thay doi trong file test ---"
  git diff "$BASE"...HEAD -- '*Tests*.cs' '*.test.ts' '*.test.tsx' || true

  echo "--- Dau hieu skip test (phai la rong) ---"
  if git diff "$BASE"...HEAD | grep -nE '^\+.*(Skip\s*=|it\.skip|describe\.skip|test\.skip|\[Ignore)'; then
    echo "GATE FAIL: co test bi skip trong diff."
    exit 1
  fi
  echo "(khong co)"
else
  echo "Bo qua: chua co ref $BASE de so sanh."
fi

echo
echo "=== GATES PASSED ==="
