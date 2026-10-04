# 03 — Planning (Bước 3)

> Reviewer ở bước 4 chỉ đọc `01-spec.md` + file này.

## 1. Kiến trúc tổng thể

```
Browser
  ├── React SPA (Vite)  ──HTTP/JSON──┐
  │     /          landing public     │
  │     /admin/*   CMS                │
  │                                   ▼
  │                      ASP.NET Core 9 Web API
  │                        Controller → Service → Repository
  │                                   │ EF Core 9
  │                                   ▼
  └── /uploads/*  (static files)   SQL Server
```

- **Phân tầng BE:** `Controller` (HTTP, không chứa business logic) → `Service` (business rule, transaction) → `AppDbContext` qua interface `IAppDbContext`. DTO riêng cho request/response, **không trả entity trực tiếp**.
- **Không có lớp Repository riêng.** EF Core `DbContext` đã là Unit of Work + Repository; thêm một lớp bọc mỏng chỉ làm tăng code mà không tăng khả năng test (test tích hợp dùng SQLite in-memory, không mock). Service phụ thuộc `IAppDbContext` để tầng Application không tham chiếu Infrastructure.
- **Auth:** JWT Bearer, policy `RequireAdmin` / `RequireStaffOrAdmin`.
- **CORS:** chỉ cho phép origin của FE (dev: `http://localhost:5173`).
- **Error handling:** 1 middleware duy nhất map exception → ProblemDetails + correlation id.

## 2. Cấu trúc thư mục

```
BaiTapLTW/
├── BaiTapLTW.sln                      [Integrator sở hữu]
├── contracts/openapi.json             [đóng băng sau P2]
├── docs/                              [tài liệu quy trình]
├── backend/
│   ├── NhaGiaKim.Api/
│   │   ├── Program.cs                 [Integrator sở hữu]
│   │   ├── Controllers/
│   │   │   ├── PublicController.cs
│   │   │   ├── OrdersController.cs
│   │   │   ├── FeedbacksController.cs
│   │   │   ├── AuthController.cs
│   │   │   └── Admin/{Book,Author,Press,Review,Order,Feedback,User,Setting,Upload}Controller.cs
│   │   ├── Middleware/ExceptionHandlingMiddleware.cs
│   │   └── wwwroot/uploads/
│   ├── NhaGiaKim.Application/
│   │   ├── Services/            (BookService, OrderService, FeedbackService, AuthService, UploadService)
│   │   ├── Dtos/
│   │   ├── Validators/          (FluentValidation)
│   │   └── Common/OrderStateMachine.cs
│   ├── NhaGiaKim.Domain/
│   │   └── Entities/            (Book, BookImage, Author, PressQuote, ContentReview,
│   │                             Feedback, Order, AppUser, Role, SiteSetting)
│   ├── NhaGiaKim.Infrastructure/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/      (IEntityTypeConfiguration)
│   │   ├── Migrations/          [chỉ Integrator chạy add-migration]
│   │   ├── Security/            (BcryptPasswordHasher, JwtTokenGenerator, JwtOptions)
│   │   ├── Storage/LocalFileStorage.cs
│   │   └── Seed/DbSeeder.cs
│   └── NhaGiaKim.Tests/
│       ├── Unit/
│       └── Integration/
├── frontend/
│   ├── src/
│   │   ├── api/                 (axios instance, interceptor, generated types)
│   │   ├── pages/public/        (LandingPage, OrderSuccessPage)
│   │   ├── components/landing/  (HeroSection, BookInfoSection, AuthorSection,
│   │   │                         PressSection, ReviewSection, FeedbackSection,
│   │   │                         OrderFormSection, Footer)
│   │   ├── pages/admin/         (LoginPage, DashboardPage, BookPage, AuthorPage,
│   │   │                         PressPage, ReviewPage, OrderPage, FeedbackPage,
│   │   │                         UserPage, SettingPage)
│   │   ├── components/admin/
│   │   ├── hooks/
│   │   ├── router.tsx           [Integrator sở hữu]
│   │   └── main.tsx             [Integrator sở hữu]
│   └── e2e/                     (Playwright)
└── scripts/gates.sh             [Integrator sở hữu]
```

## 3. Thiết kế database (SQL Server)

| Bảng | Cột chính |
|---|---|
| `Books` | Id, Name, Category, Title, Subtitle, Description, Price `decimal(18,2)`, DiscountPrice `decimal(18,2)?`, CoverImageUrl, MockupImageUrl, IsActive, CreatedAt, UpdatedAt |
| `BookImages` | Id, BookId → Books, Url, SortOrder |
| `Authors` | Id, BookId → Books, FullName, AvatarUrl, Bio `nvarchar(max)` |
| `PressQuotes` | Id, BookId → Books, PressName, LogoUrl, Quote, SourceUrl, SortOrder |
| `ContentReviews` | Id, BookId → Books, Title, Content `nvarchar(max)`, FileUrl |
| `Feedbacks` | Id, BookId → Books, CustomerName, Rating `int` (1–5), Content, IsApproved `bit` default 0, CreatedAt |
| `Orders` | Id, OrderCode `unique`, BookId → Books, CustomerName, Phone, Address, Quantity, UnitPrice, TotalPrice, PaymentMethod, Status, Note, CreatedAt, UpdatedAt |
| `AppUsers` | Id, Email `unique`, PasswordHash, FullName, RoleId → Roles, IsActive, CreatedAt |
| `Roles` | Id, Name (`Admin`, `Staff`) |
| `SiteSettings` | Id, Key `unique`, Value — key/value: logo, hotline, email, address, facebook, youtube, footerText, bankInfo |

