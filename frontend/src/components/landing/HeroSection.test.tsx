import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { Book } from '../../api/types'
import { HeroSection } from './HeroSection'

const book: Book = {
  id: 1,
  name: 'Nhà Giả Kim',
  category: 'Tiểu thuyết',
  title: 'Nhà Giả Kim',
  subtitle: 'Khi bạn khao khát một điều gì đó, cả vũ trụ sẽ hợp lực giúp bạn.',
  description: 'Mô tả',
  price: 89000,
  discountPrice: 69000,
  coverImageUrl: '/img/cover.jpg',
  mockupImageUrl: '/img/mockup.png',
  images: [],
}

describe('HeroSection', () => {
  // AC-2
  it('hiển thị title, subtitle, ảnh mockup và giá', () => {
    render(<HeroSection book={book} onOrderClick={vi.fn()} />)

    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Nhà Giả Kim')
    expect(screen.getByText(book.subtitle)).toBeInTheDocument()
    expect(screen.getByRole('img', { name: /ảnh bìa sách/i })).toHaveAttribute('src', '/img/mockup.png')
    expect(screen.getByTestId('sell-price')).toHaveTextContent('69.000')
  })

  // AC-2: có giảm giá thì giá gốc phải gạch ngang
  it('gạch ngang giá gốc khi có giá giảm', () => {
    render(<HeroSection book={book} onOrderClick={vi.fn()} />)

    const original = screen.getByTestId('original-price')
    expect(original.tagName).toBe('S')
    expect(original).toHaveTextContent('89.000')
    expect(screen.getByTestId('discount-badge')).toHaveTextContent('-22%')
  })

  // AC-3
  it('không hiện giá gạch ngang và badge khi không có giá giảm', () => {
    render(<HeroSection book={{ ...book, discountPrice: null }} onOrderClick={vi.fn()} />)

    expect(screen.queryByTestId('original-price')).not.toBeInTheDocument()
    expect(screen.queryByTestId('discount-badge')).not.toBeInTheDocument()
    expect(screen.getByTestId('sell-price')).toHaveTextContent('89.000')
  })

  // AC-3: giá giảm không hợp lệ (>= giá gốc) thì coi như không giảm
  it('bỏ qua giá giảm lớn hơn hoặc bằng giá gốc', () => {
    render(<HeroSection book={{ ...book, discountPrice: 99000 }} onOrderClick={vi.fn()} />)

    expect(screen.queryByTestId('discount-badge')).not.toBeInTheDocument()
    expect(screen.getByTestId('sell-price')).toHaveTextContent('89.000')
  })

  it('gọi onOrderClick khi bấm Đặt mua ngay', async () => {
    const onOrderClick = vi.fn()
    render(<HeroSection book={book} onOrderClick={onOrderClick} />)

    await userEvent.click(screen.getByRole('button', { name: /đặt mua ngay/i }))

    expect(onOrderClick).toHaveBeenCalledOnce()
  })
})
