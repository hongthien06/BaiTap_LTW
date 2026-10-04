import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'

interface Props {
  children: ReactNode
  /** true = chi role Admin moi vao duoc. Staff se bi day ve trang don hang. */
  adminOnly?: boolean
}

export function ProtectedRoute({ children, adminOnly = false }: Props) {
  const { user, isLoading, isAdmin } = useAuth()
  const location = useLocation()

  if (isLoading) return <p className="p-10 text-center">Đang kiểm tra phiên đăng nhập...</p>

  if (!user) return <Navigate to="/admin/login" replace state={{ from: location.pathname }} />

  if (adminOnly && !isAdmin) return <Navigate to="/admin/orders" replace />

  return <>{children}</>
}
