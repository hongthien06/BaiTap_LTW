import { ConfigProvider } from 'antd'
import viVN from 'antd/locale/vi_VN'
import type { ReactNode } from 'react'

/**
 * ConfigProvider cua Ant Design dat o DAY chu khong o main.tsx.
 * De o main.tsx thi antd nam trong bundle chinh, va khach vao landing page phai tai
 * ca thu vien ma trang do khong he dung.
 *
 * Theme lay dung token trong index.css de admin va landing khong nhu hai san pham.
 * colorTextLightSolid la chu dung tren nen mau nhan: mac dinh cua antd la TRANG,
 * ma trang tren vang #c8973f chi 2.64:1 - truot WCAG AA. Doi sang muc: 6.5:1.
 */
const theme = {
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

export function AdminProviders({ children }: { children: ReactNode }) {
  return (
    <ConfigProvider locale={viVN} theme={theme}>
      {children}
    </ConfigProvider>
  )
}
