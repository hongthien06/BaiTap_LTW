import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { TOKEN_STORAGE_KEY, isOnAdminPage } from '../api/client'
import { queryClient } from '../lib/queryClient'
import { authApi } from '../api/endpoints'
import type { CurrentUser } from '../api/types'

interface AuthContextValue {
  user: CurrentUser | null
  isLoading: boolean
  isAdmin: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  // Khoi dong: neu con token thi hoi lai server xem con hieu luc khong.
  // Chi kiem tra khi dang o khu admin - khach xem landing khong can biet phien admin con hay het,
  // va goi me() o day se tao mot 401 vo ich ngay tren trang ban hang.
  useEffect(() => {
    const token = localStorage.getItem(TOKEN_STORAGE_KEY)
    if (!token || !isOnAdminPage()) {
      setIsLoading(false)
      return
    }

    authApi.me()
      .then(setUser)
      .catch(() => {
        localStorage.removeItem(TOKEN_STORAGE_KEY)
        setUser(null)
      })
      .finally(() => setIsLoading(false))
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const result = await authApi.login(email, password)
    localStorage.setItem(TOKEN_STORAGE_KEY, result.accessToken)
    setUser(result.user)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_STORAGE_KEY)
    // Xoa cache: khong thi du lieu cua nguoi vua dang xuat con duoc phuc vu
    // cho nguoi dang nhap tiep theo trong cung tab (staleTime 30s).
    queryClient.clear()
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({ user, isLoading, isAdmin: user?.role === 'Admin', login, logout }),
    [user, isLoading, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth phải được dùng bên trong AuthProvider')
  return context
}
