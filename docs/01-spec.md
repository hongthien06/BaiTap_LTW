# 01 — Requirement Analysis (Bước 1)

**Output của bước 1. Reviewer ở bước 4 chỉ được đọc file này + `03-plan.md`.**

## 1. Bối cảnh

Website bán **một đầu sách duy nhất** — "Nhà Giả Kim" (one-product landing page), gồm:

- **Trang công khai (Landing page):** giới thiệu sách, tác giả, báo chí nói về sách, review nội dung, feedback sao của độc giả, form đặt hàng.
- **Trang quản trị (Admin):** quản lý thông tin sách, đơn đặt hàng, feedback, tài khoản + phân quyền, cấu hình site.

Ghi chú trên bảng: phần **"hard code"** = Hero / Chi tiết tác giả / Báo chí có **layout cố định trong React**, nhưng **nội dung lấy từ API** để admin sửa được. Không hard-code chuỗi text vào JSX.

## 2. Tech stack (đã chốt)

| Lớp | Công nghệ |
|---|---|
| Backend | **ASP.NET Core 9 Web API (C#)**, EF Core 9, FluentValidation, JWT Bearer, Serilog, Swagger |
| Database | **SQL Server** (dev: LocalDB / Docker `mssql`), migration bằng EF Core Migrations |
| Frontend | **ReactJS 19 + Vite + TypeScript**, React Router 7, TanStack Query, Axios, react-hook-form + Zod |
| UI | TailwindCSS cho landing; Ant Design cho admin |
| Test | xUnit + Shouldly + `WebApplicationFactory` + SQLite in-memory (BE), Vitest + React Testing Library (FE), Playwright (E2E) |

## 3. Yêu cầu chức năng

### 3.1 Landing page (public)

| ID | Chức năng | Mô tả |
|---|---|---|
| F-01 | Hero Section | Ảnh mockup sách, Title, Subtitle, giá + giá giảm, nút **Đặt mua ngay** |
| F-02 | Thông tin sách | Tên sách, thể loại, mô tả, ảnh (nhiều ảnh) |
| F-03 | Chi tiết tác giả | Ảnh + tiểu sử tác giả |
| F-04 | Báo chí viết về sách | Danh sách trích dẫn: tên báo, logo, trích đoạn, link |
| F-05 | Review nội dung | Nội dung review + tải **file review** (PDF) |
| F-06 | Feedback (số sao) | Hiển thị điểm trung bình + danh sách feedback đã duyệt; form gửi feedback mới (1–5 sao) |
| F-07 | Đặt hàng | Form: Tên, SĐT, Địa chỉ, Số lượng, Phương thức thanh toán, Ghi chú → sinh **Mã đơn** |
| F-08 | Footer | Thông tin liên hệ, social, bản quyền — lấy từ Setting |

### 3.2 Trang quản trị (admin)

| ID | Chức năng | Mô tả |
|---|---|---|
| F-10 | Đăng nhập | Email + mật khẩu → JWT |
| F-11 | Quản lý sách | CRUD: tên, thể loại, title, subtitle, giá, giá giảm, ảnh, mô tả |
| F-12 | Quản lý tác giả | CRUD thông tin tác giả |
| F-13 | Quản lý báo chí | CRUD trích dẫn báo chí |
| F-14 | Quản lý review | CRUD nội dung review + upload file review |
| F-15 | Quản lý đơn hàng | Danh sách, lọc theo trạng thái/ngày/SĐT, xem chi tiết, **đổi trạng thái** |
| F-16 | Quản lý feedback | Duyệt / ẩn / xoá feedback |
| F-17 | Tài khoản & Role | CRUD user admin, gán role **Admin** / **Staff** |
| F-18 | Setting | Logo, hotline, email, địa chỉ, link social, text footer |

### 3.3 Phân quyền

| Hành động | Admin | Staff |
|---|---|---|
| Xem đơn hàng, đổi trạng thái đơn | Có | Có |
| Duyệt/ẩn feedback | Có | Có |
| Sửa nội dung sách / tác giả / báo chí / review / setting | Có | Không |
| Quản lý tài khoản và role | Có | Không |

### 3.4 Trạng thái đơn hàng (state machine)

`New → Confirmed → Shipping → Completed`, và `New|Confirmed → Cancelled`.
**Không được** nhảy lùi, không được chuyển từ `Completed`/`Cancelled` sang trạng thái khác.

## 4. Yêu cầu phi chức năng

- **NFR-1 Bảo mật:** mật khẩu hash BCrypt; JWT hết hạn 60 phút; mọi endpoint `/api/admin/**` yêu cầu token hợp lệ; policy-based authorization theo role.
- **NFR-2 Chống spam:** rate limit 5 req/phút/IP cho `POST /api/orders` và `POST /api/feedbacks`.
- **NFR-3 Validation:** validate ở cả FE (Zod) và BE (FluentValidation). **BE là nguồn chân lý** — không tin FE.
- **NFR-4 Upload:** chỉ nhận `.jpg/.png/.webp` (ảnh, tối đa 5MB) và `.pdf` (file review, tối đa 20MB); kiểm tra **magic bytes**, không chỉ extension; lưu tên file sinh ngẫu nhiên.
- **NFR-5 Hiệu năng:** landing page LCP dưới 2.5s; ảnh lazy-load + WebP.
- **NFR-6 Responsive:** chạy đúng ở 360px / 768px / 1440px.
- **NFR-7 SEO:** title, meta description, OG tags, JSON-LD `Book` + `AggregateRating`.
- **NFR-8 Log:** Serilog ghi file + console; mọi lỗi 5xx có correlation id.

## 5. Acceptance Criteria (Given / When / Then)

> Mọi AC phải truy vết được tới ít nhất 1 test ở `02-test-design.md`.

### Landing page

- **AC-1** — *Given* DB có 1 bản ghi sách đang active, *When* GET `/api/public/landing`, *Then* trả 200 kèm object chứa `book`, `author`, `pressQuotes[]`, `review`, `settings`, `ratingSummary`.
- **AC-2** — *Given* API trả dữ liệu landing, *When* mở `/`, *Then* Hero hiển thị đúng title, subtitle, ảnh mockup và giá; nếu có `discountPrice` thì giá gốc hiển thị gạch ngang.
- **AC-3** — *Given* sách không có `discountPrice`, *When* render Hero, *Then* chỉ hiện 1 giá, không có thẻ gạch ngang và không có badge "giảm giá".
- **AC-4** — *Given* có 3 trích dẫn báo chí, *When* mở `/`, *Then* section Báo chí render đúng 3 item, mỗi item có tên báo và link mở tab mới (`rel="noopener"`).
- **AC-5** — *Given* review có `fileUrl`, *When* bấm "Tải bản review", *Then* trình duyệt tải file PDF đúng đường dẫn; nếu `fileUrl` null thì nút không hiển thị.
- **AC-6** — *Given* màn hình rộng 360px, *When* mở `/`, *Then* không có horizontal scroll và mọi nút CTA vẫn bấm được.

### Đặt hàng (high-risk)

- **AC-7** — *Given* form đặt hàng điền hợp lệ (Tên tối thiểu 2 ký tự, SĐT khớp `^0\d{9}$`, Địa chỉ tối thiểu 10 ký tự, Số lượng 1–99, PTTT thuộc {COD, BankTransfer}), *When* POST `/api/orders`, *Then* trả 201 kèm `orderCode` dạng `NGK-YYYYMMDD-XXXX` và đơn được lưu với trạng thái `New`.
- **AC-8** — *Given* SĐT sai định dạng, *When* POST `/api/orders`, *Then* trả **400** kèm `errors.phone`, và **không** tạo bản ghi nào trong DB.
- **AC-9** — *Given* `quantity = 0` hoặc `quantity = 100`, *When* POST `/api/orders`, *Then* trả 400. *(biên: 1 và 99 phải pass)*
- **AC-10** — *Given* client gửi `totalPrice` tự chế trong body, *When* POST `/api/orders`, *Then* server **bỏ qua** giá client gửi và tính lại từ giá trong DB nhân số lượng.
- **AC-11** — *Given* 1 IP đã gửi 5 đơn trong 1 phút, *When* gửi đơn thứ 6, *Then* trả **429**.
- **AC-12** — *Given* đơn tạo thành công, *When* FE nhận 201, *Then* hiện màn hình cảm ơn kèm mã đơn, và form bị reset (không gửi lại được bằng F5).

### Feedback

- **AC-13** — *Given* khách gửi feedback 5 sao kèm tên + nội dung, *When* POST `/api/feedbacks`, *Then* trả 201 và feedback lưu với `isApproved = false`.
- **AC-14** — *Given* có feedback chưa duyệt, *When* GET `/api/public/landing`, *Then* feedback đó **không** xuất hiện và **không** tính vào điểm trung bình.
- **AC-15** — *Given* rating gửi lên là 0 hoặc 6, *When* POST `/api/feedbacks`, *Then* trả 400.
- **AC-16** — *Given* nội dung feedback chứa thẻ script, *When* hiển thị trên landing, *Then* chuỗi được render dưới dạng text thuần, **không** thực thi script.

### Auth và phân quyền (high-risk)

- **AC-17** — *Given* tài khoản admin đúng mật khẩu, *When* POST `/api/auth/login`, *Then* trả 200 kèm JWT có claim `role`.
- **AC-18** — *Given* sai mật khẩu, *When* login, *Then* trả **401** với message chung chung ("Email hoặc mật khẩu không đúng") — **không** tiết lộ email có tồn tại hay không.
- **AC-19** — *Given* request không có token, *When* GET `/api/admin/orders`, *Then* trả **401**.
- **AC-20** — *Given* token của role `Staff`, *When* PUT `/api/admin/book`, *Then* trả **403**.
- **AC-21** — *Given* token đã hết hạn, *When* gọi API admin bất kỳ, *Then* trả 401 và FE tự đẩy về `/admin/login`.
- **AC-22** — *Given* DB chưa có user nào, *When* chạy seed, *Then* tạo 1 admin mặc định với mật khẩu **đã hash BCrypt** (không lưu plaintext trong DB và không log ra console).

### Admin CRUD và đơn hàng

- **AC-23** — *Given* admin sửa title sách và lưu, *When* reload landing page, *Then* title mới hiển thị.
- **AC-24** — *Given* danh sách đơn có 25 bản ghi, *When* GET `/api/admin/orders?page=1&pageSize=10`, *Then* trả 10 item + `totalCount = 25`.
- **AC-25** — *Given* lọc `status=New` và `phone=0901234567`, *When* gọi API, *Then* chỉ trả đơn thoả **cả hai** điều kiện.
- **AC-26** — *Given* đơn ở trạng thái `New`, *When* PATCH sang `Confirmed`, *Then* trả 200 và lưu `updatedAt`.
- **AC-27** — *Given* đơn ở trạng thái `Completed`, *When* PATCH sang `New`, *Then* trả **400** (vi phạm state machine).
- **AC-28** — *Given* admin duyệt 1 feedback, *When* reload landing, *Then* feedback hiện ra và điểm trung bình được tính lại.
- **AC-29** — *Given* upload file `.exe` đổi tên thành `.jpg`, *When* POST `/api/admin/upload`, *Then* trả **400** (magic bytes không khớp).
- **AC-30** — *Given* admin sửa hotline trong Setting, *When* reload landing, *Then* Footer hiển thị hotline mới.

## 6. Giả định (assumptions — cần người xác nhận)

1. **Chỉ bán 1 đầu sách.** Schema vẫn cho phép nhiều `Book` nhưng UI landing chỉ render bản ghi `IsActive = true` đầu tiên.
2. **Khách không cần đăng ký tài khoản** — đặt hàng dạng guest checkout.
3. **Không tích hợp cổng thanh toán thật.** `BankTransfer` chỉ hiển thị thông tin chuyển khoản tĩnh; không có webhook, không xác nhận tiền tự động.
4. **Không gửi email/SMS** xác nhận đơn (nếu cần sẽ là scope mở rộng).
5. Ảnh và file review lưu ở **local `wwwroot/uploads`**, chưa dùng cloud storage.
6. Chỉ hỗ trợ **tiếng Việt**, chưa đa ngôn ngữ.

> **Human checkpoint:** người dùng đã chọn "tiến hành làm đi" (2026-10-04) → 6 giả định trên được coi là **đã chấp nhận**.
> Nếu đề bài thực tế yêu cầu cổng thanh toán thật (giả định 3) thì phải quay lại bước 1 và mở thêm một work package riêng.

## 7. Ngoài phạm vi (out of scope)

Giỏ hàng nhiều sản phẩm · tài khoản khách hàng · thanh toán online thật · vận chuyển/tracking · mã giảm giá · đa ngôn ngữ · analytics dashboard.
