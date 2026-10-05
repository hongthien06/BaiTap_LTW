# 08 — Trace một lần bấm nút, từ đầu tới cuối

Toàn bộ số liệu dưới đây là **bắt thật** từ hệ thống đang chạy ngày 2026-10-05, không phải ví dụ
bịa. Trình duyệt thật bấm nút thật, request thật, câu SQL thật EF Core sinh ra, record thật trong
SQL Server.

Dùng file này để vừa quay vừa đọc: mỗi bước có **cái gì xảy ra** → **ở file nào dòng nào** →
**bằng chứng chiếu lên**.

---

# LUỒNG 1 — Khách bấm "Xác nhận đặt hàng"

Dữ liệu nhập: `Nguyen Van Trace` · `0905777888` · `25 Nguyen Hue, P. Ben Nghe, Q.1, TP.HCM` ·
số lượng `3` · COD.

**Kết quả: 2344 ms sau khi bấm, màn hình cảm ơn hiện mã đơn `NGK-20261005-2VCV`.**
Riêng phần server xử lý chỉ mất **317 ms**.

---

## Bước 1 — Nút được bấm, React bắt sự kiện

**File:** `frontend/src/components/landing/OrderFormSection.tsx`

```tsx
<form onSubmit={handleSubmit((values) => mutation.mutate({ ... }))}>
  ...
  <Button type="submit">Xác nhận đặt hàng</Button>
</form>
```

Nút là `type="submit"` nằm trong `<form>`. `handleSubmit` của **react-hook-form** chặn hành vi
submit mặc định của trình duyệt (không reload trang), gom giá trị các ô thành một object.

> **Nói:** "Em không dùng submit truyền thống kiểu form post rồi load lại trang. React bắt sự kiện,
> giữ nguyên trang, chỉ gửi dữ liệu đi bằng AJAX."

## Bước 2 — Validate ngay tại trình duyệt

**Cùng file, dòng 14–25:**

```ts
const schema = z.object({
  customerName: z.string().trim().min(2, 'Họ tên phải có ít nhất 2 ký tự').max(200, ...),
  phone: z.string().trim().regex(/^0\d{9}$/, 'Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)'),
  address: z.string().trim().min(10, ...).max(500, ...),
  quantity: z.coerce.number().int().min(1, 'Số lượng từ 1 đến 99').max(99, ...),
  paymentMethod: z.coerce.number().refine(...),
  note: z.string().trim().max(1000, ...).optional(),
})
```

Sai thì **dừng tại đây**, hiện lỗi đỏ dưới ô, **không gửi request nào**.

> **Nói:** "Validate ở client chỉ để người dùng biết sai ngay, không phải để bảo mật. Ai mở DevTools
> cũng bỏ qua được. Nguồn chân lý nằm ở server, lát nữa em chứng minh."

## Bước 3 — axios gửi request lên server

**File:** `frontend/src/api/endpoints.ts`
```ts
createOrder: (payload) => api.post<CreateOrderResponse>('/api/orders', payload).then((r) => r.data)
```

**Bắt được trong tab Network — đây là thứ thật sự rời khỏi trình duyệt:**

```http
POST http://localhost:8080/api/orders
content-type: application/json
accept: application/json, text/plain, */*

{"customerName":"Nguyen Van Trace",
 "phone":"0905777888",
 "address":"25 Nguyen Hue, P. Ben Nghe, Q.1, TP.HCM",
 "quantity":3,
 "paymentMethod":0,
 "note":null}
```

> **Chỗ ăn điểm — chỉ vào body và nói:** "Chú ý trong này **không có một chữ nào về giá tiền**.
> Client không được phép nói giá. Nó chỉ nói mua mấy cuốn."

## Bước 4 — Request đi qua chuỗi middleware của server

**File:** `backend/NhaGiaKim.Api/Program.cs` dòng 119–135 — thứ tự rất quan trọng:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();   // bắt mọi lỗi, đổi thành JSON
app.UseSerilogRequestLogging();                     // ghi log
app.UseStaticFiles(...);                            // phục vụ file tĩnh
app.UseCors(CorsPolicies.Frontend);
app.UseRateLimiter();                               // ← chặn spam ở đây
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();                               // ← mới tới controller
```

Request đi lần lượt qua từng lớp. Rate limiter đứng **trước** controller nên đơn thứ 6 trong một
phút bị chặn luôn, không tốn công vào tới nghiệp vụ.

**File:** `Program.cs` dòng 87–97 — giới hạn 5 request/phút, chia theo IP:
```csharp
RateLimitPartition.GetFixedWindowLimiter(
    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
    factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) })
