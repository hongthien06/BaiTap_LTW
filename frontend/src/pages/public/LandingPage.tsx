import { useQuery } from '@tanstack/react-query'
import { useEffect } from 'react'
import { getErrorMessage } from '../../api/client'
import { publicApi } from '../../api/endpoints'
import type { LandingResponse } from '../../api/types'
import { effectivePrice } from '../../lib/format'
import { AuthorSection } from '../../components/landing/AuthorSection'
import { BookInfoSection } from '../../components/landing/BookInfoSection'
import { FeedbackSection } from '../../components/landing/FeedbackSection'
import { HeroSection } from '../../components/landing/HeroSection'
import { OrderFormSection } from '../../components/landing/OrderFormSection'
import { PressSection } from '../../components/landing/PressSection'
import { ReviewSection } from '../../components/landing/ReviewSection'
import { SiteFooter } from '../../components/landing/SiteFooter'

export function LandingPage() {
  const { data, isPending, error } = useQuery({
    queryKey: ['landing'],
    queryFn: publicApi.getLanding,
  })

  useEffect(() => {
    if (!data) return

    document.title = truncate(`${data.book.name} - ${data.book.subtitle}`, 60)
    setMeta('description', truncate(data.book.subtitle, 160))
    setMeta('og:title', data.book.title, 'property')
    setMeta('og:description', truncate(data.book.subtitle, 200), 'property')
    if (data.book.coverImageUrl) setMeta('og:image', data.book.coverImageUrl, 'property')

    setStructuredData(data)
  }, [data])

  if (isPending) {
    return <p className="p-10 text-center text-ink/60">Đang tải...</p>
  }

  if (error || !data) {
    return (
      <div className="p-10 text-center">
        <p className="text-red-600" role="alert">{getErrorMessage(error, 'Chưa có dữ liệu sách để hiển thị.')}</p>
      </div>
    )
  }

  const scrollToOrder = () => document.getElementById('dat-hang')?.scrollIntoView({ behavior: 'smooth' })

  return (
    <>
      <HeroSection book={data.book} onOrderClick={scrollToOrder} />
      <BookInfoSection book={data.book} />
      {data.author && <AuthorSection author={data.author} />}
      <PressSection quotes={data.pressQuotes} />
      {data.review && <ReviewSection review={data.review} />}
      <FeedbackSection summary={data.ratingSummary} feedbacks={data.feedbacks} />
      <OrderFormSection book={data.book} bankInfo={data.settings['payment.bankInfo']} />
      <SiteFooter settings={data.settings} />
    </>
  )
}

/** Title dai qua 60 ky tu bi Google cat; subtitle cua sach co the toi 500 ky tu. */
function truncate(text: string, max: number) {
  return text.length <= max ? text : `${text.slice(0, max - 1).trimEnd()}…`
}

/** NFR-7: JSON-LD schema.org Book + AggregateRating de ket qua tim kiem hien sao va gia. */
function setStructuredData(data: LandingResponse) {
  const jsonLd: Record<string, unknown> = {
    '@context': 'https://schema.org',
    '@type': 'Book',
    name: data.book.name,
    description: data.book.description,
    genre: data.book.category,
    image: data.book.coverImageUrl ?? undefined,
    author: data.author ? { '@type': 'Person', name: data.author.fullName } : undefined,
    offers: {
      '@type': 'Offer',
      price: effectivePrice(data.book.price, data.book.discountPrice),
      priceCurrency: 'VND',
      availability: 'https://schema.org/InStock',
    },
  }

  // Google tu choi AggregateRating khong co danh gia nao - chi chen khi thuc su co du lieu.
  if (data.ratingSummary.count > 0) {
    jsonLd.aggregateRating = {
      '@type': 'AggregateRating',
      ratingValue: data.ratingSummary.average,
      reviewCount: data.ratingSummary.count,
      bestRating: 5,
      worstRating: 1,
    }
  }

  const id = 'landing-structured-data'
  let script = document.getElementById(id) as HTMLScriptElement | null
  if (!script) {
    script = document.createElement('script')
    script.id = id
    script.type = 'application/ld+json'
    document.head.appendChild(script)
  }
  script.textContent = JSON.stringify(jsonLd)
}

function setMeta(name: string, content: string, attribute: 'name' | 'property' = 'name') {
  let tag = document.head.querySelector<HTMLMetaElement>(`meta[${attribute}="${name}"]`)
  if (!tag) {
    tag = document.createElement('meta')
    tag.setAttribute(attribute, name)
    document.head.appendChild(tag)
  }
  tag.content = content
}
