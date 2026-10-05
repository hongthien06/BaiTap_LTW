import { Button, Drawer, Layout, Menu } from 'antd'
import { useState } from 'react'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'

const { Header, Sider, Content } = Layout

export function AdminLayout() {
  const { user, isAdmin, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const [menuOpen, setMenuOpen] = useState(false)

  // Menu đổi theo vai trò: Staff không thấy mục chỉ Admin mới vào được.
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

  const menu = (
    <Menu
      mode="inline"
      selectedKeys={[location.pathname]}
      items={items}
      onClick={() => setMenuOpen(false)}
      style={{ height: '100%', borderInlineEnd: 'none' }}
    />
  )

  return (
    <Layout className="min-h-screen">
      <Header className="flex items-center gap-3 px-4">
        <span className="lg:hidden">
          <Button
            type="text"
            style={{ color: '#f6f1e7' }}
            onClick={() => setMenuOpen(true)}
            aria-label="Mở menu"
          >
            ☰
          </Button>
        </span>

        {/* Header của Ant Design cao cố định 64px: để chữ xuống dòng là tràn ra ngoài. */}
        <Link to="/" className="truncate font-display text-lg whitespace-nowrap" style={{ color: '#f6f1e7' }}>
          Nhà Giả Kim
        </Link>
        <span className="hidden text-[13px] whitespace-nowrap sm:inline" style={{ color: '#f6f1e799' }}>
          Quản trị
        </span>

        <span className="ml-auto flex items-center gap-3 text-[14px]" style={{ color: '#f6f1e7cc' }}>
          <span className="hidden sm:inline">
            {user?.fullName} <span className="opacity-70">({user?.role})</span>
          </span>
          <Button
            size="small"
            onClick={() => {
              logout()
              navigate('/admin/login', { replace: true })
            }}
          >
            Đăng xuất
          </Button>
        </span>
      </Header>

      <Layout>
        <Sider width={232} theme="light" breakpoint="lg" collapsedWidth={0} trigger={null} className="hidden lg:block">
          {menu}
        </Sider>

        <Drawer
          open={menuOpen}
          onClose={() => setMenuOpen(false)}
          placement="left"
          width={260}
          title="Menu"
          styles={{ body: { padding: 0 } }}
        >
          {menu}
        </Drawer>

        <Content className="p-4 md:p-6">
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
