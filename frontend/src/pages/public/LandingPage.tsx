import { useQuery } from '@tanstack/react-query'
import { useEffect } from 'react'
import { getErrorMessage } from '../../api/client'
import { publicApi } from '../../api/endpoints'
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
    document.title = `${data.book.name} - ${data.book.subtitle}`
    setMeta('description', data.book.subtitle)
    setMeta('og:title', data.book.title, 'property')
    setMeta('og:description', data.book.subtitle, 'property')
    if (data.book.coverImageUrl) setMeta('og:image', data.book.coverImageUrl, 'property')
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

function setMeta(name: string, content: string, attribute: 'name' | 'property' = 'name') {
  let tag = document.head.querySelector<HTMLMetaElement>(`meta[${attribute}="${name}"]`)
  if (!tag) {
    tag = document.createElement('meta')
    tag.setAttribute(attribute, name)
    document.head.appendChild(tag)
  }
  tag.content = content
}
