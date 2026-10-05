import { effectivePrice, formatPrice, hasDiscount } from '../../lib/format'
import type { Book } from '../../api/types'
import { Button } from '../ui/Button'

interface Props {
  book: Book
  onOrderClick: () => void
}

/**
 * Thanh mua dinh day, CHI tren mobile. Landing la phieu mot nut, ma tren dien thoai
 * trang dai gan 5000px - khong co thanh nay thi nut dat mua gan nhu luon o ngoai man hinh.
 * An tren desktop vi o do da co CTA lap giua trang va form cuoi trang.
 */
export function StickyBuyBar({ book, onOrderClick }: Props) {
  const sellPrice = effectivePrice(book.price, book.discountPrice)

  return (
    <div
      className="sticky bottom-0 z-30 flex items-center justify-between gap-4 border-t border-border-strong bg-surface px-4 py-3 md:hidden"
      style={{ paddingBottom: 'max(0.75rem, env(safe-area-inset-bottom))' }}
    >
      <div className="min-w-0">
        <p className="text-lg font-semibold text-primary">{formatPrice(sellPrice)}</p>
        {hasDiscount(book.price, book.discountPrice) && (
          <s className="text-[13px] text-muted">{formatPrice(book.price)}</s>
        )}
      </div>
      <Button onClick={onOrderClick} className="shrink-0">Đặt mua</Button>
    </div>
  )
}
