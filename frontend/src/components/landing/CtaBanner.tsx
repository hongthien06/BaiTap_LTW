import { formatPrice } from '../../lib/format'
import { Button } from '../ui/Button'

interface Props {
  price: number
  onOrderClick: () => void
}

/**
 * CTA lap giua trang. Ban cu chi co nut o Hero roi mat hut toi tan cuoi trang:
 * khach cuon toi giua, muon mua thi khong co gi de bam.
 */
export function CtaBanner({ price, onOrderClick }: Props) {
  return (
    <section
      aria-label="Đặt mua nhanh"
      className="flex flex-wrap items-center justify-between gap-5 rounded-card border border-primary bg-primary-light p-6 md:p-8"
    >
      <div>
        <p className="font-display text-xl md:text-2xl">Đặt sách ngay hôm nay</p>
        <p className="mt-1 text-muted">
          {formatPrice(price)} · giao toàn quốc · trả tiền khi nhận hàng
        </p>
      </div>
      <Button size="lg" onClick={onOrderClick}>Đặt mua ngay</Button>
    </section>
  )
}
