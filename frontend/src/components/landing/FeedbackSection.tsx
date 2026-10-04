import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { getErrorMessage } from '../../api/client'
import { publicApi } from '../../api/endpoints'
import type { Feedback, RatingSummary } from '../../api/types'
import { formatDateTime } from '../../lib/format'
import { StarRating } from './StarRating'

// Phai khop CreateFeedbackRequestValidator o backend. Backend van la nguon chan ly.
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
    <section id="danh-gia" className="mx-auto max-w-6xl px-4 py-16" aria-labelledby="feedback-title">
      <h2 id="feedback-title" className="font-display text-3xl">Độc giả nói gì</h2>

      <p className="mt-3 flex items-center gap-3" data-testid="rating-summary">
        <StarRating value={summary.average} />
        <span className="font-semibold">{summary.average.toFixed(1)}/5</span>
        <span className="text-ink/60">({summary.count} đánh giá)</span>
      </p>

      <div className="mt-8 grid gap-10 md:grid-cols-2">
        <ul className="space-y-4" data-testid="feedback-list">
          {feedbacks.length === 0 && <li className="text-ink/60">Chưa có đánh giá nào được duyệt.</li>}

          {feedbacks.map((feedback) => (
            <li key={feedback.id} className="rounded-lg bg-white p-5 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="font-semibold">{feedback.customerName}</span>
                <StarRating value={feedback.rating} />
              </div>
              {/* React tu escape chuoi nay -> noi dung doc hai hien thi dang text thuan (AC-16). */}
              <p className="mt-2 text-ink/80">{feedback.content}</p>
              <p className="mt-2 text-xs text-ink/50">{formatDateTime(feedback.createdAt)}</p>
            </li>
          ))}
        </ul>

        <form
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
          className="h-fit rounded-lg bg-white p-6 shadow-sm"
          noValidate
        >
          <h3 className="font-display text-xl">Gửi đánh giá của bạn</h3>

          <label className="mt-4 block text-sm font-medium" htmlFor="feedback-name">Họ tên</label>
          <input
            id="feedback-name"
            {...register('customerName')}
            className="mt-1 w-full rounded border border-ink/20 px-3 py-2"
          />
          {formState.errors.customerName && (
            <p className="mt-1 text-sm text-red-600">{formState.errors.customerName.message}</p>
          )}

          <span className="mt-4 block text-sm font-medium">Số sao</span>
          <Controller
            control={control}
            name="rating"
            render={({ field }) => <StarRating value={field.value} size="md" onChange={field.onChange} />}
          />

          <label className="mt-4 block text-sm font-medium" htmlFor="feedback-content">Nội dung</label>
          <textarea
            id="feedback-content"
            rows={4}
            {...register('content')}
            className="mt-1 w-full rounded border border-ink/20 px-3 py-2"
          />
          {formState.errors.content && (
            <p className="mt-1 text-sm text-red-600">{formState.errors.content.message}</p>
          )}

          <button
            type="submit"
            disabled={mutation.isPending}
            className="mt-5 rounded-full bg-ink px-6 py-2 font-semibold text-sand disabled:opacity-60"
          >
            {mutation.isPending ? 'Đang gửi...' : 'Gửi đánh giá'}
          </button>

          {sent && <p className="mt-3 text-sm text-green-700" role="status">{sent}</p>}
          {error && <p className="mt-3 text-sm text-red-600" role="alert">{error}</p>}
        </form>
      </div>
    </section>
  )
}
