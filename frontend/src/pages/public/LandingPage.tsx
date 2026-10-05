import { useQuery } from '@tanstack/react-query'
import { useEffect } from 'react'
import { getErrorMessage } from '../../api/client'
import { publicApi } from '../../api/endpoints'
import type { LandingResponse } from '../../api/types'
import { AuthorSection } from '../../components/landing/AuthorSection'
import { BookInfoSection } from '../../components/landing/BookInfoSection'
import { CtaBanner } from '../../components/landing/CtaBanner'
import { FeedbackSection } from '../../components/landing/FeedbackSection'
import { HeroSection } from '../../components/landing/HeroSection'
import { OrderFormSection } from '../../components/landing/OrderFormSection'
import { PressSection } from '../../components/landing/PressSection'
import { ReviewSection } from '../../components/landing/ReviewSection'
import { SiteFooter } from '../../components/landing/SiteFooter'
import { StickyBuyBar } from '../../components/landing/StickyBuyBar'
import { effectivePrice } from '../../lib/format'

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

  if (isPending) return <LoadingSkeleton />

  if (error || !data) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-24 text-center">
        <h1 className="font-display text-2xl">Chưa hiển thị được trang</h1>
        <p className="mt-3 text-muted" role="alert">
          {getErrorMessage(error, 'Chưa có dữ liệu sách để hiển thị.')}
        </p>
      </div>
    )
  }

  const scrollTo = (id: string) => document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
  const toOrder = () => scrollTo('dat-hang')

  return (
    <div className="mx-auto grid max-w-6xl gap-4 px-4 py-4 md:gap-5 md:py-8">
      <HeroSection
        book={data.book}
        average={data.ratingSummary.average}
        reviewCount={data.ratingSummary.count}
        onOrderClick={toOrder}
        onReviewClick={() => scrollTo('review')}
      />

      <BookInfoSection book={data.book} />

      <CtaBanner price={effectivePrice(data.book.price, data.book.discountPrice)} onOrderClick={toOrder} />

      {data.author && <AuthorSection author={data.author} />}
      <PressSection quotes={data.pressQuotes} />
      {data.review && <ReviewSection review={data.review} />}

      <FeedbackSection summary={data.ratingSummary} feedbacks={data.feedbacks} />

      <OrderFormSection book={data.book} bankInfo={data.settings['payment.bankInfo']} />

      <SiteFooter settings={data.settings} />

      <StickyBuyBar book={data.book} onOrderClick={toOrder} />
    </div>
  )
}

/** Khung chờ đúng hình các khối thật, để trang không nhảy khi dữ liệu về. */
function LoadingSkeleton() {
  return (
    <div className="mx-auto grid max-w-6xl gap-4 px-4 py-4 md:py-8" aria-busy="true" aria-label="Đang tải">
      <div className="h-[420px] animate-pulse rounded-card border border-border bg-surface" />
      <div className="h-56 animate-pulse rounded-card border border-border bg-surface" />
      <div className="h-32 animate-pulse rounded-card border border-border bg-surface" />
      <div className="h-72 animate-pulse rounded-card border border-border bg-surface" />
    </div>
  )
}

/** Title dài quá 60 ký tự bị Google cắt; subtitle của sách có thể tới 500 ký tự. */
function truncate(text: string, max: number) {
  return text.length <= max ? text : `${text.slice(0, max - 1).trimEnd()}…`
}

/** NFR-7: JSON-LD schema.org Book + AggregateRating. */
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
