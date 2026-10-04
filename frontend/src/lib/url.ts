/**
 * Loc URL truoc khi dat vao thuoc tinh href.
 *
 * Cac truong nhu sourceUrl cua bao chi, fileUrl cua review hay link mang xa hoi deu do admin
 * nhap. Mot chuoi dang javascript:... se duoc trinh duyet thuc thi khi khach bam vao - stored
 * XSS pham vi cong khai. Backend da validate, day la lop chan thu hai cho du lieu cu.
 */
export function safeHref(url: string | null | undefined): string | undefined {
  if (!url) return undefined

  // Duong dan noi bo, vi du /uploads/abc.pdf. Loai bo //evil.com (protocol-relative).
  if (url.startsWith('/')) return url.startsWith('//') ? undefined : url

  try {
    const parsed = new URL(url)
    return parsed.protocol === 'http:' || parsed.protocol === 'https:' ? url : undefined
  } catch {
    return undefined
  }
}
