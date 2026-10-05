import type { ReactNode } from 'react'

interface HeaderProps {
  title: string
  description?: ReactNode
  actions?: ReactNode
}

/** Dau trang quan tri: moi man mot kieu thi nguoi dung phai do lai mat moi lan chuyen trang. */
export function AdminPageHeader({ title, description, actions }: HeaderProps) {
  return (
    <div className="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 className="font-display text-2xl">{title}</h1>
        {description && <p className="mt-1 text-[14px] text-muted">{description}</p>}
      </div>
      {actions}
    </div>
  )
}

interface CardProps {
  children: ReactNode
  className?: string
  /** Bang cua Ant Design tu co padding rieng, bo padding cua card di cho khoi chong. */
  flush?: boolean
}

/** Khung card dung chung: vien toc, bo 12px, KHONG bong — giong het landing. */
export function AdminCard({ children, className = '', flush = false }: CardProps) {
  return (
    <div className={`rounded-card border border-border bg-surface ${flush ? 'overflow-hidden' : 'p-4 md:p-5'} ${className}`}>
      {children}
    </div>
  )
}

/** Form quan tri khong bao gio rong qua 720px: dong chu dai hon the rat kho doc. */
export function AdminFormCard({ children, className = '' }: CardProps) {
  return <AdminCard className={`max-w-[720px] ${className}`}>{children}</AdminCard>
}