```

## Bước 5 — Validate lần hai, ở server

**File:** `backend/NhaGiaKim.Application/Validators/CreateOrderRequestValidator.cs`

```csharp
RuleFor(x => x.Phone)
    .NotEmpty().WithMessage("Vui long nhap so dien thoai.")
    .Matches(@"^0\d{9}$").WithMessage("So dien thoai khong hop le (10 so, bat dau bang 0).");

RuleFor(x => x.Quantity).InclusiveBetween(1, 99).WithMessage("So luong phai tu 1 den 99.");
```

Đăng ký tự động ở `Program.cs` dòng 34–35, nên nó chạy **trước khi** vào controller. Sai là trả
400 ngay, service không bao giờ nhận dữ liệu rác.

## Bước 6 — Controller nhận

**File:** `backend/NhaGiaKim.Api/Controllers/OrdersController.cs`

```csharp
[Route("api/orders")]                                      // dòng 9
public class OrdersController(IOrderService orderService) : ApiControllerBase
{
    [HttpPost]                                             // dòng 13
    [EnableRateLimiting(RateLimitPolicies.PublicWrite)]     // dòng 14
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        => ToResponse(await orderService.CreateAsync(request, ct), StatusCodes.Status201Created);
}
```

Controller chỉ làm 3 việc: nhận HTTP → gọi service → đổi kết quả thành mã HTTP. **Không có một dòng
nghiệp vụ nào.**

`IOrderService` được tiêm vào qua constructor. Chỗ đăng ký: `backend/NhaGiaKim.Infrastructure/DependencyInjection.cs`
```csharp
services.AddScoped<IOrderService, OrderService>();
```

> **Nói:** "Đây là Dependency Injection. Controller không tự `new OrderService()`, nó khai báo cần
> gì rồi container đưa vào. Nhờ vậy khi test em thay bằng bản giả được."

## Bước 7 — Service xử lý nghiệp vụ

**File:** `backend/NhaGiaKim.Application/Services/OrderService.cs`

### 7a. Đọc sách đang mở bán — dòng 23–27

```csharp
var book = await db.Books.AsNoTracking()
    .Where(b => b.IsActive)
    .OrderBy(b => b.Id)
    .FirstOrDefaultAsync(ct);
```

**SQL thật EF sinh ra:**
```sql
SELECT TOP(1) [b].[Id], [b].[Category], [b].[Price], [b].[DiscountPrice], ...
FROM [Books] AS [b]
WHERE [b].[IsActive] = CAST(1 AS bit)
ORDER BY [b].[Id]
```

### 7b. Tính tiền — dòng 34–35 ⭐ **ĐOẠN QUAN TRỌNG NHẤT**

```csharp
var unitPrice = MoneyCalculator.EffectiveUnitPrice(book.Price, book.DiscountPrice);
var total     = MoneyCalculator.Total(unitPrice, request.Quantity);
```

**File:** `backend/NhaGiaKim.Application/Common/MoneyCalculator.cs`
```csharp
public static decimal EffectiveUnitPrice(decimal price, decimal? discountPrice)
    => discountPrice is > 0 && discountPrice < price ? discountPrice.Value : price;

public static decimal Total(decimal unitPrice, int quantity)
    => decimal.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);
