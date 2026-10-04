# 04 — Decomposition & Work Packages (P1 của chế độ song song)

> Áp dụng mục 7 của `multi_agent_coding_workflow.pdf`.
> **Quy tắc vàng:** mỗi file thuộc đúng **một** WP. Sửa file ngoài ownership = **fail ngay** ở Automated Gates.

## 1. Danh sách Work Package

### WP-A — Domain & Database

- **Mục tiêu:** entities, `AppDbContext`, configuration, migration đầu tiên, seeder.
- **AC liên quan:** AC-22 (một phần AC-1, AC-24).
- **Sở hữu file:** `backend/NhaGiaKim.Domain/**`, `backend/NhaGiaKim.Infrastructure/**`
- **Phụ thuộc:** không. **Phải xong trước mọi WP backend khác.**

### WP-B — Public API (landing / order / feedback)

- **Mục tiêu:** `PublicController`, `OrdersController`, `FeedbacksController` + service + validator + rate limit.
- **AC:** AC-1, 7, 8, 9, 10, 11, 13, 14, 15.
- **Sở hữu file:** `Controllers/PublicController.cs`, `Controllers/OrdersController.cs`, `Controllers/FeedbacksController.cs`, `Application/Services/{Landing,Order,Feedback}Service.cs`, `Application/Validators/{Order,Feedback}Validator.cs`, `Application/Common/OrderStateMachine.cs`, `Application/Dtos/Public/**`
- **Phụ thuộc:** WP-A.

### WP-C — Auth, Admin API, Upload

- **Mục tiêu:** login JWT, policy role, CRUD admin, upload file.
- **AC:** AC-17–20, 22, 23–30.
- **Sở hữu file:** `Controllers/AuthController.cs`, `Controllers/Admin/**`, `Application/Services/{Auth,Book,Author,Press,Review,User,Setting,Upload}Service.cs`, `Application/Dtos/Admin/**`
- **Phụ thuộc:** WP-A. Chạy **song song** với WP-B.

### WP-D — Frontend Landing

- **Mục tiêu:** 8 section landing + form đặt hàng + form feedback + SEO + responsive.
- **AC:** AC-2, 3, 4, 5, 6, 12, 16.
- **Sở hữu file:** `frontend/src/pages/public/**`, `frontend/src/components/landing/**`, `frontend/src/styles/landing.css`
- **Phụ thuộc:** **contract** (`openapi.yaml`), KHÔNG phụ thuộc code của WP-B. Dev với **MSW mock** theo contract.

### WP-E — Frontend Admin

- **Mục tiêu:** login, dashboard, quản lý đơn/feedback/nội dung/user/setting.
- **AC:** AC-21, 23–30 (phía UI).
- **Sở hữu file:** `frontend/src/pages/admin/**`, `frontend/src/components/admin/**`, `frontend/src/hooks/**`
- **Phụ thuộc:** contract. Song song với WP-D.

### WP-F — Test & CI

- **Mục tiêu:** test skeleton theo `02-test-design.md`, `gates.sh`, GitHub Actions, Playwright E2E.
- **Sở hữu file:** `backend/NhaGiaKim.Tests/**`, `frontend/e2e/**`, `frontend/src/**/*.test.tsx` *(file test, không phải file nguồn)*
- **Phụ thuộc:** contract + `02-test-design.md`. **Bắt đầu sớm nhất có thể** (bước 2 đi trước bước 5).

### Integrator (không phải WP — 1 người/agent duy nhất)

- Sở hữu toàn bộ file dùng chung ở mục 8 của `03-plan.md`.
- Là **người duy nhất** được merge và chạy `dotnet ef migrations add`.

## 2. Đồ thị phụ thuộc (DAG)

```
         contracts/openapi.yaml  (đóng băng ở P2)
                    |
      +-------------+--------------+---------------+
      |             |              |               |
    WP-A          WP-D           WP-E            WP-F
  (domain+db)   (FE landing)   (FE admin)    (tests + CI)
      |
   +--+---+
   |      |
 WP-B   WP-C          <- song song, cùng tách từ 1 base commit
   |      |
   +--+---+
      |
   P4 Integration (Integrator merge theo thứ tự A -> B,C -> D,E -> F)
      |
   P5 Parallel Review & Challenge
      |
   P6 Verification -> Final Decision
```

**Giới hạn:** tối đa **3 agent chạy đồng thời**. Thứ tự ưu tiên (đường găng): **WP-A → WP-C → WP-E**.

## 3. Contract đóng băng (P2)

`contracts/openapi.yaml` phải định nghĩa xong **trước khi** bất kỳ WP nào viết code:

- Mọi route, method, status code.
- Schema request/response đầy đủ (bao gồm `ProblemDetails` cho lỗi).
- Enum: `OrderStatus`, `PaymentMethod`, `RoleName`.
- Quy tắc đặt tên JSON: **camelCase**.

Sau khi duyệt ở P2 → **đóng băng**. Muốn đổi contract: dừng mọi WP phụ thuộc → quay lại P1 → cập nhật → duyệt lại P2.

FE sinh type bằng:
```bash
npx openapi-typescript contracts/openapi.yaml -o frontend/src/api/schema.d.ts
```

## 4. Quy tắc chống xung đột

1. **File ownership độc quyền** — script gate kiểm tra: `git diff --name-only` của mỗi branch phải nằm trọn trong danh sách ownership của WP đó.
2. **Contract-first** — WP-D/E gọi API qua type sinh từ contract, **không** đọc code của WP-B/C.
3. **Cùng base commit** — mọi WP tách từ một commit; không rebase giữa chừng trừ khi Integrator yêu cầu.
4. **Một Integrator duy nhất** — chỉ Integrator merge và chỉ Integrator sửa file dùng chung.
5. **Không chia sẻ trạng thái ngầm** — mỗi WP dùng **database riêng** khi chạy test (`NhaGiaKim_Test_WPB`, `NhaGiaKim_Test_WPC`...) và **port riêng**.

## 5. Xử lý sự cố

| Tình huống | Hành động |
|---|---|
| 1 WP fail gates 3 vòng | Escalate lên người. WP độc lập vẫn chạy tiếp; WP phụ thuộc tạm dừng |
| Contract sai hoặc thiếu | Dừng WP phụ thuộc → quay lại P1 → cập nhật → duyệt lại P2 |
| Conflict khi merge do chồng file | Quay lại P1 tách/gộp lại WP. **Không đoán khi giải conflict** |
| Gates fail sau merge dù từng branch đều xanh | Integrator xác định WP gây lỗi, trả lại đúng WP đó kèm log |
| Quá giới hạn agent | Xếp hàng đợi, ưu tiên WP trên đường găng (A → C → E) |

## 6. Definition of Done bổ sung cho chế độ song song

- [ ] Mỗi WP chỉ sửa file trong ownership (có output `git diff --name-only` chứng minh).
- [ ] Contract không đổi sau khi đóng băng, hoặc mọi thay đổi đã qua lại P2.
- [ ] Toàn bộ gates + test tích hợp pass trên **kết quả đã merge**, không chỉ trên từng branch.
- [ ] Bảng truy vết phủ đủ **30 AC của toàn task**, không chỉ AC của từng WP.
- [ ] Không còn conflict, WP dở dang hay branch chưa xử lý.
