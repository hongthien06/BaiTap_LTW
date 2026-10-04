import { expect, test } from '@playwright/test'

const ADMIN_EMAIL = process.env.E2E_ADMIN_EMAIL ?? 'admin@nhagiakim.local'
const ADMIN_PASSWORD = process.env.E2E_ADMIN_PASSWORD ?? 'Admin@12345'

test.describe('Trang quản trị', () => {

  async function login(page: import('@playwright/test').Page) {
    await page.goto('/admin/login')
    await page.getByLabel('Email').fill(ADMIN_EMAIL)
    await page.getByLabel('Mật khẩu').fill(ADMIN_PASSWORD)
    await page.getByRole('button', { name: 'Đăng nhập' }).click()
    await expect(page.getByRole('heading', { name: 'Đơn đặt hàng' })).toBeVisible()
  }

  // AC-19 phia nguoi dung
  test('chưa đăng nhập thì bị đẩy về trang login', async ({ page }) => {
    await page.goto('/admin/orders')
    await expect(page).toHaveURL(/\/admin\/login/)
  })

  // AC-18 phia nguoi dung
  test('sai mật khẩu hiện lỗi chung chung', async ({ page }) => {
    await page.goto('/admin/login')
    await page.getByLabel('Email').fill(ADMIN_EMAIL)
    await page.getByLabel('Mật khẩu').fill('sai-mat-khau')
    await page.getByRole('button', { name: 'Đăng nhập' }).click()

    await expect(page.getByText('Email hoac mat khau khong dung.')).toBeVisible()
    await expect(page).toHaveURL(/\/admin\/login/)
  })

  // AC-26 + AC-27 phia nguoi dung: chi hien buoc chuyen hop le
  test('đổi trạng thái đơn chỉ theo các bước hợp lệ', async ({ page }) => {
    await login(page)

    // Ant Design chen mot <tr> do kich thuoc an o dau tbody -> phai loc theo .ant-table-row.
    const firstRow = page.locator('tbody tr.ant-table-row').first()
    await expect(firstRow).toBeVisible()

    // Don moi -> chi duoc phep "Da xac nhan" hoac "Da huy", khong co "Hoan thanh".
    const confirm = firstRow.getByRole('button', { name: 'Đã xác nhận' })
    if (await confirm.isVisible()) {
      await expect(firstRow.getByRole('button', { name: 'Hoàn thành' })).toHaveCount(0)
      await confirm.click()
      await expect(page.getByText('Đã cập nhật trạng thái đơn.')).toBeVisible()
    }
  })

  // AC-21 phia nguoi dung
  test('token hỏng thì bị đẩy về trang login', async ({ page }) => {
    await login(page)

    await page.evaluate(() => localStorage.setItem('ngk.admin.token', 'token-rac'))
    await page.goto('/admin/orders')

    await expect(page).toHaveURL(/\/admin\/login/)
  })

  // Cac trang quan tri noi dung va tai khoan phai render duoc, khong trang trang.
  test('trang Nội dung landing mở được cả 3 tab', async ({ page }) => {
    await login(page)
    await page.getByRole('link', { name: 'Nội dung landing' }).click()

    await expect(page.getByRole('heading', { name: 'Nội dung landing page' })).toBeVisible()
    await expect(page.getByLabel('Họ tên')).toBeVisible()           // tab Tác giả

    await page.getByRole('tab', { name: 'Báo chí' }).click()
    await expect(page.getByRole('button', { name: 'Thêm trích dẫn' })).toBeVisible()

    await page.getByRole('tab', { name: 'Review nội dung' }).click()
    await expect(page.getByLabel('Tiêu đề')).toBeVisible()
  })

  test('trang Tài khoản mở được form thêm tài khoản', async ({ page }) => {
    await login(page)
    await page.getByRole('link', { name: 'Tài khoản' }).click()

    await expect(page.getByRole('heading', { name: 'Tài khoản quản trị' })).toBeVisible()
    await expect(page.getByRole('cell', { name: 'admin@nhagiakim.local' })).toBeVisible()

    await page.getByRole('button', { name: 'Thêm tài khoản' }).click()
    await expect(page.getByRole('dialog')).toBeVisible()
    await expect(page.getByRole('dialog').getByLabel('Email')).toBeEditable()
  })
})
