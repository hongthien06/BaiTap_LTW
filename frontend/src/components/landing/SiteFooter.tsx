import { safeHref } from '../../lib/url'

interface Props {
  settings: Record<string, string>
}

export function SiteFooter({ settings }: Props) {
  const hotline = settings['contact.hotline']
  const email = settings['contact.email']
  const address = settings['contact.address']
  const facebook = safeHref(settings['social.facebook'])
  const youtube = safeHref(settings['social.youtube'])
  const footerText = settings['footer.text']

  return (
    <footer className="rounded-card border border-border bg-surface p-6 md:p-8" aria-label="Chân trang">
      <div className="grid gap-8 md:grid-cols-3">
        <div>
          <h2 className="font-display text-xl">Liên hệ</h2>
          <ul className="mt-3 grid gap-1 text-muted">
            {hotline && <li data-testid="footer-hotline">Hotline: {hotline}</li>}
            {email && <li>{email}</li>}
            {address && <li>{address}</li>}
          </ul>
        </div>

        <div>
          <h2 className="font-display text-xl">Theo dõi</h2>
          <ul className="mt-3 grid gap-1">
            {facebook && (
              <li><a href={facebook} target="_blank" rel="noopener noreferrer" className="underline underline-offset-4">Facebook</a></li>
            )}
            {youtube && (
              <li><a href={youtube} target="_blank" rel="noopener noreferrer" className="underline underline-offset-4">YouTube</a></li>
            )}
          </ul>
        </div>

        <div>
          <h2 className="font-display text-xl">Liên kết nhanh</h2>
          <ul className="mt-3 grid gap-1">
            <li><a href="#thong-tin-sach" className="underline underline-offset-4">Về cuốn sách</a></li>
            <li><a href="#tac-gia" className="underline underline-offset-4">Tác giả</a></li>
            <li><a href="#danh-gia" className="underline underline-offset-4">Đánh giá</a></li>
            <li><a href="#dat-hang" className="underline underline-offset-4">Đặt hàng</a></li>
          </ul>
        </div>
      </div>

      {footerText && (
        <p className="mt-8 border-t border-border pt-5 text-center text-[13px] text-muted">{footerText}</p>
      )}
    </footer>
  )
}
