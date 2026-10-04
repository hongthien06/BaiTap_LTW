import type { PressQuote } from '../../api/types'
import { safeHref } from '../../lib/url'

export function PressSection({ quotes }: { quotes: PressQuote[] }) {
  if (quotes.length === 0) return null

  return (
    <section id="bao-chi" className="mx-auto max-w-6xl px-4 py-16" aria-labelledby="press-title">
      <h2 id="press-title" className="font-display text-3xl">Báo chí viết về sách</h2>

      <ul className="mt-8 grid gap-6 md:grid-cols-3" data-testid="press-list">
        {quotes.map((quote) => (
          <li key={`${quote.pressName}-${quote.quote}`} className="rounded-lg bg-white p-6 shadow-sm">
            {quote.logoUrl ? (
              <img src={quote.logoUrl} alt={quote.pressName} loading="lazy" className="h-8 object-contain" />
            ) : (
              <p className="font-semibold text-gold-dark">{quote.pressName}</p>
            )}

            <blockquote className="mt-4 italic text-ink/80">“{quote.quote}”</blockquote>

            {safeHref(quote.sourceUrl) && (
              <a
                href={safeHref(quote.sourceUrl)}
                target="_blank"
                rel="noopener noreferrer"
                className="mt-4 inline-block text-sm font-medium text-gold-dark underline"
              >
                Đọc bài gốc
              </a>
            )}
          </li>
        ))}
      </ul>
    </section>
  )
}
