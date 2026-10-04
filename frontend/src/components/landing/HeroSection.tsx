import type { Book } from '../../api/types'
import { discountPercent, effectivePrice, formatPrice, hasDiscount } from '../../lib/format'

interface Props {
  book: Book
  onOrderClick: () => void
}

export function HeroSection({ book, onOrderClick }: Props) {
  const showDiscount = hasDiscount(book.price, book.discountPrice)
  const sellPrice = effectivePrice(book.price, book.discountPrice)

  return (
    <section className="bg-ink text-sand" aria-labelledby="hero-title">
      <div className="mx-auto grid max-w-6xl gap-10 px-4 py-16 md:grid-cols-2 md:items-center md:py-24">
        <div className="order-2 md:order-1">
          <p className="mb-3 text-sm uppercase tracking-[0.2em] text-gold">{book.category}</p>

          <h1 id="hero-title" className="font-display text-4xl leading-tight md:text-5xl">
            {book.title}
          </h1>

          <p className="mt-5 text-lg text-sand/80">{book.subtitle}</p>

          <div className="mt-8 flex flex-wrap items-baseline gap-3">
            <span className="text-3xl font-semibold text-gold" data-testid="sell-price">
              {formatPrice(sellPrice)}
            </span>

            {showDiscount && (
              <>
                <s className="text-lg text-sand/50" data-testid="original-price">
                  {formatPrice(book.price)}
                </s>
                <span className="rounded-full bg-gold px-3 py-1 text-sm font-semibold text-ink" data-testid="discount-badge">
                  -{discountPercent(book.price, book.discountPrice)}%
                </span>
              </>
            )}
          </div>

          <button
            type="button"
            onClick={onOrderClick}
            className="mt-8 rounded-full bg-gold px-8 py-3 font-semibold text-ink transition hover:bg-gold-dark"
          >
            Đặt mua ngay
          </button>
        </div>

        <div className="order-1 flex justify-center md:order-2">
          {book.mockupImageUrl ? (
            <img
              src={book.mockupImageUrl}
              alt={`Ảnh bìa sách ${book.name}`}
              className="w-64 max-w-full drop-shadow-2xl md:w-80"
              loading="eager"
            />
          ) : (
            <div className="flex h-80 w-56 items-center justify-center rounded border border-sand/20 text-sand/40">
              Chưa có ảnh
            </div>
          )}
        </div>
      </div>
    </section>
  )
}
