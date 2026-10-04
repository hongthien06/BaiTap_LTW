// Kieu du lieu doi chieu 1-1 voi DTO cua backend (contracts/openapi.yaml).
// Doi contract o backend thi phai cap nhat file nay.

export const OrderStatus = {
  New: 0,
  Confirmed: 1,
  Shipping: 2,
  Completed: 3,
  Cancelled: 4,
} as const

export type OrderStatusValue = (typeof OrderStatus)[keyof typeof OrderStatus]

export const ORDER_STATUS_LABEL: Record<OrderStatusValue, string> = {
  [OrderStatus.New]: 'Mới',
  [OrderStatus.Confirmed]: 'Đã xác nhận',
  [OrderStatus.Shipping]: 'Đang giao',
  [OrderStatus.Completed]: 'Hoàn thành',
  [OrderStatus.Cancelled]: 'Đã huỷ',
}

/** Các bước chuyển hợp lệ — phải khớp với OrderStateMachine ở backend. */
export const ORDER_NEXT_STATUSES: Record<OrderStatusValue, OrderStatusValue[]> = {
  [OrderStatus.New]: [OrderStatus.Confirmed, OrderStatus.Cancelled],
  [OrderStatus.Confirmed]: [OrderStatus.Shipping, OrderStatus.Cancelled],
  [OrderStatus.Shipping]: [OrderStatus.Completed],
  [OrderStatus.Completed]: [],
  [OrderStatus.Cancelled]: [],
}

export const PaymentMethod = {
  Cod: 0,
  BankTransfer: 1,
} as const

export type PaymentMethodValue = (typeof PaymentMethod)[keyof typeof PaymentMethod]

export const PAYMENT_METHOD_LABEL: Record<PaymentMethodValue, string> = {
  [PaymentMethod.Cod]: 'Thanh toán khi nhận hàng (COD)',
  [PaymentMethod.BankTransfer]: 'Chuyển khoản ngân hàng',
}

export interface BookImage {
  url: string
  caption: string | null
  sortOrder: number
}

export interface Book {
  id: number
  name: string
  category: string
  title: string
  subtitle: string
  description: string
  price: number
  discountPrice: number | null
  coverImageUrl: string | null
  mockupImageUrl: string | null
  images: BookImage[]
}

export interface Author {
  fullName: string
  avatarUrl: string | null
  bio: string
}

export interface PressQuote {
  pressName: string
  logoUrl: string | null
  quote: string
  sourceUrl: string | null
}

export interface ContentReview {
  title: string
  content: string
  fileUrl: string | null
}

export interface RatingSummary {
  average: number
  count: number
}

export interface Feedback {
  id: number
  customerName: string
  rating: number
  content: string
  createdAt: string
}

export interface LandingResponse {
  book: Book
  author: Author | null
  pressQuotes: PressQuote[]
  review: ContentReview | null
  ratingSummary: RatingSummary
  feedbacks: Feedback[]
  settings: Record<string, string>
}

export interface CreateOrderRequest {
  customerName: string
  phone: string
  address: string
  quantity: number
  paymentMethod: PaymentMethodValue
  note?: string | null
}

export interface CreateOrderResponse {
  orderCode: string
  unitPrice: number
  totalPrice: number
}

export interface CreateFeedbackRequest {
  customerName: string
  rating: number
  content: string
}

export interface CreateFeedbackResponse {
  id: number
  message: string
}

export interface CurrentUser {
  id: number
  email: string
  fullName: string
  role: 'Admin' | 'Staff'
}

export interface LoginResponse {
  accessToken: string
  expiresAtUtc: string
  user: CurrentUser
}

export interface AdminBook extends Omit<Book, 'images'> {
  isActive: boolean
  updatedAt: string
}

export interface AdminOrder {
  id: number
  orderCode: string
  customerName: string
  phone: string
  address: string
  quantity: number
  unitPrice: number
  totalPrice: number
  paymentMethod: PaymentMethodValue
  status: OrderStatusValue
  note: string | null
  createdAt: string
  updatedAt: string
}

export interface AdminFeedback {
  id: number
  customerName: string
  rating: number
  content: string
  isApproved: boolean
  createdAt: string
  approvedAt: string | null
}

export interface AdminUser {
  id: number
  email: string
  fullName: string
  role: 'Admin' | 'Staff'
  isActive: boolean
  createdAt: string
  lastLoginAt: string | null
}

export interface SettingItem {
  key: string
  value: string
  description: string | null
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

/** ProblemDetails do backend tra ve khi loi. */
export interface ApiProblem {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
  traceId?: string
}

// ---- Admin: noi dung phu (tac gia, bao chi, review) ----

export interface AdminAuthor {
  id: number
  fullName: string
  avatarUrl: string | null
  bio: string
}

export type UpdateAuthorRequest = Omit<AdminAuthor, 'id'>

export interface AdminPressQuote {
  id: number
  pressName: string
  logoUrl: string | null
  quote: string
  sourceUrl: string | null
  sortOrder: number
}

export type PressQuoteRequest = Omit<AdminPressQuote, 'id'>

export interface AdminReview {
  id: number
  title: string
  content: string
  fileUrl: string | null
}

export type UpdateReviewRequest = Omit<AdminReview, 'id'>

// ---- Admin: tai khoan ----

export interface CreateUserRequest {
  email: string
  password: string
  fullName: string
  role: 'Admin' | 'Staff'
}

export interface UpdateUserRequest {
  fullName: string
  role: 'Admin' | 'Staff'
  isActive: boolean
  /** De trong neu khong doi mat khau. */
  newPassword?: string | null
}

// ---- Upload ----

/** Khop enum UploadKind o backend (binding theo ten). */
export type UploadKind = 'Image' | 'Pdf'

export interface StoredFile {
  url: string
  fileName: string
  size: number
}
