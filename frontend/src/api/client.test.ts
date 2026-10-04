import { AxiosError, AxiosHeaders } from 'axios'
import { beforeEach, describe, expect, it, vi } from 'vitest'

// Phai mock truoc khi import client vi interceptor dung window.location.assign.
const assign = vi.fn()
Object.defineProperty(window, 'location', {
  value: { assign, pathname: '/admin/orders' },
  writable: true,
})

const { api, getErrorMessage, TOKEN_STORAGE_KEY } = await import('./client')

function axiosErrorWith(status: number, url: string, data: unknown = {}) {
  const headers = new AxiosHeaders()
  const config = { url, headers }
  return new AxiosError('error', String(status), config, {}, {
    status,
    statusText: '',
    data,
    headers,
    config,
  })
}

type RejectedHandler = (error: AxiosError) => Promise<never>

/** Chay thang handler loi da dang ky tren axios instance. */
async function runResponseErrorInterceptor(error: AxiosError) {
  const { handlers } = api.interceptors.response as unknown as {
    handlers: Array<{ rejected?: RejectedHandler }>
  }
  const rejected = handlers[0]?.rejected
  if (!rejected) throw new Error('Chua dang ky response interceptor')

  await expect(rejected(error)).rejects.toBe(error)
}

describe('interceptor 401', () => {
  beforeEach(() => {
    assign.mockClear()
    localStorage.setItem(TOKEN_STORAGE_KEY, 'token-cu')
    window.location.pathname = '/admin/orders'
  })

  // AC-21
  it('xoá token và chuyển về trang đăng nhập khi API admin trả 401', async () => {
    await runResponseErrorInterceptor(axiosErrorWith(401, '/api/admin/orders'))

    expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
    expect(assign).toHaveBeenCalledWith('/admin/login')
  })

  it('không chuyển hướng khi API công khai trả 401', async () => {
    await runResponseErrorInterceptor(axiosErrorWith(401, '/api/public/landing'))

    expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBe('token-cu')
    expect(assign).not.toHaveBeenCalled()
  })

  it('không chuyển hướng lặp khi đang ở trang đăng nhập', async () => {
    window.location.pathname = '/admin/login'

    await runResponseErrorInterceptor(axiosErrorWith(401, '/api/admin/orders'))

    expect(assign).not.toHaveBeenCalled()
  })

  // Loi that: khach tung dang nhap admin, mo landing sau khi token het han,
  // bi loi ra /admin/login ngay giua trang ban hang.
  it('KHÔNG kéo khách khỏi landing page khi token admin cũ hết hạn', async () => {
    window.location.pathname = '/'

    await runResponseErrorInterceptor(axiosErrorWith(401, '/api/auth/me'))

    expect(assign).not.toHaveBeenCalled()
    // Token hong van phai bi don di.
    expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
  })

  it('vẫn chuyển hướng khi đang ở trang admin khác', async () => {
    window.location.pathname = '/admin/feedbacks'

    await runResponseErrorInterceptor(axiosErrorWith(401, '/api/auth/me'))

    expect(assign).toHaveBeenCalledWith('/admin/login')
  })
})

describe('getErrorMessage', () => {
  it('lấy lỗi field đầu tiên từ ProblemDetails', () => {
    const error = axiosErrorWith(400, '/api/orders', {
      errors: { Phone: ['Số điện thoại không hợp lệ'] },
    })

    expect(getErrorMessage(error)).toBe('Số điện thoại không hợp lệ')
  })

  it('có thông báo riêng cho 429', () => {
    expect(getErrorMessage(axiosErrorWith(429, '/api/orders'))).toContain('quá nhanh')
  })

  it('trả fallback khi lỗi không phải từ axios', () => {
    expect(getErrorMessage(new Error('boom'), 'Lỗi mặc định')).toBe('Lỗi mặc định')
  })
})
