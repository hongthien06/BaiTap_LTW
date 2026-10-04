# 05 — Kết quả Code Review (bước 7) và Challenge (bước 8)

Ngày: 2026-10-04. Hai reviewer chạy **độc lập, context sạch**, không đọc `docs/evidence/**` và `docs/02-test-design.md` (tránh nhiễm tự đánh giá của người viết code).

## Tổng kết

| Bước | Kết quả |
|---|---|
| **8 — Challenge** (tấn công hệ thống đang chạy) | 1 finding (**F-1**, major). Chống được: giả giá, số lượng/enum ngoài dải, bypass rate limit bằng `X-Forwarded-For`, JWT `alg=none` và sửa payload, ma trận phân quyền Staff/Admin, polyglot upload, injection, dữ liệu rác |
| **7 — Code Review** | **3 blocker + 8 major + 16 minor**. Kết luận của reviewer: *"Không thể pass bước 7"* |

Chỗ nào mình **không nhận bừa** đều ghi rõ bên dưới.

## Blocker

| ID | Vấn đề | Trạng thái |
|---|---|---|
| **B1** | `vite.config.ts` không giới hạn `include` → Vitest gom cả `e2e/*.spec.ts` của Playwright → `npm run test:run` exit 1 → gate [6/7] đỏ | ✅ Đã sửa **trước khi** reviewer báo cáo (reviewer đọc working tree cũ). Xác minh lại: 25/25 pass |
| **B2** | `OrderService`: `catch (DbUpdateException) when (attempt < MaxCodeAttempts)` → lần thử thứ 3 **không** được bắt → bay lên thành **500** thay vì 409; nhánh trả 409 là **code chết**. `catch` còn quá rộng, nuốt cả timeout/FK rồi retry ghi thêm 2 lần | ✅ Đã sửa. Thêm `IDbExceptionClassifier` (Infrastructure nhận diện mã lỗi 2601/2627 của SQL Server), chỉ retry khi đúng là trùng mã đơn; `Detach` thay cho `Remove` để nói đúng ý định |
| **B3** | Khách từng đăng nhập admin, mở landing sau 60 phút → `me()` trả 401 → interceptor **lôi ra `/admin/login`** ngay giữa trang bán hàng. Vỡ AC-2…AC-6, AC-12 cho nhóm người đó | ✅ Đã sửa 2 lớp: `useAuth` không gọi `me()` khi ngoài khu `/admin`; interceptor chỉ chuyển hướng khi `isOnAdminPage()`. Thêm 2 test hồi quy |

## Major

| ID | Vấn đề | Trạng thái |
|---|---|---|
| **F-1** | Khoá tài khoản (`IsActive = false`) hoặc hạ quyền **không có tác dụng tới 60 phút** — token cũ vẫn vào được API admin | ✅ Đã sửa. `ActiveUserValidator` chạy ở `OnTokenValidated`: đối chiếu `IsActive` và role với DB ở **mọi** request. Trả giá bằng 1 truy vấn nhỏ mỗi request — chấp nhận được cho khu quản trị |
| **M4** | Timing attack ở login: email không tồn tại trả về sau ~2ms, email có thật sai mật khẩu ~300ms → dò được email admin. AC-18 chỉ đạt một nửa | ✅ Đã sửa. Luôn chạy đúng một phép BCrypt verify, dùng `IPasswordHasher.DummyHash` khi không tìm thấy user |
| **M5** | **Không có validator nào** cho DTO admin → body thiếu field → `NullReferenceException` → **500** thay vì 400. Vi phạm NFR-3 | ✅ Đã sửa. 8 validator mới trong `AdminRequestValidators.cs`, giới hạn độ dài khớp cấu hình EF |
| **M6** | Middleware nuốt **mọi** `ArgumentException` thành 400, **không log**, lộ nguyên văn message ở production. Lỗi 500 thật bị phân loại nhầm | ✅ Đã sửa. Thêm `FileValidationException` riêng; `ArgumentException` rơi xuống nhánh 500 có log |
| **M7** | `Response.Clear()` trong middleware xoá luôn header CORS → trình duyệt chặn response lỗi → FE không bao giờ đọc được `traceId` mà NFR-8 hứa | ✅ Đã sửa. Giữ lại header `Access-Control-*` qua `Clear()` |
| **M8** | `AdminContentService.ActiveBookAsync` **không** lọc `IsActive` → admin sửa quyển này, landing đọc quyển khác (AC-23 im lặng fail). Và tắt quyển cuối cùng là sập landing | ✅ Đã sửa cả hai: thêm filter `IsActive`, chặn tắt quyển active cuối cùng |
| **M9** | `LandingService` đổ **toàn bộ** bảng `SiteSettings` ra endpoint công khai; admin tạo được key tuỳ ý | ✅ Đã sửa. `PublicSettingKeys` whitelist dùng chung cho cả nơi đọc và nơi ghi |
| **M10** | Luật "phải còn ít nhất 1 Admin" là read-then-write không transaction → 2 request song song hạ quyền 2 Admin cuối cùng là lockout vĩnh viễn. `DeleteAsync` còn không xét `IsActive` trong khi `UpdateAsync` có | ✅ Đã sửa. Transaction SERIALIZABLE, kiểm tra **sau** khi ghi rồi rollback; thống nhất điều kiện giữa update và delete |
| **M11** | NFR-7 chưa làm: không có JSON-LD `Book` + `AggregateRating`; OG tag chỉ tồn tại sau khi JS chạy nên crawler không thấy | ✅ Đã sửa. OG tag tĩnh trong `index.html`, JSON-LD chèn runtime, title cắt về 60 ký tự |