```

Số thật của lần chạy này:

| | Giá trị | Từ đâu |
|---|---|---|
| `book.Price` | 89.000 | đọc từ bảng `Books` |
| `book.DiscountPrice` | 69.000 | đọc từ bảng `Books` |
| `unitPrice` | **69.000** | server tính |
| `request.Quantity` | 3 | **client gửi** |
| `total` | **207.000** | server tính |

> **Nói:** "Client chỉ được nói **số lượng**. Giá do server đọc từ database rồi nhân. Nếu tin giá
> client gửi thì khách sửa JSON trong DevTools là mua giá 1 đồng. Em có test chứng minh, tên là
> `CreateOrder_IgnoresClientSuppliedTotal` trong `PublicApiTests.cs`."

### 7c. Sinh mã đơn — dòng 42

```csharp
OrderCode = OrderCodeGenerator.Generate(now)
```

**File:** `OrderCodeGenerator.cs`
```csharp
private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // bỏ I,O,0,1 cho dễ đọc
suffix[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
return $"NGK-{utcNow:yyyyMMdd}-{new string(suffix)}";
```

Ra `NGK-20261005-2VCV`. Dùng `RandomNumberGenerator` chứ không phải `Random` thường.

### 7d. Đưa vào change tracker rồi lưu — dòng 57, 60

```csharp
db.Orders.Add(order);          // mới chỉ đánh dấu "sẽ thêm", CHƯA chạm database
await db.SaveChangesAsync(ct); // tới đây EF mới sinh SQL và gửi xuống
```

**SQL thật EF sinh ra:**
```sql
INSERT INTO [Orders] ([Address], [BookId], [CreatedAt], [CustomerName], [Note],
                      [OrderCode], [PaymentMethod], [Phone], [Quantity], [Status],
                      [TotalPrice], [UnitPrice], [UpdatedAt])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12);
```

> **Nói:** "Em không viết SQL tay. EF Core dịch từ `Add` + `SaveChangesAsync` ra câu này.
> Hai điểm đáng chú ý: thứ nhất nó dùng **tham số `@p0, @p1`** chứ không nối chuỗi — nhờ vậy
> không dính SQL injection. Thứ hai là `OUTPUT INSERTED.[Id]` — nó lấy luôn Id vừa sinh về
> mà không cần truy vấn lại."

### 7e. Phòng trường hợp trùng mã — dòng 64–72

```csharp
catch (DbUpdateException ex) when (dbErrors.IsUniqueConstraintViolation(ex))
{
    db.Entry(order).State = EntityState.Detached;
}
```

Cột `OrderCode` có unique index. Trùng thì sinh mã khác, thử tối đa 3 lần. Lỗi khác (mất kết nối,
timeout) **không bắt** — để nó thành 500 chứ không nuốt rồi retry vô ích.

## Bước 8 — Record nằm trong database

```sql
SELECT TOP 2 Id, OrderCode, CustomerName, Phone, Quantity, UnitPrice, TotalPrice,
             PaymentMethod, Status, CreatedAt
FROM Orders WHERE CustomerName = N'Nguyen Van Trace' ORDER BY Id DESC;
```

**Kết quả thật:**

| Id | OrderCode | CustomerName | Phone | Quantity | UnitPrice | TotalPrice | PaymentMethod | Status | CreatedAt |
|---|---|---|---|---|---|---|---|---|---|
| 3003 | NGK-20261005-2VCV | Nguyen Van Trace | 0905777888 | 3 | 69000.00 | 207000.00 | 0 | 0 | 2026-10-05 11:17:57 |

> **Nói:** "`UnitPrice` 69.000 nhân `Quantity` 3 bằng `TotalPrice` 207.000 — khớp đúng cái server
> tính ở bước 7b. `PaymentMethod` 0 là COD, `Status` 0 là trạng thái Mới. Em lưu enum dưới dạng số
> cho gọn, ánh xạ khai ở `OrderConfiguration.cs`."

## Bước 9 — Server trả về

```http
HTTP/1.1 201 Created
content-type: application/json

{"orderCode":"NGK-20261005-2VCV","unitPrice":69000.00,"totalPrice":207000.00}
```

Chỗ đổi `ServiceResult` thành mã HTTP: `backend/NhaGiaKim.Api/Controllers/ApiControllerBase.cs`

```csharp
var status = result.ErrorCode switch
{
    ServiceErrorCode.Validation   => StatusCodes.Status400BadRequest,
    ServiceErrorCode.NotFound     => StatusCodes.Status404NotFound,
    ServiceErrorCode.Conflict     => StatusCodes.Status409Conflict,
    ServiceErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
    ...
};
```

> **Nói:** "Service không biết gì về HTTP, nó chỉ trả về thành công hay thất bại kèm loại lỗi.
> Việc đổi sang mã 400/404/409 là của tầng API. Nhờ vậy logic nghiệp vụ test được mà không cần
> dựng web server."

## Bước 10 — Giao diện đổi theo

**File:** `OrderFormSection.tsx`
```tsx
onSuccess: (data) => {
  setResult(data)
  setError(null)
  reset()          // ← xoá sạch form, F5 không gửi lại đơn
}
```

Có `result` thì component render nhánh khác: màn hình cảm ơn kèm mã đơn.

---

# LUỒNG 2 — Admin bấm "Đăng nhập"

## Bước 1 — Gửi đi

```http
POST http://localhost:8080/api/auth/login
content-type: application/json

