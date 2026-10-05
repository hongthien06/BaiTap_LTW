import type { Book } from '../../api/types'
import { Section } from '../ui/Section'

export function BookInfoSection({ book }: { book: Book }) {
  return (
    <Section id="thong-tin-sach" title="Về cuốn sách">
      <p className="max-w-prose whitespace-pre-line text-lg leading-relaxed text-muted">
        {book.description}
      </p>

      <dl className="mt-6 grid gap-3 sm:grid-cols-2">
        <div className="rounded-lg border border-border p-4">
          <dt className="text-[13px] text-muted">Tên sách</dt>
          <dd className="mt-0.5 font-medium">{book.name}</dd>
        </div>
        <div className="rounded-lg border border-border p-4">
          <dt className="text-[13px] text-muted">Thể loại</dt>
          <dd className="mt-0.5 font-medium">{book.category}</dd>
        </div>
      </dl>

      {book.images.length > 0 && (
        <ul className="mt-6 grid grid-cols-2 gap-4 md:grid-cols-4">
          {book.images.map((image) => (
            <li key={image.url}>
              <img
                src={image.url}
                alt={image.caption ?? book.name}
                loading="lazy"
                className="aspect-2/3 w-full rounded-lg border border-border object-cover"
              />
            </li>
          ))}
        </ul>
      )}
    </Section>
  )
}
