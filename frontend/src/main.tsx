import { QueryClientProvider } from '@tanstack/react-query'
import { ConfigProvider } from 'antd'
import viVN from 'antd/locale/vi_VN'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { AuthProvider } from './hooks/useAuth'
import './index.css'
import { queryClient } from './lib/queryClient'
import { router } from './router'

/**
 * Theme của Ant Design lấy đúng token trong index.css để trang quản trị và landing
 * không trông như hai sản phẩm khác nhau.
 *
 * `colorTextLightSolid` là chữ đứng trên nền màu nhấn. Mặc định của Ant Design là TRẮNG,
 * mà trắng trên vàng #c8973f chỉ đạt 2.64:1 — trượt WCAG AA. Đổi sang mực: 6.5:1.
 */
const antdTheme = {
  token: {
    colorPrimary: '#c8973f',
    colorTextLightSolid: '#1b1b1f',
    colorLink: '#a97a28',
    colorText: '#1b1b1f',
    colorTextSecondary: '#6f6a60',
    colorBgLayout: '#f6f1e7',
    colorBgContainer: '#ffffff',
    colorBorder: '#ddd3c0',
    colorBorderSecondary: '#ece5d7',
    colorSuccess: '#15803d',
    colorWarning: '#b45309',
    colorError: '#b91c1c',
    colorInfo: '#1d4ed8',
    borderRadius: 10,
    fontFamily: '"Inter", ui-sans-serif, system-ui, sans-serif',
  },
  components: {
    Layout: { headerBg: '#1b1b1f', siderBg: '#ffffff', bodyBg: '#f6f1e7' },
    Menu: { itemSelectedBg: '#c8973f1a', itemSelectedColor: '#a97a28' },
    Table: { headerBg: '#ffffff', headerColor: '#6f6a60', rowHoverBg: '#f6f1e7' },
  },
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ConfigProvider locale={viVN} theme={antdTheme}>
        <AuthProvider>
          <RouterProvider router={router} />
        </AuthProvider>
      </ConfigProvider>
    </QueryClientProvider>
  </StrictMode>,
)