{"email":"admin@nhagiakim.local","password":"Admin@12345"}
```

## Bước 2 — Service truy vấn người dùng

**File:** `backend/NhaGiaKim.Application/Services/AuthService.cs` dòng 33–35

```csharp
var user = await db.AppUsers
    .Include(u => u.Role)
    .FirstOrDefaultAsync(u => u.Email == email, ct);
```

**SQL thật:**
```sql
SELECT TOP(1) [a].[Id], [a].[CreatedAt], [a].[Email], [a].[FullName], [a].[IsActive],
              [a].[LastLoginAt], [a].[PasswordHash], [a].[RoleId], [r].[Id], [r].[Name]
FROM [AppUsers] AS [a]
INNER JOIN [Roles] AS [r] ON [a].[RoleId] = [r].[Id]
WHERE [a].[Email] = @__email_0
```

> **Nói:** "`.Include(u => u.Role)` được EF dịch thành `INNER JOIN` sang bảng `Roles`. Em lấy luôn
> role trong một câu truy vấn thay vì gọi hai lần."

## Bước 3 — Kiểm mật khẩu

**Dòng 39–45:**
```csharp
var hashToCheck = user is { IsActive: true } ? user.PasswordHash : hasher.DummyHash;
var passwordMatches = hasher.Verify(request.Password, hashToCheck);

if (user is null || !user.IsActive || !passwordMatches)
    return ServiceResult<LoginResponse>.Fail(ServiceErrorCode.Unauthorized, InvalidCredentials);
```

Trong database không có mật khẩu gốc:
```sql
SELECT Id, Email, LEFT(PasswordHash, 30) + '...' AS Hash, IsActive FROM AppUsers;
-- $2a$12$Km9... → định dạng BCrypt, 12 là work factor
```

> **Hai điểm nói kỹ:**
> 1. "BCrypt là hàm một chiều, có salt riêng từng mật khẩu. Hai người đặt mật khẩu giống nhau vẫn
>    ra hash khác nhau. Lộ database cũng không đọc ngược ra mật khẩu."
> 2. "Dòng 39 — kể cả email **không tồn tại**, em vẫn chạy một phép BCrypt với hash giả. Không làm
>    vậy thì email sai trả lời sau 2ms, email đúng mà sai mật khẩu mất 300ms. Kẻ tấn công bấm giờ
>    là biết email nào có thật. Thông báo lỗi cũng để chung chung, không nói sai email hay sai mật khẩu."

## Bước 4 — Sinh JWT

**File:** `backend/NhaGiaKim.Infrastructure/Security/JwtTokenGenerator.cs` dòng 20–37

```csharp
Claim[] claims =
[
    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new(JwtRegisteredClaimNames.Email, user.Email),
    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new(ClaimTypes.Name, user.FullName),
    new(ClaimTypes.Role, roleName),              // ← role nằm đây
];

signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
```

**Payload thật, giải mã từ token vừa nhận:**

```json
{
  "sub": "1",
  "email": "admin@nhagiakim.local",
  "jti": "894b445b-1d05-433a-9cd2-9a984dceb146",
  ".../claims/nameidentifier": "1",
  ".../claims/name": "Quản trị viên",
  ".../claims/role": "Admin",
  "nbf": 1791199079,
  "exp": 1791202679,
  "iss": "NhaGiaKim.Api",
  "aud": "NhaGiaKim.Client"
}
```

> **Nói:** "`exp` trừ `nbf` ra đúng 3600 giây — token sống 60 phút. Quan trọng: JWT chỉ **mã hoá
> base64, không phải mã hoá bí mật** — ai cũng đọc được nội dung này, em vừa giải mã ngay trước mặt
> thầy. Cái bảo vệ nó là **chữ ký HMAC-SHA256**, sửa một ký tự là chữ ký sai, server từ chối.
> Vì ai cũng đọc được nên em không bao giờ bỏ mật khẩu vào token."

## Bước 5 — Trả về và frontend lưu token

```http
HTTP/1.1 200 OK
{"accessToken":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",   (611 ký tự)
 "expiresAtUtc":"2026-10-05T12:17:34Z",
 "user":{"id":1,"email":"admin@nhagiakim.local","fullName":"Quản trị viên","role":"Admin"}}
```

**File:** `frontend/src/hooks/useAuth.tsx` dòng 37–41
```ts
localStorage.setItem(TOKEN_STORAGE_KEY, result.accessToken)
```

## Bước 6 — Mọi request sau tự mang token

**File:** `frontend/src/api/client.ts`
```ts
api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_STORAGE_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})
```

**Bắt được khi vào trang đơn hàng:**
```http
GET http://localhost:8080/api/admin/orders?page=1&pageSize=10
authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e...
```

> **Nói:** "Header này do interceptor tự gắn, em không viết tay ở từng chỗ gọi API."

## Bước 7 — Server kiểm token và role

**File:** `Program.cs` dòng 60–70
```csharp
options.Events = new JwtBearerEvents { OnTokenValidated = ActiveUserValidator.ValidateAsync };
...
.AddPolicy(AuthPolicies.RequireAdmin, p => p.RequireRole(RoleName.Admin))
.AddPolicy(AuthPolicies.RequireStaffOrAdmin, p => p.RequireRole(RoleName.Admin, RoleName.Staff));
```

**File:** `AdminOrdersController.cs` dòng 12
```csharp
[Authorize(Policy = AuthPolicies.RequireStaffOrAdmin)]
```

**Response thật:**
```json
{"items":[{"orderCode":"NGK-20261005-2VCV","customerName":"Nguyen Van Trace","totalPrice":207000,...}],
 "totalCount":8, "page":1, "pageSize":10}
```

> **Nói thêm nếu còn giờ** (`ActiveUserValidator.cs`): "JWT là stateless, phát ra rồi không thu hồi
> được cho tới khi hết hạn. Nghĩa là admin khoá một tài khoản xong người đó vẫn vào được 60 phút.
> Em vá bằng cách mỗi request đã xác thực thì đối chiếu lại `IsActive` và role với database.
> Trả giá là thêm một truy vấn nhỏ mỗi request — với khu quản trị thì đáng."

---

# Cách tự bắt lại các số này khi quay

```bash
# 1. Bật log SQL: mở .env sửa SQL_LOG_LEVEL=Information
docker compose up -d

# 2. Cửa sổ riêng, để chạy suốt lúc quay
docker compose logs -f api

# 3. Xem record trong database
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d NhaGiaKim \
  -Q "SELECT TOP 3 Id, OrderCode, Quantity, UnitPrice, TotalPrice, Status FROM Orders ORDER BY Id DESC"
```

> Git Bash trên Windows thì thêm `MSYS_NO_PATHCONV=1` vào đầu lệnh thứ 3.

# Hai thứ nên chứng minh ngay trên video

**1. Server không tin giá client gửi**
```bash
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" \
  -d "{\"customerName\":\"Thu Gian Lan\",\"phone\":\"0905000111\",\"address\":\"1 Duong Test, Quan 1, TPHCM\",\"quantity\":3,\"paymentMethod\":0,\"totalPrice\":1,\"unitPrice\":1}"
```
Gửi kèm `totalPrice: 1` mà kết quả vẫn ra `207000`.

**2. Sai định dạng là chặn, không tạo record**
```bash
curl -i -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" \
  -d "{\"customerName\":\"Test\",\"phone\":\"901234567\",\"address\":\"1 Duong Test, Quan 1, TPHCM\",\"quantity\":1,\"paymentMethod\":0}"
```
400 kèm `errors.Phone`, đếm lại bảng `Orders` thấy số dòng không đổi.
