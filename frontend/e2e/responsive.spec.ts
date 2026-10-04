import { expect, test } from '@playwright/test'

// AC-6
test('landing không có horizontal scroll và CTA bấm được', async ({ page }, testInfo) => {
  await page.goto('/')
  await expect(page.getByRole('heading', { level: 1 })).toBeVisible()

  const { scrollWidth, clientWidth } = await page.evaluate(() => ({
    scrollWidth: document.documentElement.scrollWidth,
    clientWidth: document.documentElement.clientWidth,
  }))

  // Cho sai so 1px do lam tron cua trinh duyet.
  expect(scrollWidth, `viewport ${testInfo.project.name}`).toBeLessThanOrEqual(clientWidth + 1)

  const cta = page.getByRole('button', { name: 'Đặt mua ngay' })
  await expect(cta).toBeVisible()
  await cta.click()
  await expect(page.getByRole('heading', { name: 'Đặt mua sách' })).toBeInViewport()
})

test('mọi section chính đều hiển thị', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByRole('heading', { name: 'Về cuốn sách' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Chi tiết tác giả' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Báo chí viết về sách' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Độc giả nói gì' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Đặt mua sách' })).toBeVisible()
  await expect(page.getByRole('contentinfo')).toBeVisible()
})
