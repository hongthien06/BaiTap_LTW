import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { describe, expect, it } from 'vitest'
import type { Feedback } from '../../api/types'
import { FeedbackSection } from './FeedbackSection'

function wrap(ui: ReactNode) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>)
}

describe('FeedbackSection', () => {
  // AC-16: noi dung nguoi dung phai hien thi dang text thuan, khong duoc thuc thi.
  it('render nội dung chứa thẻ script dưới dạng text thuần', () => {
    const malicious = '<script>alert(1)</script>'
    const feedbacks: Feedback[] = [
      { id: 1, customerName: 'Hacker', rating: 5, content: malicious, createdAt: '2026-10-04T10:00:00Z' },
    ]

    const { container } = wrap(
      <FeedbackSection summary={{ average: 5, count: 1 }} feedbacks={feedbacks} />,
    )

    expect(screen.getByText(malicious)).toBeInTheDocument()
    expect(container.querySelector('script')).toBeNull()
  })

  it('hiển thị điểm trung bình và số lượng đánh giá', () => {
    wrap(<FeedbackSection summary={{ average: 4.5, count: 12 }} feedbacks={[]} />)

    const summary = screen.getByTestId('rating-summary')
    expect(summary).toHaveTextContent('4.5/5')
    expect(summary).toHaveTextContent('12 đánh giá')
  })

  it('báo chưa có đánh giá khi danh sách rỗng', () => {
    wrap(<FeedbackSection summary={{ average: 0, count: 0 }} feedbacks={[]} />)

    expect(screen.getByText(/chưa có đánh giá nào được duyệt/i)).toBeInTheDocument()
  })
})
