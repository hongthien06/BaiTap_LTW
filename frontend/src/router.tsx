import { Suspense, lazy, type ReactNode } from 'react'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { ProtectedRoute } from './components/admin/ProtectedRoute'
import { LandingPage } from './pages/public/LandingPage'

/**
 * Trang quản trị nạp theo kiểu lazy. Khách vào landing page chiếm gần hết lưu lượng,
 * mà Ant Design chỉ dùng ở khu /admin — gộp chung thì mỗi khách mua sách phải tải
 * thêm khoảng 900 KB không bao giờ chạy tới.
 *
 * Landing KHÔNG lazy: nó là trang đầu tiên, lazy chỉ thêm một vòng chờ.
 */
const AdminProviders = lazy(() => import('./components/admin/AdminProviders').then((m) => ({ default: m.AdminProviders })))
const AdminLayout = lazy(() => import('./components/admin/AdminLayout').then((m) => ({ default: m.AdminLayout })))
const LoginPage = lazy(() => import('./pages/admin/LoginPage').then((m) => ({ default: m.LoginPage })))
const OrdersPage = lazy(() => import('./pages/admin/OrdersPage').then((m) => ({ default: m.OrdersPage })))
const FeedbacksPage = lazy(() => import('./pages/admin/FeedbacksPage').then((m) => ({ default: m.FeedbacksPage })))
const BookPage = lazy(() => import('./pages/admin/BookPage').then((m) => ({ default: m.BookPage })))
const ContentPage = lazy(() => import('./pages/admin/ContentPage').then((m) => ({ default: m.ContentPage })))
const SettingsPage = lazy(() => import('./pages/admin/SettingsPage').then((m) => ({ default: m.SettingsPage })))
const UsersPage = lazy(() => import('./pages/admin/UsersPage').then((m) => ({ default: m.UsersPage })))

/** Mọi màn quản trị đi qua đây: nạp lazy và bọc trong ConfigProvider của Ant Design. */
function Lazy({ children }: { children: ReactNode }) {
  return (
    <Suspense fallback={<p className="p-10 text-center text-muted">Đang tải...</p>}>
      <AdminProviders>{children}</AdminProviders>
    </Suspense>
  )
}

export const router = createBrowserRouter([
  { path: '/', element: <LandingPage /> },
  { path: '/admin/login', element: <Lazy><LoginPage /></Lazy> },
  {
    path: '/admin',
    element: (
      <Lazy>
        <ProtectedRoute>
          <AdminLayout />
        </ProtectedRoute>
      </Lazy>
    ),
    children: [
      { index: true, element: <Navigate to="/admin/orders" replace /> },
      { path: 'orders', element: <Lazy><OrdersPage /></Lazy> },
      { path: 'feedbacks', element: <Lazy><FeedbacksPage /></Lazy> },
      { path: 'book', element: <ProtectedRoute adminOnly><Lazy><BookPage /></Lazy></ProtectedRoute> },
      { path: 'content', element: <ProtectedRoute adminOnly><Lazy><ContentPage /></Lazy></ProtectedRoute> },
      { path: 'settings', element: <ProtectedRoute adminOnly><Lazy><SettingsPage /></Lazy></ProtectedRoute> },
      { path: 'users', element: <ProtectedRoute adminOnly><Lazy><UsersPage /></Lazy></ProtectedRoute> },
    ],
  },
  { path: '*', element: <Navigate to="/" replace /> },
])
