# 02 — Test Design (Bước 2)

> **Quy tắc:** bước này do **agent/người KHÁC** với người implement làm, và làm **TRƯỚC khi có code**.
> Output: test files + bảng truy vết + baseline.

## 1. Tầng test

| Tầng | Công cụ | Phạm vi |
|---|---|---|
| Unit (BE) | xUnit + FluentAssertions | validator, state machine đơn hàng, sinh mã đơn, tính giá, tính rating trung bình |
| Integration (BE) | xUnit + `WebApplicationFactory` + SQL Server test container (fallback: SQLite in-memory) | toàn bộ endpoint, auth, phân quyền, rate limit |
| Unit/Component (FE) | Vitest + React Testing Library + MSW | render section, validate form, state loading/error |
| E2E | Playwright | 3 luồng tới hạn: đặt hàng, gửi + duyệt feedback, admin login và đổi trạng thái đơn |

## 2. Bảng truy vết AC → Test

> Mỗi dòng phải được điền **bằng chứng thực tế** ở bước 9. Dòng nào thiếu bằng chứng = **chưa Done**.

| AC | Test case | Tầng | File test | Bằng chứng (điền ở bước 9) |
|---|---|---|---|---|
| AC-1 | `GetLanding_ReturnsFullAggregate` | Integration | `PublicLandingTests.cs` | |
| AC-2 | `Hero renders title/subtitle/price` | FE unit | `HeroSection.test.tsx` | |
| AC-3 | `Hero hides strikethrough when no discount` | FE unit | `HeroSection.test.tsx` | |
| AC-4 | `Press section renders N quotes with safe links` | FE unit | `PressSection.test.tsx` | |
| AC-5 | `Review download button visibility` | FE unit | `ReviewSection.test.tsx` | |
| AC-6 | `Landing has no horizontal scroll at 360px` | E2E | `responsive.spec.ts` | |
| AC-7 | `CreateOrder_ValidPayload_Returns201WithCode` | Integration | `OrderTests.cs` | |
| AC-8 | `CreateOrder_InvalidPhone_Returns400_NoDbWrite` | Integration | `OrderTests.cs` | |
| AC-9 | `CreateOrder_QuantityBoundaries` (0,1,99,100) | Unit | `OrderValidatorTests.cs` | |
| AC-10 | `CreateOrder_IgnoresClientSuppliedTotal` | Integration | `OrderTests.cs` | |
| AC-11 | `CreateOrder_RateLimited_Returns429` | Integration | `RateLimitTests.cs` | |
| AC-12 | `Order form shows thank-you and resets` | E2E | `order.spec.ts` | |
| AC-13 | `CreateFeedback_DefaultsToUnapproved` | Integration | `FeedbackTests.cs` | |
| AC-14 | `Landing excludes unapproved feedback from list and average` | Integration | `FeedbackTests.cs` | |
| AC-15 | `FeedbackValidator_RatingOutOfRange` (0,1,5,6) | Unit | `FeedbackValidatorTests.cs` | |
| AC-16 | `Feedback content is escaped, not executed` | FE unit | `FeedbackList.test.tsx` | |
| AC-17 | `Login_ValidCredentials_ReturnsJwtWithRoleClaim` | Integration | `AuthTests.cs` | |
| AC-18 | `Login_WrongPassword_Returns401_GenericMessage` | Integration | `AuthTests.cs` | |
| AC-19 | `AdminEndpoint_NoToken_Returns401` | Integration | `AuthorizationTests.cs` | |
| AC-20 | `StaffToken_UpdateBook_Returns403` | Integration | `AuthorizationTests.cs` | |
| AC-21 | `Expired token redirects to /admin/login` | FE unit | `axiosInterceptor.test.ts` | |
| AC-22 | `Seed_CreatesAdmin_WithBcryptHash` | Integration | `SeedTests.cs` | |
| AC-23 | `UpdateBook_ReflectsOnLanding` | Integration | `AdminBookTests.cs` | |
| AC-24 | `GetOrders_Pagination` | Integration | `AdminOrderTests.cs` | |
| AC-25 | `GetOrders_FilterByStatusAndPhone` | Integration | `AdminOrderTests.cs` | |
| AC-26 | `UpdateStatus_ValidTransition` | Unit | `OrderStateMachineTests.cs` | |
| AC-27 | `UpdateStatus_InvalidTransition_Returns400` | Unit + Integration | `OrderStateMachineTests.cs` | |
| AC-28 | `ApproveFeedback_RecalculatesAverage` | Integration | `AdminFeedbackTests.cs` | |
| AC-29 | `Upload_MismatchedMagicBytes_Returns400` | Integration | `UploadTests.cs` | |
| AC-30 | `UpdateSetting_ReflectsInFooter` | Integration | `AdminSettingTests.cs` | |

## 3. Edge case bắt buộc phải có test

- Giá trị biên số lượng: 0 / 1 / 99 / 100.
- Rating: 0 / 1 / 5 / 6.
- SĐT: `0901234567` (pass), `901234567`, `+84901234567`, `0901234ABC` (fail).
- Landing khi DB **rỗng** (chưa có sách) — phải trả 404 hoặc empty state, **không** 500.
- Feedback có nội dung rất dài (2000+ ký tự) — phải bị chặn ở validator.
- Hai request đặt hàng cùng lúc — mã đơn phải **không trùng**.
- Token bị sửa chữ ký (tampered JWT) — trả 401.

## 4. Baseline

Trước khi implement, chạy và lưu lại:

```bash
dotnet test  > docs/baseline-backend.txt  2>&1
npm --prefix frontend run test -- --run > docs/baseline-frontend.txt 2>&1
```

Ghi rõ test nào **đã fail sẵn** (expected-fail do chưa có code). Bước 6 so sánh với baseline này; test mới fail ngoài danh sách = gate đỏ.
