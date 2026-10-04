# 02 — Test Design (Bước 2)

> **Quy tắc:** bước này do **agent/người KHÁC** với người implement làm, và làm **TRƯỚC khi có code**.
> Output: test files + bảng truy vết + baseline.

## 1. Tầng test

| Tầng | Công cụ | Phạm vi | Trạng thái |
|---|---|---|---|
| Unit (BE) | xUnit + **Shouldly** | validator, state machine đơn hàng, sinh mã đơn, tính giá, magic bytes | ✅ 65 test |
| Integration (BE) | xUnit + `WebApplicationFactory` + **SQLite in-memory** | toàn bộ endpoint, auth, phân quyền, rate limit | ✅ 27 test |
| Unit/Component (FE) | Vitest + React Testing Library | render section, hiển thị giá, escape nội dung, interceptor 401 | ✅ 18 test |
| E2E | Playwright | 3 luồng tới hạn: đặt hàng, gửi + duyệt feedback, admin đổi trạng thái | ⬜ chưa làm |

> Dùng **Shouldly** thay cho FluentAssertions: FluentAssertions 8 đổi sang giấy phép thương mại,
> không phù hợp cho bài tập. Shouldly là BSD, cú pháp tương đương.

## 2. Bảng truy vết AC → Test → Bằng chứng

> Dòng nào chưa có bằng chứng = **chưa Done**.

| AC | Test case | Tầng | File test | Bằng chứng |
|---|---|---|---|---|
| AC-1 | `GetLanding_ReturnsFullAggregate` | Integration | `Integration/PublicApiTests.cs` | ✅ pass |
| AC-2 | `hiển thị title, subtitle, ảnh mockup và giá` | FE unit | `components/landing/HeroSection.test.tsx` | ✅ pass |
| AC-2 | `gạch ngang giá gốc khi có giá giảm` | FE unit | `HeroSection.test.tsx` | ✅ pass |
| AC-3 | `không hiện giá gạch ngang và badge khi không có giá giảm` | FE unit | `HeroSection.test.tsx` | ✅ pass |
| AC-3 | `bỏ qua giá giảm lớn hơn hoặc bằng giá gốc` | FE unit | `HeroSection.test.tsx` | ✅ pass |
| AC-4 | `render đúng số trích dẫn và link mở tab mới an toàn` | FE unit | `PressAndReview.test.tsx` | ✅ pass |
| AC-5 | `hiện nút tải khi có fileUrl` / `ẩn nút tải khi fileUrl là null` | FE unit | `PressAndReview.test.tsx` | ✅ pass |
| AC-6 | Responsive 360px, không horizontal scroll | E2E | `e2e/responsive.spec.ts` | ⬜ **chưa có** |
| AC-7 | `CreateOrder_ValidPayload_Returns201WithCode` | Integration | `PublicApiTests.cs` | ✅ pass |
| AC-8 | `CreateOrder_InvalidPhone_Returns400` | Integration | `PublicApiTests.cs` | ✅ pass |
| AC-8 | `Phone_FormatRules` (6 biến thể) | Unit | `Unit/ValidatorTests.cs` | ✅ pass |
| AC-9 | `Quantity_BoundaryValues` (0/1/99/100/-1) | Unit + Integration | `ValidatorTests.cs`, `PublicApiTests.cs` | ✅ pass |
| AC-10 | `CreateOrder_IgnoresClientSuppliedTotal` | Integration | `PublicApiTests.cs` | ✅ pass |
| AC-10 | `EffectiveUnitPrice_*`, `Total_*` | Unit | `Unit/MoneyCalculatorTests.cs` | ✅ pass |
| AC-11 | `CreateOrder_SixthRequestInWindow_Returns429` | Integration | `PublicApiTests.cs` | ✅ pass |
| AC-12 | Màn hình cảm ơn + reset form | E2E | `e2e/order.spec.ts` | ⬜ **chưa có** (code đã `reset()` sau 201) |
| AC-13 | `CreateFeedback_DefaultsToUnapproved` | Integration | `PublicApiTests.cs` | ✅ pass |
| AC-14 | Cùng test AC-13 (kiểm tra landing không chứa feedback chưa duyệt) | Integration | `PublicApiTests.cs` | ✅ pass |
| AC-15 | `CreateFeedback_RatingOutOfRange_Returns400` (0/6/-1) | Integration | `PublicApiTests.cs` | ✅ pass |
| AC-15 | `Rating_BoundaryValues` (0/1/5/6/-1) | Unit | `ValidatorTests.cs` | ✅ pass |
| AC-16 | `render nội dung chứa thẻ script dưới dạng text thuần` | FE unit | `FeedbackSection.test.tsx` | ✅ pass |
| AC-17 | `Login_ValidCredentials_ReturnsJwtWithRole` | Integration | `Integration/AuthAndAdminTests.cs` | ✅ pass |
| AC-18 | `Login_WrongPasswordAndUnknownEmail_ReturnSameGenericError` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-19 | `AdminEndpoint_WithoutToken_Returns401` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-19 | `AdminEndpoint_WithTamperedToken_Returns401` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-20 | `StaffToken_UpdateBook_Returns403_ButCanReadOrders` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-21 | `xoá token và chuyển về trang đăng nhập khi API admin trả 401` | FE unit | `api/client.test.ts` | ✅ pass |
| AC-22 | `Seed_StoresAdminPasswordAsBcryptHash` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-23 | `UpdateBook_ReflectsOnLanding` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-24 | `GetOrders_Pagination_ReturnsPageSizeAndTotalCount` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-25 | `GetOrders_FilterByStatusAndPhone_AppliesBothConditions` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-26 | `UpdateStatus_FollowsStateMachine` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-26/27 | `CanTransition_*` (18 cặp trạng thái) | Unit | `Unit/OrderStateMachineTests.cs` | ✅ pass |
| AC-28 | `ApproveFeedback_MakesItVisibleAndRecalculatesAverage` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-29 | `Upload_ExecutableDisguisedAsImage_Returns400` | Integration | `AuthAndAdminTests.cs` | ✅ pass |
| AC-29 | `Detect_RejectsExecutableDisguisedAsImage` + 9 case khác | Unit | `Unit/FileSignatureValidatorTests.cs` | ✅ pass |
| AC-30 | `UpdateSetting_ReflectsInLandingSettings` | Integration | `AuthAndAdminTests.cs` | ✅ pass |

