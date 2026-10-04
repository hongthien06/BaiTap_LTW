# Bài tập LTW — Landing page bán sách "Nhà Giả Kim"

Website bán một đầu sách: landing page công khai + trang quản trị.

**Stack:** ASP.NET Core 8 Web API (C#) · SQL Server + EF Core 8 · ReactJS 18 + Vite + TypeScript

## Tài liệu (đọc theo thứ tự)

| File | Nội dung | Bước trong quy trình |
|---|---|---|
| [docs/00-workflow.md](docs/00-workflow.md) | Quy trình làm bài, phân tầng, automated gates, Definition of Done | Khung chung |
| [docs/01-spec.md](docs/01-spec.md) | Yêu cầu chức năng/phi chức năng + **30 Acceptance Criteria** | Bước 1 |
| [docs/02-test-design.md](docs/02-test-design.md) | Test case + bảng truy vết AC → test + baseline | Bước 2 |
| [docs/03-plan.md](docs/03-plan.md) | Kiến trúc, cấu trúc thư mục, schema DB, API, 7 giai đoạn, rủi ro | Bước 3 |
| [docs/04-work-packages.md](docs/04-work-packages.md) | Chia work package, DAG, ownership file, chống xung đột | P1–P2 (song song) |

Quy trình gốc: `multi_agent_coding_workflow.pdf`

## Nguyên tắc cốt lõi

> Không tin báo cáo "đã xong". Chỉ tin bằng chứng kiểm chứng độc lập được.

## Việc tiếp theo

1. Xác nhận 6 giả định ở mục 6 của [docs/01-spec.md](docs/01-spec.md) — **human checkpoint**.
2. Chốt `contracts/openapi.yaml` (P2 — contract freeze).
3. Khởi tạo G0: solution, project FE, Docker SQL Server, `scripts/gates.sh`.
