# Bài tập LTW — Landing page bán sách "Nhà Giả Kim"

Website bán một đầu sách: landing page công khai + trang quản trị.

**Stack:** ASP.NET Core 9 Web API (C#) · SQL Server (LocalDB) + EF Core 9 · ReactJS 18 + Vite + TypeScript + Tailwind + Ant Design

## Chạy dự án

### 1. Chuẩn bị secret (chỉ làm một lần)

```bash
cd backend/NhaGiaKim.Api
dotnet user-secrets set "Jwt:Key" "<chuoi-ngau-nhien-it-nhat-32-ky-tu>"
dotnet user-secrets set "Seed:AdminPassword" "<mat-khau-admin>"
```

> Secret **không** nằm trong `appsettings.json` và không được commit. Xem `appsettings.Development.example.json`.

### 2. Tạo database

```bash
dotnet ef database update -p backend/NhaGiaKim.Infrastructure -s backend/NhaGiaKim.Api
```

Dùng SQL Server LocalDB (`(localdb)\MSSQLLocalDB`, database `NhaGiaKim`). Đổi chuỗi kết nối ở `appsettings.json` nếu dùng SQL Server khác.

### 3. Chạy backend

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project backend/NhaGiaKim.Api \
  --urls http://localhost:5080 --no-launch-profile
```

- API: http://localhost:5080
- Swagger: http://localhost:5080/swagger
- Lần chạy đầu sẽ **seed** sách mẫu + tài khoản admin `admin@nhagiakim.local`.

### 4. Chạy frontend

```bash
cd frontend
npm install
npm run dev
```

- Landing page: http://localhost:5173
- Trang quản trị: http://localhost:5173/admin/login

Vite proxy `/api` và `/uploads` sang `http://localhost:5080` nên khi dev không vướng CORS.

## Kiểm thử

```bash
bash scripts/gates.sh            # chạy toàn bộ cổng tự động (build, format, test, lint, typecheck, build FE)

dotnet test BaiTapLTW.sln        # 92 test backend
npm --prefix frontend run test:run   # 18 test frontend
```

## Tài liệu

| File | Nội dung | Bước trong quy trình |
|---|---|---|
| [docs/00-workflow.md](docs/00-workflow.md) | Quy trình làm bài, phân tầng, automated gates, Definition of Done | Khung chung |
| [docs/01-spec.md](docs/01-spec.md) | Yêu cầu chức năng/phi chức năng + **30 Acceptance Criteria** | Bước 1 |
| [docs/02-test-design.md](docs/02-test-design.md) | Test case + bảng truy vết AC → test → bằng chứng | Bước 2 |
| [docs/03-plan.md](docs/03-plan.md) | Kiến trúc, cấu trúc thư mục, schema DB, API, 7 giai đoạn, rủi ro | Bước 3 |
| [docs/04-work-packages.md](docs/04-work-packages.md) | Chia work package, DAG, ownership file, chống xung đột | P1–P2 |
| [docs/evidence/](docs/evidence/) | Raw output kiểm chứng | Bước 9 |
| [contracts/README.md](contracts/README.md) | Contract API đã đóng băng (`openapi.json`) | P2 |

Quy trình gốc: `multi_agent_coding_workflow.pdf`

## Cấu trúc

```
backend/
  NhaGiaKim.Domain/           entities + enums, không phụ thuộc gì
  NhaGiaKim.Application/      service, DTO, validator, business rule thuần
  NhaGiaKim.Infrastructure/   EF Core, SQL Server, BCrypt, JWT, file storage
  NhaGiaKim.Api/              controller, middleware, DI, Swagger
  NhaGiaKim.Tests/            65 unit + 27 integration
frontend/src/
  api/                        axios client, endpoint, type khớp contract
  components/landing/         8 section của landing page
  pages/admin/                login, đơn hàng, đánh giá, sách, cấu hình, tài khoản
contracts/openapi.json        contract sinh từ Swagger
scripts/gates.sh              cổng tự động (bước 6)
```

## Nguyên tắc cốt lõi

> Không tin báo cáo "đã xong". Chỉ tin bằng chứng kiểm chứng độc lập được.

## Việc còn lại

- [ ] E2E Playwright cho AC-6 (responsive 360px) và AC-12 (màn hình cảm ơn sau khi đặt hàng)
- [ ] Trang admin cho tác giả / báo chí / review (API đã có, UI chưa làm)
- [ ] Form tạo & sửa tài khoản trong trang Tài khoản (API đã có, UI mới ở mức xem danh sách)
- [ ] Code-splitting cho bundle frontend (hiện 1.28 MB, gzip 409 KB)
- [ ] Ảnh thật cho sách/tác giả/báo chí (hiện là đường dẫn placeholder)
