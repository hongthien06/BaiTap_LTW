import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { getErrorMessage } from '../../api/client'
import { publicApi } from '../../api/endpoints'
import { PAYMENT_METHOD_LABEL, PaymentMethod, type Book, type CreateOrderResponse } from '../../api/types'
import { effectivePrice, formatPrice } from '../../lib/format'

// Phai khop CreateOrderRequestValidator o backend.
const schema = z.object({
  customerName: z.string().trim().min(2, 'Họ tên phải có ít nhất 2 ký tự').max(200, 'Họ tên tối đa 200 ký tự'),
  phone: z.string().trim().regex(/^0\d{9}$/, 'Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)'),
  address: z.string().trim().min(10, 'Địa chỉ phải có ít nhất 10 ký tự').max(500, 'Địa chỉ tối đa 500 ký tự'),
  quantity: z.coerce.number().int().min(1, 'Số lượng từ 1 đến 99').max(99, 'Số lượng từ 1 đến 99'),
  paymentMethod: z.coerce.number().refine(
    (v) => v === PaymentMethod.Cod || v === PaymentMethod.BankTransfer,
    'Phương thức thanh toán không hợp lệ',
  ),
  note: z.string().trim().max(1000, 'Ghi chú tối đa 1000 ký tự').optional(),
})

type FormValues = z.input<typeof schema>

interface Props {
  book: Book
  bankInfo?: string
}

export function OrderFormSection({ book, bankInfo }: Props) {
  const [result, setResult] = useState<CreateOrderResponse | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { register, handleSubmit, watch, reset, formState } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { customerName: '', phone: '', address: '', quantity: 1, paymentMethod: PaymentMethod.Cod, note: '' },
  })

  const mutation = useMutation({
    mutationFn: publicApi.createOrder,
    onSuccess: (data) => {
      setResult(data)
      setError(null)
      // Reset de F5 khong gui lai don (AC-12).
      reset()
    },
    onError: (err) => {
      setError(getErrorMessage(err))
      setResult(null)
    },
  })

  const quantity = Number(watch('quantity')) || 0
  const unitPrice = effectivePrice(book.price, book.discountPrice)
  const paymentMethod = Number(watch('paymentMethod'))

  if (result) {
    return (
      <section id="dat-hang" className="bg-ink py-16 text-sand" aria-labelledby="order-success-title">
        <div className="mx-auto max-w-2xl px-4 text-center" data-testid="order-success">
          <h2 id="order-success-title" className="font-display text-3xl">Cảm ơn bạn đã đặt hàng!</h2>
          <p className="mt-4 text-lg">
            Mã đơn của bạn là{' '}
            <strong className="text-gold" data-testid="order-code">{result.orderCode}</strong>
          </p>
          <p className="mt-2 text-sand/80">
            Tổng tiền: <strong>{formatPrice(result.totalPrice)}</strong>
          </p>
          <p className="mt-4 text-sm text-sand/70">
            Chúng tôi sẽ liên hệ theo số điện thoại bạn cung cấp để xác nhận đơn.
          </p>
          <button
            type="button"
            onClick={() => setResult(null)}
            className="mt-8 rounded-full border border-gold px-6 py-2 font-semibold text-gold"
          >
            Đặt thêm đơn khác
          </button>
        </div>
      </section>
    )
  }

  return (
    <section id="dat-hang" className="bg-ink py-16 text-sand" aria-labelledby="order-title">
      <div className="mx-auto max-w-3xl px-4">
        <h2 id="order-title" className="font-display text-3xl">Đặt mua sách</h2>
        <p className="mt-2 text-sand/70">Điền thông tin bên dưới, chúng tôi sẽ gọi xác nhận trước khi giao.</p>

        <form
          onSubmit={handleSubmit((values) =>
            mutation.mutate({
              customerName: values.customerName,
              phone: values.phone,
              address: values.address,
              quantity: Number(values.quantity),
              paymentMethod: Number(values.paymentMethod) as 0 | 1,
              note: values.note || null,
            }),
          )}
          className="mt-8 grid gap-5 md:grid-cols-2"
          noValidate
        >
          <div>
            <label className="block text-sm font-medium" htmlFor="order-name">Họ tên</label>
            <input id="order-name" {...register('customerName')} className="mt-1 w-full rounded border border-sand/30 bg-transparent px-3 py-2" />
            {formState.errors.customerName && <p className="mt-1 text-sm text-red-400">{formState.errors.customerName.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium" htmlFor="order-phone">Số điện thoại</label>
            <input id="order-phone" inputMode="tel" {...register('phone')} className="mt-1 w-full rounded border border-sand/30 bg-transparent px-3 py-2" />
            {formState.errors.phone && <p className="mt-1 text-sm text-red-400">{formState.errors.phone.message}</p>}
          </div>

          <div className="md:col-span-2">
            <label className="block text-sm font-medium" htmlFor="order-address">Địa chỉ nhận hàng</label>
            <input id="order-address" {...register('address')} className="mt-1 w-full rounded border border-sand/30 bg-transparent px-3 py-2" />
            {formState.errors.address && <p className="mt-1 text-sm text-red-400">{formState.errors.address.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium" htmlFor="order-quantity">Số lượng</label>
            <input id="order-quantity" type="number" min={1} max={99} {...register('quantity')} className="mt-1 w-full rounded border border-sand/30 bg-transparent px-3 py-2" />
            {formState.errors.quantity && <p className="mt-1 text-sm text-red-400">{formState.errors.quantity.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium" htmlFor="order-payment">Phương thức thanh toán</label>
            <select id="order-payment" {...register('paymentMethod')} className="mt-1 w-full rounded border border-sand/30 bg-ink px-3 py-2">
              <option value={PaymentMethod.Cod}>{PAYMENT_METHOD_LABEL[PaymentMethod.Cod]}</option>
              <option value={PaymentMethod.BankTransfer}>{PAYMENT_METHOD_LABEL[PaymentMethod.BankTransfer]}</option>
            </select>
          </div>

          {paymentMethod === PaymentMethod.BankTransfer && bankInfo && (
            <p className="rounded border border-gold/40 p-3 text-sm text-sand/80 md:col-span-2">
              Thông tin chuyển khoản: {bankInfo}
            </p>
          )}

          <div className="md:col-span-2">
            <label className="block text-sm font-medium" htmlFor="order-note">Ghi chú (không bắt buộc)</label>
            <textarea id="order-note" rows={3} {...register('note')} className="mt-1 w-full rounded border border-sand/30 bg-transparent px-3 py-2" />
          </div>

          <p className="md:col-span-2" data-testid="order-total">
            Tạm tính: <strong className="text-gold">{formatPrice(unitPrice * quantity)}</strong>
            <span className="ml-2 text-sm text-sand/60">(giá cuối do hệ thống xác nhận)</span>
          </p>

          <button
            type="submit"
            disabled={mutation.isPending}
            className="rounded-full bg-gold px-8 py-3 font-semibold text-ink disabled:opacity-60 md:col-span-2"
          >
            {mutation.isPending ? 'Đang gửi...' : 'Xác nhận đặt hàng'}
          </button>

          {error && <p className="text-sm text-red-400 md:col-span-2" role="alert">{error}</p>}
        </form>
      </div>
    </section>
  )
}
