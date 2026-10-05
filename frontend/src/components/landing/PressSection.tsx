import type { PressQuote } from '../../api/types'
import { safeHref } from '../../lib/url'
import { Section } from '../ui/Section'

export function PressSection({ quotes }: { quotes: PressQuote[] }) {
  if (quotes.length === 0) return null

  return (
    <Section id="bao-chi" title="Báo chí viết về sách">
      <ul className="grid gap-4 md:grid-cols-3" data-testid="press-list">
        {quotes.map((quote) => {
          const href = safeHref(quote.sourceUrl)

          return (
            <li key={`${quote.pressName}-${quote.quote}`} className="rounded-lg border border-border p-5">
              {quote.logoUrl ? (
                <img src={quote.logoUrl} alt={quote.pressName} loading="lazy" className="h-7 object-contain" />
              ) : (
                <p className="text-[13px] font-semibold text-primary">{quote.pressName}</p>
              )}

              <blockquote className="mt-3 italic text-muted">“{quote.quote}”</blockquote>

              {href && (
                <a
                  href={href}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="mt-4 inline-block text-[14px] font-medium underline underline-offset-4"
                >
                  Đọc bài gốc
                </a>
              )}
            </li>
          )
        })}
      </ul>
    </Section>
  )
}