## Minor đã sửa

| ID | Vấn đề |
|---|---|
| m12 | `LocalFileStorage` ghi xong hết file rồi mới đo kích thước → viết lại thành chặn ngay trong lúc copy |
| m13 | Đọc header 16 byte bằng một `ReadAsync` có thể short-read → `ReadAtLeastAsync` |
| m15 | `logout()` không xoá cache TanStack Query → dữ liệu người trước còn lại cho người sau trong cùng tab |
| m16 | API có 3 hình dạng lỗi khác nhau; endpoint upload thiếu `traceId` → gom về một ProblemDetails |
| m17 | URL `javascript:` từ field admin nhập được đưa thẳng vào `href` → validate ở BE (`UrlRules`) + guard ở FE (`safeHref`) |
| m19 | `MigrateOnStartup: true` cho **mọi** môi trường → chuyển sang `false`, chỉ bật ở Development |
| m20 | `WHERE LOWER(Email) = @email` không dùng được unique index → bỏ `.ToLower()` |
| m25 | `/uploads` thiếu `X-Content-Type-Options: nosniff` và `Content-Disposition` |
| m26 | `AdminUserService` có **0 test**; AC-8 chưa kiểm "không tạo bản ghi"; Staff xoá feedback chưa có test → thêm 11 test |
| m27 | Tài liệu ghi ReactJS 18 nhưng thực tế React 19; plan gọi contract là `openapi.yaml` còn repo có `openapi.json` → sửa tài liệu |

## Minor chưa sửa, có lý do

| ID | Vấn đề | Vì sao chưa làm |
|---|---|---|
| m14 | Rate limit partition theo IP sẽ sai khi đứng sau reverse proxy; IPv6 nên gom theo /64 | Chưa có kế hoạch deploy sau proxy. Bật `UseForwardedHeaders` mà không khai báo `KnownProxies` thì **mở ra lỗ hổng giả mạo IP** — tệ hơn hiện trạng. Để lại khi nào biết hạ tầng thật |
| m18 | Thuộc tính `download` bị bỏ qua khi `/uploads` khác origin với SPA | Đã giảm nhẹ bằng `Content-Disposition: attachment` ở m25. Giải quyết triệt để cần quyết định về deploy |
| m22 | Token trong `localStorage` | Chuyển sang refresh token trong HttpOnly cookie là thay đổi kiến trúc auth, vượt phạm vi bài tập. Rủi ro chính đi kèm (XSS) đã đóng ở m17 |
| m21, m24 | Reviewer ghi "đã kiểm tra, không có vấn đề" | Không phải defect |

## Nghi vấn reviewer nêu mà **không** thành finding

- **PDF polyglot** (`%PDF-` + HTML): upload được nhưng extension bị ép theo magic byte, tên file là GUID, nay thêm `nosniff` + `Content-Disposition: attachment` → không dựng được XSS thực tế.
- **SiteSettings key lạ**: giờ đã bị whitelist chặn (M9).

## Một lỗi phát sinh **từ chính bản vá** — và cách bắt được

Sửa M10 xong, 107 test backend vẫn xanh, nhưng chạy thật trên SQL Server thì `PUT /api/admin/users/{id}` trả **500**:

```
The configured execution strategy 'SqlServerRetryingExecutionStrategy' does not support
user-initiated transactions.
```

Nguyên nhân: test tích hợp chạy **SQLite in-memory** (không bật retry), còn production dùng **SQL Server có `EnableRetryOnFailure`**. Một khác biệt provider mà 107 test không với tới được.

Hai việc đã làm:
1. Transaction đi qua `Database.CreateExecutionStrategy()` (`AppDbContext.ExecuteInSerializableTransactionAsync`).
2. Thêm **`scripts/smoke.sh`** — 18 kiểm chứng chạy trên **API thật + SQL Server thật**, phủ đúng những đường mà SQLite không đại diện được. Đây giờ là bước bắt buộc trước khi tuyên bố Done.

> Bài học ghi lại: *test xanh trên provider thay thế không phải bằng chứng cho provider thật.*
