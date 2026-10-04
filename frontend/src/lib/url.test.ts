import { describe, expect, it } from 'vitest'
import { safeHref } from './url'

describe('safeHref', () => {
  it('cho qua http và https', () => {
    expect(safeHref('https://tuoitre.vn/bai-viet')).toBe('https://tuoitre.vn/bai-viet')
    expect(safeHref('http://example.com')).toBe('http://example.com')
  })

  it('cho qua đường dẫn nội bộ', () => {
    expect(safeHref('/uploads/review.pdf')).toBe('/uploads/review.pdf')
  })

  // Chinh la lo hong stored XSS: admin luu javascript:... vao link bao chi.
  it('chặn javascript: và các scheme nguy hiểm', () => {
    expect(safeHref('javascript:alert(1)')).toBeUndefined()
    expect(safeHref('JavaScript:alert(1)')).toBeUndefined()
    expect(safeHref('data:text/html,<script>alert(1)</script>')).toBeUndefined()
    expect(safeHref('vbscript:msgbox(1)')).toBeUndefined()
  })

  it('chặn URL protocol-relative', () => {
    expect(safeHref('//evil.com/phish')).toBeUndefined()
  })

  it('trả undefined cho giá trị rỗng hoặc không parse được', () => {
    expect(safeHref(null)).toBeUndefined()
    expect(safeHref('')).toBeUndefined()
    expect(safeHref('khong-phai-url')).toBeUndefined()
  })
})
