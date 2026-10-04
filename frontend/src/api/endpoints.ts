import { api } from './client'
import type {
  AdminBook, AdminFeedback, AdminOrder, AdminUser, CreateFeedbackRequest, CreateFeedbackResponse,
  CreateOrderRequest, CreateOrderResponse, CurrentUser, LandingResponse, LoginResponse,
  OrderStatusValue, PagedResult, SettingItem,
} from './types'

export const publicApi = {
  getLanding: () => api.get<LandingResponse>('/api/public/landing').then((r) => r.data),

  createOrder: (payload: CreateOrderRequest) =>
    api.post<CreateOrderResponse>('/api/orders', payload).then((r) => r.data),

  createFeedback: (payload: CreateFeedbackRequest) =>
    api.post<CreateFeedbackResponse>('/api/feedbacks', payload).then((r) => r.data),
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<LoginResponse>('/api/auth/login', { email, password }).then((r) => r.data),

  me: () => api.get<CurrentUser>('/api/auth/me').then((r) => r.data),
}

export interface OrderFilter {
  status?: OrderStatusValue
  phone?: string
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export const adminApi = {
  getBook: () => api.get<AdminBook>('/api/admin/book').then((r) => r.data),
  updateBook: (payload: Omit<AdminBook, 'id' | 'updatedAt'>) =>
    api.put<AdminBook>('/api/admin/book', payload).then((r) => r.data),

  getOrders: (filter: OrderFilter) =>
    api.get<PagedResult<AdminOrder>>('/api/admin/orders', { params: filter }).then((r) => r.data),
  updateOrderStatus: (id: number, status: OrderStatusValue) =>
    api.patch<AdminOrder>(`/api/admin/orders/${id}/status`, { status }).then((r) => r.data),

  getFeedbacks: (isApproved?: boolean, page = 1, pageSize = 20) =>
    api.get<PagedResult<AdminFeedback>>('/api/admin/feedbacks', { params: { isApproved, page, pageSize } })
      .then((r) => r.data),
  approveFeedback: (id: number) => api.patch(`/api/admin/feedbacks/${id}/approve`).then((r) => r.data),
  hideFeedback: (id: number) => api.patch(`/api/admin/feedbacks/${id}/hide`).then((r) => r.data),
  deleteFeedback: (id: number) => api.delete(`/api/admin/feedbacks/${id}`).then((r) => r.data),

  getUsers: () => api.get<AdminUser[]>('/api/admin/users').then((r) => r.data),

  getSettings: () => api.get<SettingItem[]>('/api/admin/settings').then((r) => r.data),
  updateSettings: (items: SettingItem[]) =>
    api.put<SettingItem[]>('/api/admin/settings', { items }).then((r) => r.data),
}
