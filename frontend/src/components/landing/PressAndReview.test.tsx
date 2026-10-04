import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { ContentReview, PressQuote } from '../../api/types'
import { PressSection } from './PressSection'
import { ReviewSection } from './ReviewSection'

const quotes: PressQuote[] = [
  { pressName: 'Tuổi Trẻ', logoUrl: null, quote: 'Sách bán chạy nhất', sourceUrl: 'https://tuoitre.vn' },
  { pressName: 'Thanh Niên', logoUrl: null, quote: 'Hành trình cuốn hút', sourceUrl: 'https://thanhnien.vn' },
  { pressName: 'VnExpress', logoUrl: null, quote: 'Nên đọc một lần', sourceUrl: null },
]

describe('PressSection', () => {
  // AC-4
  it('render đúng số trích dẫn và link mở tab mới an toàn', () => {
    render(<PressSection quotes={quotes} />)

    expect(screen.getByTestId('press-list').children).toHaveLength(3)
    expect(screen.getByText('Tuổi Trẻ')).toBeInTheDocument()

    const links = screen.getAllByRole('link', { name: /đọc bài gốc/i })
    expect(links).toHaveLength(2) // mục không có sourceUrl thì không render link
    for (const link of links) {
      expect(link).toHaveAttribute('target', '_blank')
      expect(link.getAttribute('rel')).toContain('noopener')
    }
  })

  it('không render gì khi không có trích dẫn nào', () => {
    const { container } = render(<PressSection quotes={[]} />)
    expect(container).toBeEmptyDOMElement()
  })
})

describe('ReviewSection', () => {
  const review: ContentReview = {
    title: 'Review nội dung',
    content: 'Một cuốn sách ngắn nhưng đọng.',
    fileUrl: '/uploads/review.pdf',
  }

  // AC-5
  it('hiện nút tải khi có fileUrl', () => {
    render(<ReviewSection review={review} />)

    const download = screen.getByTestId('review-download')
    expect(download).toHaveAttribute('href', '/uploads/review.pdf')
    expect(download).toHaveAttribute('download')
  })

  // AC-5
  it('ẩn nút tải khi fileUrl là null', () => {
    render(<ReviewSection review={{ ...review, fileUrl: null }} />)

    expect(screen.queryByTestId('review-download')).not.toBeInTheDocument()
  })
})
