import { safeHref } from '../../lib/url'

interface Props {
  settings: Record<string, string>
}

export function SiteFooter({ settings }: Props) {
  const hotline = settings['contact.hotline']
  const email = settings['contact.email']
  const address = settings['contact.address']
  const facebook = settings['social.facebook']
  const youtube = settings['social.youtube']
  const footerText = settings['footer.text']

  return (
    <footer className="bg-ink py-12 text-sand/80" aria-label="Chân trang">
      <div className="mx-auto grid max-w-6xl gap-8 px-4 md:grid-cols-3">
        <div>
          <h2 className="font-display text-xl text-sand">Liên hệ</h2>
          {hotline && <p className="mt-3" data-testid="footer-hotline">Hotline: {hotline}</p>}
          {email && <p className="mt-1">Email: {email}</p>}
          {address && <p className="mt-1">{address}</p>}
        </div>

        <div>
          <h2 className="font-display text-xl text-sand">Theo dõi</h2>
          <ul className="mt-3 space-y-1">
            {safeHref(facebook) && (
              <li><a href={safeHref(facebook)} target="_blank" rel="noopener noreferrer" className="underline">Facebook</a></li>
            )}
            {safeHref(youtube) && (
              <li><a href={safeHref(youtube)} target="_blank" rel="noopener noreferrer" className="underline">YouTube</a></li>
            )}
          </ul>
        </div>

        <div>
          <h2 className="font-display text-xl text-sand">Liên kết nhanh</h2>
          <ul className="mt-3 space-y-1">
            <li><a href="#thong-tin-sach" className="underline">Về cuốn sách</a></li>
            <li><a href="#tac-gia" className="underline">Tác giả</a></li>
            <li><a href="#danh-gia" className="underline">Đánh giá</a></li>
            <li><a href="#dat-hang" className="underline">Đặt hàng</a></li>
          </ul>
        </div>
      </div>

      {footerText && <p className="mt-10 text-center text-sm text-sand/50">{footerText}</p>}
    </footer>
  )
}
