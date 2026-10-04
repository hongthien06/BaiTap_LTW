import type { Author } from '../../api/types'

export function AuthorSection({ author }: { author: Author }) {
  return (
    <section id="tac-gia" className="bg-white py-16" aria-labelledby="author-title">
      <div className="mx-auto flex max-w-6xl flex-col items-center gap-10 px-4 md:flex-row md:items-start">
        {author.avatarUrl && (
          <img
            src={author.avatarUrl}
            alt={`Ảnh tác giả ${author.fullName}`}
            loading="lazy"
            className="size-40 shrink-0 rounded-full object-cover shadow-md"
          />
        )}

        <div>
          <h2 id="author-title" className="font-display text-3xl">Chi tiết tác giả</h2>
          <p className="mt-2 text-xl font-semibold text-gold-dark">{author.fullName}</p>
          <p className="mt-4 whitespace-pre-line leading-relaxed text-ink/80">{author.bio}</p>
        </div>
      </div>
    </section>
  )
}
