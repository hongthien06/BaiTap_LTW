interface Props {
  value: number
  size?: 'sm' | 'md'
  onChange?: (value: number) => void
}

/** Hien thi hoac chon so sao 1-5. Co onChange thi thanh input, khong thi chi de doc. */
export function StarRating({ value, size = 'sm', onChange }: Props) {
  const stars = [1, 2, 3, 4, 5]
  const className = size === 'md' ? 'text-3xl' : 'text-lg'

  if (!onChange) {
    return (
      <span className={`${className} text-gold`} aria-label={`${value} trên 5 sao`}>
        {stars.map((star) => (
          <span key={star} aria-hidden="true">{star <= Math.round(value) ? '★' : '☆'}</span>
        ))}
      </span>
    )
  }

  return (
    <span role="radiogroup" aria-label="Chọn số sao" className={className}>
      {stars.map((star) => (
        <button
          key={star}
          type="button"
          role="radio"
          aria-checked={value === star}
          aria-label={`${star} sao`}
          onClick={() => onChange(star)}
          className={star <= value ? 'text-gold' : 'text-ink/25'}
        >
          ★
        </button>
      ))}
    </span>
  )
}
