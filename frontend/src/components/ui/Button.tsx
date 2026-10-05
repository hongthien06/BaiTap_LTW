import type { ButtonHTMLAttributes, ReactNode } from 'react'

type Variant = 'primary' | 'outline' | 'ghost'
type Size = 'md' | 'lg'

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  size?: Size
  icon?: ReactNode
}

// Chi bon dang nut, khong them. Nhieu dang hon thi khong ai con biet dang nao la chinh.
const BASE =
  'inline-flex items-center justify-center gap-2 rounded-lg font-semibold transition-colors ' +
  'disabled:cursor-not-allowed disabled:opacity-60'

const VARIANT: Record<Variant, string> = {
  // Chu tren nen nhan la --primary-foreground (muc), KHONG phai trang: trang tren vang chi 2.64:1.
  primary: 'bg-primary text-primary-foreground hover:bg-primary-hover',
  outline: 'border border-border-strong bg-surface text-foreground hover:border-foreground/30',
  ghost: 'text-foreground hover:bg-primary-light',
}

const SIZE: Record<Size, string> = {
  md: 'h-11 px-5 text-[15px]',
  lg: 'h-12 px-7 text-base',
}

export function Button({ variant = 'primary', size = 'md', icon, children, className = '', ...rest }: Props) {
  return (
    <button className={`${BASE} ${VARIANT[variant]} ${SIZE[size]} ${className}`} {...rest}>
      {icon}
      {children}
    </button>
  )
}
