import type { Author } from '../../api/types'
import { Section } from '../ui/Section'

export function AuthorSection({ author }: { author: Author }) {
  return (
    <Section id="tac-gia" title="Chi tiết tác giả">
      <div className="flex flex-col gap-5 sm:flex-row">
        {author.avatarUrl && (
          <img
            src={author.avatarUrl}
            alt={`Ảnh tác giả ${author.fullName}`}
            loading="lazy"
            width={96}
            height={96}
            className="size-24 shrink-0 rounded-full border border-border object-cover"
          />
        )}

        <div className="min-w-0">
          <p className="text-lg font-semibold text-primary">{author.fullName}</p>
          <p className="mt-2 max-w-prose whitespace-pre-line leading-relaxed text-muted">
            {author.bio}
          </p>
        </div>
      </div>
    </Section>
  )
}
