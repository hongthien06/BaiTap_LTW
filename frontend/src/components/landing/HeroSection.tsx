import type { Book } from '../../api/types'
import { discountPercent, effectivePrice, formatPrice, hasDiscount } from '../../lib/format'
import { Button } from '../ui/Button'
import { StarRating } from './StarRating'

interface Props {
  book: Book
  average: number
  reviewCount: number
  onOrderClick: () => void
  onReviewClick: () => void
}

export function HeroSection({ book, average, reviewCount, onOrderClick, onReviewClick }: Props) {
  const showDiscount = hasDiscount(book.price, book.discountPrice)
  const sellPrice = effectivePrice(book.price, book.discountPrice)

  return (
    <section
      aria-labelledby="hero-title"
      className="rounded-card border border-border bg-surface px-6 py-10 md:px-10 md:py-14"
    >
      <div className="grid items-center gap-10 md:grid-cols-[1fr_minmax(0,300px)]">
        <div className="order-2 md:order-1">
          <p className="text-[13px] font-medium uppercase tracking-[0.18em] text-primary">
            {book.category}
          </p>

          <h1 id="hero-title" className="mt-3 font-display text-4xl leading-[1.12] md:text-5xl">
            {book.title}
          </h1>

          <p className="mt-4 max-w-prose text-lg text-muted">{book.subtitle}</p>

          <div className="mt-6 flex flex-wrap items-baseline gap-3">
            <span className="text-4xl font-semibold text-primary" data-testid="sell-price">
              {formatPrice(sellPrice)}
            </span>

            {showDiscount && (
              <>
                <s className="text-lg text-muted" data-testid="original-price">
                  {formatPrice(book.price)}
                </s>
                <span
                  className="rounded-full bg-primary px-2.5 py-0.5 text-[13px] font-semibold text-primary-foreground"
                  data-testid="discount-badge"
                >
                  -{discountPercent(book.price, book.discountPrice)}%
                </span>
              </>
            )}
          </div>

          <div className="mt-6 flex flex-wrap gap-3">
            <Button size="lg" onClick={onOrderClick}>Đặt mua ngay</Button>
            <Button size="lg" variant="outline" onClick={onReviewClick}>Đọc thử</Button>
          </div>

          {reviewCount > 0 && (
            <p className="mt-5 flex flex-wrap items-center gap-2 text-[14px] text-muted">
              <StarRating value={average} />
              <span className="font-medium text-foreground">{average.toFixed(1)}/5</span>
              <span>từ {reviewCount} đánh giá</span>
              <span aria-hidden="true">·</span>
              <span>Giao toàn quốc</span>
            </p>
          )}
        </div>

        <div className="order-1 md:order-2">
          {book.mockupImageUrl ? (
            <img
              src={book.mockupImageUrl}
              alt={`Ảnh bìa sách ${book.name}`}
              className="mx-auto w-full max-w-[280px]"
              loading="eager"
              width={280}
              height={420}
            />
          ) : (
            <div className="mx-auto grid aspect-2/3 w-full max-w-[280px] place-items-center rounded-card border border-border-strong text-muted">
              Chưa có ảnh
            </div>
          )}
        </div>
      </div>
    </section>
  )
}
