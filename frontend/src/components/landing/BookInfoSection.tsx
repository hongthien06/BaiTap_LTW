import type { Book } from '../../api/types'

export function BookInfoSection({ book }: { book: Book }) {
  return (
    <section id="thong-tin-sach" className="mx-auto max-w-6xl px-4 py-16" aria-labelledby="book-info-title">
      <h2 id="book-info-title" className="font-display text-3xl">Về cuốn sách</h2>

      <div className="mt-8 grid gap-8 md:grid-cols-[2fr_1fr]">
        <p className="whitespace-pre-line text-lg leading-relaxed text-ink/80">{book.description}</p>

        <dl className="h-fit rounded-lg bg-white p-6 shadow-sm">
          <dt className="text-sm text-ink/60">Tên sách</dt>
          <dd className="mb-4 font-semibold">{book.name}</dd>
          <dt className="text-sm text-ink/60">Thể loại</dt>
          <dd className="font-semibold">{book.category}</dd>
        </dl>
      </div>

      {book.images.length > 0 && (
        <ul className="mt-10 grid grid-cols-2 gap-4 md:grid-cols-4">
          {book.images.map((image) => (
            <li key={image.url}>
              <img
                src={image.url}
                alt={image.caption ?? book.name}
                loading="lazy"
                className="aspect-3/4 w-full rounded object-cover shadow-sm"
              />
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
