import { Layout, Menu } from 'antd'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'

const { Header, Sider, Content } = Layout

export function AdminLayout() {
  const { user, isAdmin, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()

  // Menu doi theo role: Staff khong thay muc chi Admin duoc vao (AC-20 phia UI).
  const items = [
    { key: '/admin/orders', label: <Link to="/admin/orders">Đơn đặt hàng</Link> },
    { key: '/admin/feedbacks', label: <Link to="/admin/feedbacks">Đánh giá</Link> },
    ...(isAdmin
      ? [
          { key: '/admin/book', label: <Link to="/admin/book">Thông tin sách</Link> },
          { key: '/admin/content', label: <Link to="/admin/content">Nội dung landing</Link> },
          { key: '/admin/settings', label: <Link to="/admin/settings">Cấu hình site</Link> },
          { key: '/admin/users', label: <Link to="/admin/users">Tài khoản</Link> },
        ]
      : []),
  ]

  return (
    <Layout className="min-h-screen">
      <Header className="flex items-center justify-between">
        <span className="text-lg font-semibold text-white">Nhà Giả Kim - Quản trị</span>
        <span className="text-white/80">
          {user?.fullName} ({user?.role})
          <button
            type="button"
            className="ml-4 underline"
            onClick={() => {
              logout()
              navigate('/admin/login', { replace: true })
            }}
          >
            Đăng xuất
          </button>
        </span>
      </Header>

      <Layout>
        <Sider width={220} theme="light">
          <Menu mode="inline" selectedKeys={[location.pathname]} items={items} style={{ height: '100%' }} />
        </Sider>

        <Content className="bg-white p-6">
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
