import type { ContentReview } from '../../api/types'

export function ReviewSection({ review }: { review: ContentReview }) {
  return (
    <section id="review" className="bg-white py-16" aria-labelledby="review-title">
      <div className="mx-auto max-w-4xl px-4">
        <h2 id="review-title" className="font-display text-3xl">{review.title}</h2>
        <p className="mt-6 whitespace-pre-line text-lg leading-relaxed text-ink/80">{review.content}</p>

        {review.fileUrl && (
          <a
            href={review.fileUrl}
            download
            className="mt-8 inline-block rounded-full border-2 border-gold px-6 py-2 font-semibold text-gold-dark transition hover:bg-gold hover:text-ink"
            data-testid="review-download"
          >
            Tải bản review (PDF)
          </a>
        )}
      </div>
    </section>
  )
}