**Tổng: 110 test tự động (92 backend + 18 frontend), tất cả pass. Còn 2 AC (AC-6, AC-12) chưa có test tự động.**

## 3. Edge case đã có test

- Số lượng: 0 / 1 / 99 / 100 / -1 ✅
- Rating: 0 / 1 / 5 / 6 / -1 ✅
- SĐT: `0901234567`, `0000000000` (pass); `901234567`, `+84901234567`, `0901234ABC`, `09012345678`, rỗng (fail) ✅
- Giá giảm vô lý (bằng, lớn hơn, bằng 0, âm) → bỏ qua, dùng giá gốc ✅
- Mã đơn: đúng format, không trùng trong 2000 lần sinh, không chứa ký tự dễ nhầm (O/I/0/1) ✅
- Magic bytes: JPEG/PNG/WebP/PDF nhận đúng; EXE giả `.jpg`, PDF khi cần ảnh, header cụt, header rỗng → từ chối ✅
- JWT bị sửa chữ ký → 401 ✅
- Feedback > 2000 ký tự → 400 ✅
- Chuyển trạng thái: lùi, nhảy cóc, tự chuyển về chính nó, rời khỏi `Completed`/`Cancelled` → đều bị chặn ✅

## 4. Edge case còn thiếu test

- Landing khi DB **rỗng hoàn toàn** (chưa có sách) → hiện trả 404, cần test khẳng định không phải 500.
- Hai request đặt hàng **đồng thời** → unique index đã có, chưa có test concurrency.
- Responsive 360px và luồng đặt hàng trên trình duyệt thật (cần Playwright).

## 5. Baseline

Baseline của dự án này là **0 test fail**. Lấy lại bất cứ lúc nào:

```bash
dotnet test BaiTapLTW.sln --nologo          > docs/baseline-backend.txt  2>&1
npm --prefix frontend run test:run          > docs/baseline-frontend.txt 2>&1
```

Bước 6 so sánh với baseline này; **bất kỳ test nào fail hoặc biến mất = gate đỏ**.

## 6. Lưu ý kỹ thuật khi viết integration test

Test tích hợp cấu hình API qua **biến môi trường**, không dùng `ConfigureAppConfiguration`.
Lý do: `Program.cs` đọc `builder.Configuration` ngay khi dựng host (để kiểm tra `Jwt:Key` và rate limit),
thời điểm đó các nguồn config do `WebApplicationFactory` thêm vào **chưa được áp dụng**.
Dùng `ConfigureAppConfiguration` sẽ khiến token được **ký bằng key test** nhưng lại **kiểm tra bằng key
trong user-secrets** → mọi request kèm token đều 401. Vì biến môi trường có phạm vi tiến trình,
test chạy **tuần tự** (`TestCollectionBehavior.cs`).
