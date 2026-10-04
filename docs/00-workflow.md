# 00 — Quy trình làm bài (áp dụng Multi-Agent Coding Workflow)

> Nguồn: `multi_agent_coding_workflow.pdf`. File này map quy trình đó vào đồ án
> **Landing page bán sách "Nhà Giả Kim" + Trang quản trị** (ASP.NET Core + SQL Server + ReactJS).

## 1. Nguyên tắc bắt buộc (không thương lượng)

- **Người làm không tự chấm bài.** Agent/người viết code không được tự review code của mình.
- **Không tin báo cáo "đã xong".** Chỉ tin raw output: log build, log test, `git diff`, screenshot runtime.
- **Rẻ chạy trước, đắt chạy sau.** `dotnet build` → `dotnet test` → `npm run test` chạy TRƯỚC khi nhờ LLM review.
- **Mọi vòng lặp có giới hạn:** tối đa 3 vòng, quá thì escalate lên người.
- **Core principle:** *Do not trust the agent's completion report. Trust independently verifiable evidence.*

## 2. Phân tầng cho đồ án này

| Hạng mục | Tầng | Quy trình áp dụng |
|---|---|---|
| Sửa text/CSS/màu sắc, đổi copy hero | **Trivial** | Implementation → Automated Gates → Diff check |
| Section landing page, CRUD admin, filter đơn hàng, feedback | **Standard** | Đầy đủ bước 1–10 |
| Login/JWT + phân quyền Role, API tạo đơn hàng (public write), upload file, migration DB | **High-risk** | Đầy đủ + human sign-off + security review + model thứ hai ở bước Challenge |

## 3. Mười bước, bản rút gọn cho đồ án

| # | Bước | Ai làm | Output |
|---|---|---|---|
| 1 | Requirement Analysis | Opus | `docs/01-spec.md` |
| 2 | Test Design | Agent riêng, **không** phải người implement | `tests/**`, `docs/02-test-design.md`, `baseline.md` |
| 3 | Planning | Opus | `docs/03-plan.md` |
| 4 | Plan Review | Opus, context sạch (chỉ đọc spec + plan) | `docs/plan-review.md` |
| 5 | Implementation | Sonnet | code trên branch/worktree riêng |
| 6 | Automated Gates | **Script, không dùng LLM** | `gate-report.txt` (raw) |
| 7 | Code Review | Opus, context sạch (chỉ spec + plan + diff) | `docs/code-review.md` |
| 8 | Challenge | Reviewer độc lập, chủ động phá | `docs/challenge.md` |
| 9 | Independent Verification | Agent chỉ đọc, tự chạy lại mọi thứ | `docs/evidence.md` + raw logs |
| 10 | Final Decision | Checklist xác định | quyết định merge / quay lại bước 5 |

## 4. Automated Gates (bước 6) — script cụ thể

```bash
# gates.sh — KHÔNG có LLM trong file này
set -e
dotnet build  BaiTapLTW.sln -warnaserror
dotnet format --verify-no-changes
dotnet test   BaiTapLTW.sln --logger "trx;LogFileName=test.trx"
npm --prefix frontend run lint
npm --prefix frontend run typecheck
npm --prefix frontend run test -- --run
npm --prefix frontend run build
git diff --name-only origin/main...HEAD   # đối chiếu với file list trong plan.md
git diff origin/main...HEAD -- '**/*Tests*' '**/*.test.*'  # soi test bị xoá/skip/nới assertion
```

Fail → quay lại bước 5. Fail 3 vòng → escalate.

### Hai lớp kiểm chứng KHÔNG nằm trong gates (vì cần server đang chạy)

```bash
bash scripts/smoke.sh                  # 18 kiem chung tren API that + SQL Server that
npm --prefix frontend run test:e2e     # 12 test Playwright tren trinh duyet that
```

**Bắt buộc chạy `smoke.sh` trước khi tuyên bố Done.** Test tích hợp chạy SQLite in-memory cho
nhanh, nên mọi khác biệt giữa SQLite và SQL Server đều lọt lưới — đã từng lọt một lỗi 500 thật
(xem `docs/05-review-findings.md`, mục cuối).

## 5. Definition of Done của đồ án

Task chỉ Done khi có **đủ bằng chứng**:

- [ ] Mọi AC trong `01-spec.md` có ít nhất 1 dòng trong bảng truy vết, và test đó **pass**.
- [ ] Không còn finding mức **blocker/major** từ bước 7 và 8.
- [ ] `dotnet build`, `dotnet test`, `npm run build`, lint, typecheck — pass, **có raw output kèm theo**.
- [ ] Không có test nào bị xoá / `[Skip]` / `it.skip` / nới assertion so với baseline.
- [ ] Runtime đúng yêu cầu: có screenshot hoặc log request/response thật (không phải mô tả bằng lời).
- [ ] `git diff` chỉ chứa file nằm trong plan, hoặc có giải trình.
- [ ] Phần high-risk (auth, order, migration): có người duyệt + rollback plan đã kiểm tra.
- [ ] `scripts/smoke.sh` pass trên SQL Server thật (không chỉ SQLite của test tích hợp).
- [ ] Đã chạy bước 7 (Code Review) và bước 8 (Challenge) bởi người/agent **khác** người viết code,
      và mọi finding blocker/major đã xử lý hoặc có lý do ghi lại.

## 6. Chạy song song (phần 7 của PDF)

Đồ án này **đủ lớn để song song** vì chia được theo ranh giới file rõ ràng (backend / frontend public / frontend admin / tests).

- Quyết định WP + ownership file: xem `docs/04-work-packages.md`.
- Contract đóng băng: `contracts/openapi.json` — sau khi duyệt ở P2, **muốn đổi phải quay lại P1**.
- Quy tắc chống xung đột: mỗi file thuộc đúng **một** WP; file dùng chung (`.sln`, `Program.cs`, `router.tsx`, migration) chỉ **Integrator** được sửa.
- Tối đa 3 agent chạy đồng thời; WP dư xếp hàng đợi.
