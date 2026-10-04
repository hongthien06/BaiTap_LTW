import { expect, test } from '@playwright/test'

test.describe('Luồng đặt hàng', () => {

  // AC-12
  test('đặt hàng thành công hiện mã đơn và reset form', async ({ page }) => {
    await page.goto('/')

    await page.getByRole('button', { name: 'Đặt mua ngay' }).click()

    // Gioi han vao dung section dat hang: nhan "Ho ten" cung xuat hien o form danh gia.
    const orderForm = page.getByRole('region', { name: 'Đặt mua sách' })
    await orderForm.getByLabel('Họ tên').fill('Nguyen Van E2E')
    await orderForm.getByLabel('Số điện thoại').fill('0977000111')
    await orderForm.getByLabel('Địa chỉ nhận hàng').fill('12 Duong Playwright, Quan 1, TPHCM')
    await orderForm.getByLabel('Số lượng').fill('2')

    await orderForm.getByRole('button', { name: 'Xác nhận đặt hàng' }).click()

    const success = page.getByTestId('order-success')
    await expect(success).toBeVisible()
    await expect(page.getByTestId('order-code')).toHaveText(/^NGK-\d{8}-[A-Z2-9]{4}$/)

    // Form da bi reset -> quay lai form thi cac o deu trong, F5 khong gui lai don.
    await page.getByRole('button', { name: 'Đặt thêm đơn khác' }).click()
    const formAgain = page.getByRole('region', { name: 'Đặt mua sách' })
    await expect(formAgain.getByLabel('Họ tên')).toHaveValue('')
    await expect(formAgain.getByLabel('Số điện thoại')).toHaveValue('')
  })

  // AC-8 phia nguoi dung: loi validate hien ngay, khong goi API
  test('số điện thoại sai định dạng bị chặn ngay ở client', async ({ page }) => {
    await page.goto('/')

    const orderForm = page.getByRole('region', { name: 'Đặt mua sách' })
    await orderForm.getByLabel('Họ tên').fill('Nguyen Van E2E')
    await orderForm.getByLabel('Số điện thoại').fill('901234567')
    await orderForm.getByLabel('Địa chỉ nhận hàng').fill('12 Duong Playwright, Quan 1, TPHCM')

    await orderForm.getByRole('button', { name: 'Xác nhận đặt hàng' }).click()

    await expect(page.getByText('Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)')).toBeVisible()
    await expect(page.getByTestId('order-success')).toHaveCount(0)
  })
})
