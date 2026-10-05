import type { ReactNode } from 'react'

interface Props {
  id?: string
  title?: string
  /** Noi dung canh tieu de, vi du diem danh gia trung binh. */
  aside?: ReactNode
  children: ReactNode
  className?: string
}

/**
 * Khung chung cua moi khoi tren landing: card trang, vien toc, bo 12px, KHONG bong.
 * Thu bac dua bang co chu va khoang trang chu khong bang do noi.
 */
export function Section({ id, title, aside, children, className = '' }: Props) {
  const titleId = id ? `${id}-title` : undefined

  return (
    <section
      id={id}
      aria-labelledby={titleId}
      className={`rounded-card border border-border bg-surface p-6 md:p-8 ${className}`}
    >
      {title && (
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
          <h2 id={titleId} className="font-display text-2xl md:text-3xl">{title}</h2>
          {aside}
        </div>
      )}
      {children}
    </section>
  )
}
