const vnd = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
const dateTime = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'short', timeStyle: 'short' })

export const formatPrice = (value: number) => vnd.format(value)

export const formatDateTime = (iso: string) => {
  const date = new Date(iso)
  return Number.isNaN(date.getTime()) ? '-' : dateTime.format(date)
}

/** Gia ban thuc te: uu tien gia giam neu hop le. Phai khop MoneyCalculator o backend. */
export const effectivePrice = (price: number, discountPrice: number | null) =>
  discountPrice !== null && discountPrice > 0 && discountPrice < price ? discountPrice : price

export const hasDiscount = (price: number, discountPrice: number | null) =>
  discountPrice !== null && discountPrice > 0 && discountPrice < price

export const discountPercent = (price: number, discountPrice: number | null) =>
  hasDiscount(price, discountPrice) ? Math.round((1 - discountPrice! / price) * 100) : 0
