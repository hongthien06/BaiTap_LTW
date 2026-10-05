interface Props {
  value: number
  size?: 'sm' | 'md'
  onChange?: (value: number) => void
}

const STARS = [1, 2, 3, 4, 5]

/** Hien thi hoac chon so sao 1-5. Co onChange thi thanh input, khong thi chi de doc. */
export function StarRating({ value, size = 'sm', onChange }: Props) {
  const cls = size === 'md' ? 'text-3xl' : 'text-lg'

  if (!onChange) {
    return (
      <span className={`${cls} whitespace-nowrap text-primary`} aria-label={`${value} trên 5 sao`}>
        {STARS.map((star) => (
          <span key={star} aria-hidden="true" className={star <= Math.round(value) ? '' : 'text-foreground/25'}>
            ★
          </span>
        ))}
      </span>
    )
  }

  return (
    <span role="radiogroup" aria-label="Chọn số sao" className={`${cls} whitespace-nowrap`}>
      {STARS.map((star) => (
        <button
          key={star}
          type="button"
          role="radio"
          aria-checked={value === star}
          aria-label={`${star} sao`}
          onClick={() => onChange(star)}
          className={`rounded px-0.5 ${star <= value ? 'text-primary' : 'text-foreground/25'}`}
        >
          ★
        </button>
      ))}
    </span>
  )
}
