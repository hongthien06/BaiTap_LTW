-- Don du lieu rac do kiem thu sinh ra (smoke test, E2E, buoc Challenge).
-- KHONG chay tren database that co don hang cua khach.
--
-- Chay:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -d NhaGiaKim -i scripts/clean-test-data.sql
--
-- Sau khi chay, nho xoa file trong backend/NhaGiaKim.Api/wwwroot/uploads/ neu muon sach hoan toan.

SET NOCOUNT ON;

PRINT '--- Truoc khi don ---';
SELECT 'Orders' AS Bang, COUNT(*) AS SoDong FROM Orders
UNION ALL SELECT 'Feedbacks', COUNT(*) FROM Feedbacks
UNION ALL SELECT 'AppUsers', COUNT(*) FROM AppUsers;

-- Don hang do test tao: cac so dien thoai dung trong smoke test, E2E va Challenge.
DELETE FROM Orders
WHERE Phone IN (
    '0977000111',  -- E2E Playwright
    '0966000111',  -- scripts/smoke.sh
    '0911222333',  -- kiem chung end-to-end qua Vite proxy
    '0909999999',  -- kiem chung rate limit
    '0901234567',  -- kiem chung thu cong
    '0912345678',
    '0988888888'
);

-- Danh gia do test gui.
DELETE FROM Feedbacks
WHERE CustomerName IN ('Tran Thi B', 'Hacker', 'XSSer', 'Smoke Test', 'Khach')
   OR CustomerName LIKE 'RL%';

-- Tai khoan do test tao. Giu lai admin mac dinh.
DELETE FROM AppUsers
WHERE Email <> 'admin@nhagiakim.local'
  AND (Email LIKE 'smoke-%'
    OR Email LIKE 'revoke-%'
    OR Email LIKE 'admin2-%'
    OR Email IN (
        'staff@nhagiakim.local',
        'staff_chal@nhagiakim.local',
        'verify@nhagiakim.local',
        'recheck@nhagiakim.local'
    ));

PRINT '--- Sau khi don ---';
SELECT 'Orders' AS Bang, COUNT(*) AS SoDong FROM Orders
UNION ALL SELECT 'Feedbacks', COUNT(*) FROM Feedbacks
UNION ALL SELECT 'AppUsers', COUNT(*) FROM AppUsers;

-- Kiem tra bat bien: phai con it nhat mot Admin dang hoat dong.
IF NOT EXISTS (
    SELECT 1 FROM AppUsers u
    JOIN Roles r ON r.Id = u.RoleId
    WHERE u.IsActive = 1 AND r.Name = 'Admin')
BEGIN
    RAISERROR('CANH BAO: khong con tai khoan Admin dang hoat dong!', 16, 1);
END
