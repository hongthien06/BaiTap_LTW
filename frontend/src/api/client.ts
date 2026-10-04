import axios, { AxiosError } from 'axios'
import { queryClient } from '../lib/queryClient'
import type { ApiProblem } from './types'

export const TOKEN_STORAGE_KEY = 'ngk.admin.token'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '',
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_STORAGE_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

/** Duong dan hien tai co thuoc khu quan tri khong. */
export const isOnAdminPage = () => window.location.pathname.startsWith('/admin')

/**
 * AC-21: token het han hoac khong hop le -> xoa token, xoa cache va day ve man dang nhap.
 *
 * Chi chuyen huong khi NGUOI DUNG DANG O khu admin. Truoc day khong co dieu kien nay:
 * mot khach tung dang nhap admin roi mo landing page sau 60 phut se bi loi ra /admin/login
 * ngay giua trang ban hang.
 */
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const status = error.response?.status
    const url = error.config?.url ?? ''
    const isAdminCall = url.startsWith('/api/admin') || url === '/api/auth/me'

    if (status === 401 && isAdminCall) {
      localStorage.removeItem(TOKEN_STORAGE_KEY)
      queryClient.clear()

      if (isOnAdminPage() && !window.location.pathname.startsWith('/admin/login')) {
        window.location.assign('/admin/login')
      }
    }
    return Promise.reject(error)
  },
)

/** Lay thong bao loi doc duoc tu ProblemDetails, co fallback khi mat mang. */
export function getErrorMessage(error: unknown, fallback = 'Đã có lỗi xảy ra, vui lòng thử lại.'): string {
  if (axios.isAxiosError(error)) {
    const problem = error.response?.data as ApiProblem | undefined
    const firstFieldError = problem?.errors
      ? Object.values(problem.errors).flat().find(Boolean)
      : undefined

    if (error.response?.status === 429) {
      return 'Bạn thao tác quá nhanh. Vui lòng thử lại sau một phút.'
    }
    return firstFieldError ?? problem?.detail ?? problem?.title ?? fallback
  }
  return fallback
}
