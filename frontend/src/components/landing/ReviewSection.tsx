import type { ContentReview } from '../../api/types'
import { safeHref } from '../../lib/url'
import { Button } from '../ui/Button'
import { Section } from '../ui/Section'

export function ReviewSection({ review }: { review: ContentReview }) {
  const fileHref = safeHref(review.fileUrl)

  return (
    <Section id="review" title={review.title}>
      <p className="max-w-prose whitespace-pre-line text-lg leading-relaxed text-muted">
        {review.content}
      </p>

      {fileHref && (
        <a href={fileHref} download className="mt-6 inline-block" data-testid="review-download">
          <Button variant="outline" type="button" tabIndex={-1}>Tải bản review (PDF)</Button>
        </a>
      )}
    </Section>
  )
}
