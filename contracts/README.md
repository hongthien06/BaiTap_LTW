# Contract (P2 — Contract Freeze)

`openapi.json` là **contract đã đóng băng** giữa backend và frontend. File này được **sinh ra từ API đang chạy**, không viết tay:

```bash
# API phải đang chạy ở http://localhost:5080
curl -s http://localhost:5080/swagger/v1/swagger.json -o contracts/openapi.json
```

## Quy tắc

1. **Frontend không đọc code backend.** Mọi kiểu dữ liệu FE dùng phải khớp file này (`frontend/src/api/types.ts`).
2. **Muốn đổi contract phải quay lại P1**, cập nhật `docs/04-work-packages.md`, rồi duyệt lại ở P2. Không sửa lén.
3. Sau mỗi lần đổi API, **sinh lại file này** và commit cùng commit đổi API — để diff của contract nhìn thấy được khi review.

## Sinh type TypeScript từ contract

```bash
npx openapi-typescript contracts/openapi.json -o frontend/src/api/schema.d.ts
```

Hiện tại `frontend/src/api/types.ts` đang viết tay vì cần thêm nhãn tiếng Việt và bảng
`ORDER_NEXT_STATUSES`. Nếu API phình to, nên chuyển sang sinh tự động và chỉ giữ phần nhãn ở file riêng.

## Tổng quan endpoint (21 path)

| Nhóm | Endpoint |
|---|---|
| Công khai | `GET /api/public/landing`, `POST /api/orders`, `POST /api/feedbacks` |
| Auth | `POST /api/auth/login`, `GET /api/auth/me` |
| Admin — nội dung | `/api/admin/book`, `/api/admin/author`, `/api/admin/press-quotes`, `/api/admin/review`, `/api/admin/settings` |
| Admin — vận hành | `/api/admin/orders`, `/api/admin/orders/{id}/status`, `/api/admin/feedbacks`, `/api/admin/users`, `/api/admin/upload` |

Swagger UI khi chạy dev: http://localhost:5080/swagger