**Index:** `Orders.OrderCode` (unique), `Orders.Phone`, `Orders.Status`, `Orders.CreatedAt`, `Feedbacks.IsApproved`.
**Quy ước:** tiền dùng `decimal(18,2)` — **không bao giờ dùng `float/double`**. Thời gian lưu UTC.

## 4. Thiết kế API (sẽ đóng băng vào `contracts/openapi.json`)

### Public (không cần token)

| Method | Route | Mô tả |
|---|---|---|
| GET | `/api/public/landing` | Trả toàn bộ dữ liệu landing trong **1 request** (book, images, author, press, review, settings, ratingSummary, approvedFeedbacks) |
| POST | `/api/orders` | Tạo đơn, trả `{ orderCode }` — rate-limited |
| POST | `/api/feedbacks` | Gửi feedback (mặc định chưa duyệt) — rate-limited |

### Auth

| Method | Route |
|---|---|
| POST | `/api/auth/login` |
| GET | `/api/auth/me` |

### Admin (`[Authorize]`)

| Method | Route | Role |
|---|---|---|
| GET/PUT | `/api/admin/book` | Admin |
| GET/PUT | `/api/admin/author` | Admin |
| GET/POST/PUT/DELETE | `/api/admin/press-quotes[/{id}]` | Admin |
| GET/PUT | `/api/admin/review` | Admin |
| GET | `/api/admin/orders?status=&phone=&from=&to=&page=&pageSize=` | Admin, Staff |
| GET | `/api/admin/orders/{id}` | Admin, Staff |
| PATCH | `/api/admin/orders/{id}/status` | Admin, Staff |
| GET | `/api/admin/feedbacks?isApproved=` | Admin, Staff |
| PATCH | `/api/admin/feedbacks/{id}/approve` | Admin, Staff |
| DELETE | `/api/admin/feedbacks/{id}` | Admin |
| GET/POST/PUT/DELETE | `/api/admin/users[/{id}]` | Admin |
| GET/PUT | `/api/admin/settings` | Admin |
| POST | `/api/admin/upload` | Admin |

**Response lỗi thống nhất:**
```json
{ "type": "validation_error", "title": "...", "status": 400,
  "errors": { "phone": ["SĐT không hợp lệ"] }, "traceId": "..." }
```

## 5. Thứ tự triển khai (7 giai đoạn)

| GĐ | Nội dung | AC phủ | Rủi ro |
|---|---|---|---|
| **G0** | Khởi tạo solution + project FE, Docker SQL Server, CI chạy `gates.sh`, EditorConfig | — | Thấp |
| **G1** | Domain entities + `AppDbContext` + migration đầu + seed (sách mẫu, admin mặc định) | AC-22 | **DB migration = high-risk** |
| **G2** | API public landing + API order + feedback + rate limit | AC-1, 7–11, 13–15 | **Order = high-risk** |
| **G3** | Auth JWT + role policy + admin CRUD + upload | AC-17–20, 22–30 | **Auth, upload = high-risk** |
| **G4** | FE landing page (8 section) + form đặt hàng + form feedback | AC-2–6, 12, 16 | Trung bình |
| **G5** | FE admin (login, orders, feedback, book/author/press/review, users, settings) | AC-21, 23–30 | Trung bình |
| **G6** | E2E Playwright + SEO + responsive + tối ưu ảnh + hardening | AC-6, 12, NFR-5/6/7 | Thấp |

Mỗi giai đoạn chạy **trọn vẹn bước 1→10**, không gộp.

## 6. Rủi ro và phương án

| Rủi ro | Phương án |
|---|---|
| Migration làm hỏng DB | Mỗi migration kèm script rollback; test migration trên DB sạch trước; **không** sửa migration đã commit — tạo migration mới |
| Mã đơn trùng khi concurrent | Unique index trên `OrderCode` + retry 3 lần; test 2 request song song |
| Client tự chế giá | Server **luôn** tính lại `TotalPrice` từ DB (AC-10) |
| Lộ thông tin qua login | Message lỗi chung chung (AC-18) |
| Upload file độc | Kiểm tra magic bytes + whitelist MIME + đổi tên file + chặn execute trong `/uploads` (AC-29) |
| XSS từ feedback | React escape mặc định; **cấm** `dangerouslySetInnerHTML` cho nội dung người dùng (AC-16) |
| CORS mở toang | Whitelist origin theo environment, không dùng `AllowAnyOrigin` kèm credentials |
| Secret trong `appsettings.json` | JWT key và connection string lấy từ User Secrets (dev) / env var (prod); `appsettings.*.json` chứa secret phải vào `.gitignore` |
| FE và BE lệch kiểu dữ liệu | Sinh type TS từ `openapi.json` bằng `openapi-typescript`, không gõ tay interface |

## 7. Rollback plan

- Code: mỗi WP 1 branch; merge vào `integration` trước, `main` chỉ nhận khi bước 10 pass → rollback = `git revert` commit merge.
- DB: giữ `Down()` của mọi migration; backup `.bak` trước khi chạy migration trên DB có dữ liệu.
- File upload: thư mục `/uploads` không bị migration đụng tới.

## 8. File dùng chung (chỉ Integrator được sửa)

`BaiTapLTW.sln` · `Program.cs` · `appsettings*.json` · `AppDbContext.cs` · `Migrations/**` · `frontend/src/router.tsx` · `frontend/src/main.tsx` · `frontend/package.json` · `contracts/openapi.json` · `scripts/gates.sh` · `.github/workflows/**`
