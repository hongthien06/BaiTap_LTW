import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AdminLayout } from './components/admin/AdminLayout'
import { ProtectedRoute } from './components/admin/ProtectedRoute'
import { BookPage } from './pages/admin/BookPage'
import { ContentPage } from './pages/admin/ContentPage'
import { FeedbacksPage } from './pages/admin/FeedbacksPage'
import { LoginPage } from './pages/admin/LoginPage'
import { OrdersPage } from './pages/admin/OrdersPage'
import { SettingsPage } from './pages/admin/SettingsPage'
import { UsersPage } from './pages/admin/UsersPage'
import { LandingPage } from './pages/public/LandingPage'

export const router = createBrowserRouter([
  { path: '/', element: <LandingPage /> },
  { path: '/admin/login', element: <LoginPage /> },
  {
    path: '/admin',
    element: (
      <ProtectedRoute>
        <AdminLayout />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="/admin/orders" replace /> },
      { path: 'orders', element: <OrdersPage /> },
      { path: 'feedbacks', element: <FeedbacksPage /> },
      { path: 'book', element: <ProtectedRoute adminOnly><BookPage /></ProtectedRoute> },
      { path: 'content', element: <ProtectedRoute adminOnly><ContentPage /></ProtectedRoute> },
      { path: 'settings', element: <ProtectedRoute adminOnly><SettingsPage /></ProtectedRoute> },
      { path: 'users', element: <ProtectedRoute adminOnly><UsersPage /></ProtectedRoute> },
    ],
  },
  { path: '*', element: <Navigate to="/" replace /> },
])
