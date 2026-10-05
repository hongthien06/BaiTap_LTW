import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { getErrorMessage } from '../../api/client'
import { publicApi } from '../../api/endpoints'
import type { Feedback, RatingSummary } from '../../api/types'
import { formatDateTime } from '../../lib/format'
import { Button } from '../ui/Button'
import { Section } from '../ui/Section'
import { StarRating } from './StarRating'

// Phải khớp CreateFeedbackRequestValidator ở backend. Backend vẫn là nguồn chân lý.
const schema = z.object({
  customerName: z.string().trim().min(2, 'Tên phải có ít nhất 2 ký tự').max(200, 'Tên tối đa 200 ký tự'),
  rating: z.number().int().min(1, 'Vui lòng chọn số sao').max(5),
  content: z.string().trim().min(1, 'Vui lòng nhập nội dung').max(2000, 'Nội dung tối đa 2000 ký tự'),
})

type FormValues = z.infer<typeof schema>

interface Props {
  summary: RatingSummary
  feedbacks: Feedback[]
}

export function FeedbackSection({ summary, feedbacks }: Props) {
  const [sent, setSent] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { register, handleSubmit, control, reset, formState } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { customerName: '', rating: 5, content: '' },
  })

  const mutation = useMutation({
    mutationFn: publicApi.createFeedback,
    onSuccess: (data) => {
      setSent(data.message)
      setError(null)
      reset()
    },
    onError: (err) => {
      setError(getErrorMessage(err))
      setSent(null)
    },
  })

  return (
    <Section
      id="danh-gia"
      title="Độc giả nói gì"
      aside={
        <p className="flex flex-wrap items-center gap-2" data-testid="rating-summary">
          <StarRating value={summary.average} />
          <span className="font-semibold">{summary.average.toFixed(1)}/5</span>
          <span className="text-muted">({summary.count} đánh giá)</span>
        </p>
      }
    >
      <div className="grid gap-8 lg:grid-cols-2">
        <ul className="grid content-start gap-3" data-testid="feedback-list">
          {feedbacks.length === 0 && (
            <li className="rounded-lg border border-dashed border-border-strong p-6 text-center text-muted">
              Chưa có đánh giá nào được duyệt. Bạn là người đầu tiên?
            </li>
          )}

          {feedbacks.map((feedback) => (
            <li key={feedback.id} className="rounded-lg border border-border p-4">
              <div className="flex items-start justify-between gap-3">
                {/* min-w-0 + break-words: tên dài (ví dụ "Nguyễn Hoàng Anh Tuấn Kiệt") từng
                    đẩy cả trang rộng ra 386px trên màn 375px vì flex item không co được. */}
                <span className="min-w-0 flex-1 font-medium break-words">{feedback.customerName}</span>
                <span className="shrink-0"><StarRating value={feedback.rating} /></span>
              </div>

              {/* React tự escape chuỗi này -> nội dung độc hại hiển thị dạng text thuần. */}
              <p className="mt-2 text-muted">{feedback.content}</p>
              <p className="mt-2 text-[13px] text-muted">{formatDateTime(feedback.createdAt)}</p>
            </li>
          ))}
        </ul>

        <form
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
          className="h-fit rounded-lg border border-border bg-background p-5"
          noValidate
        >
          <h3 className="font-display text-xl">Gửi đánh giá của bạn</h3>

          <label className="mt-4 block">
            <span className="mb-1.5 block text-[13px] font-medium">Họ tên</span>
            <input
              {...register('customerName')}
              className="h-11 w-full rounded-lg border border-border-strong bg-surface px-3 placeholder:text-muted"
            />
          </label>
          {formState.errors.customerName && (
            <p className="mt-1.5 text-[13px] text-danger">{formState.errors.customerName.message}</p>
          )}

          <div className="mt-4">
            <span className="mb-1.5 block text-[13px] font-medium">Số sao</span>
            <Controller
              control={control}
              name="rating"
              render={({ field }) => <StarRating value={field.value} size="md" onChange={field.onChange} />}
            />
          </div>

          <label className="mt-4 block">
            <span className="mb-1.5 block text-[13px] font-medium">Nội dung</span>
            <textarea
              rows={4}
              {...register('content')}
              className="w-full rounded-lg border border-border-strong bg-surface px-3 py-2 placeholder:text-muted"
            />
          </label>
          {formState.errors.content && (
            <p className="mt-1.5 text-[13px] text-danger">{formState.errors.content.message}</p>
          )}

          <Button type="submit" variant="outline" className="mt-5 w-full" disabled={mutation.isPending}>
            {mutation.isPending ? 'Đang gửi...' : 'Gửi đánh giá'}
          </Button>

          {sent && <p className="mt-3 text-[14px] text-success" role="status">{sent}</p>}
          {error && <p className="mt-3 text-[14px] text-danger" role="alert">{error}</p>}
        </form>
      </div>
    </Section>
  )
}
