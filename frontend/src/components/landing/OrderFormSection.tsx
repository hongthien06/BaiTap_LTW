import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { getErrorMessage } from "../../api/client";
import { publicApi } from "../../api/endpoints";
import {
  PaymentMethod,
  type Book,
  type CreateOrderResponse,
} from "../../api/types";
import { effectivePrice, formatPrice } from "../../lib/format";
import { Button } from "../ui/Button";

const schema = z.object({
  customerName: z
    .string()
    .trim()
    .min(2, "Họ tên phải có ít nhất 2 ký tự")
    .max(200, "Họ tên tối đa 200 ký tự"),
  phone: z
    .string()
    .trim()
    .regex(/^0\d{9}$/, "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)"),
  address: z
    .string()
    .trim()
    .min(10, "Địa chỉ phải có ít nhất 10 ký tự")
    .max(500, "Địa chỉ tối đa 500 ký tự"),
  quantity: z.coerce
    .number()
    .int()
    .min(1, "Số lượng từ 1 đến 99")
    .max(99, "Số lượng từ 1 đến 99"),
  paymentMethod: z.coerce
    .number()
    .refine(
      (v) => v === PaymentMethod.Cod || v === PaymentMethod.BankTransfer,
      "Phương thức thanh toán không hợp lệ",
    ),
  note: z.string().trim().max(1000, "Ghi chú tối đa 1000 ký tự").optional(),
});

type FormValues = z.input<typeof schema>;

interface Props {
  book: Book;
  bankInfo?: string;
}

const FIELD =
  "h-11 w-full rounded-lg border border-border-strong bg-surface px-3 placeholder:text-muted";

export function OrderFormSection({ book, bankInfo }: Props) {
  const [result, setResult] = useState<CreateOrderResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  const { register, handleSubmit, watch, setValue, reset, formState } =
    useForm<FormValues>({
      resolver: zodResolver(schema),
      defaultValues: {
        customerName: "",
        phone: "",
        address: "",
        quantity: 1,
        paymentMethod: PaymentMethod.Cod,
        note: "",
      },
    });

  const mutation = useMutation({
    mutationFn: publicApi.createOrder,
    onSuccess: (data) => {
      setResult(data);
      setError(null);
      reset();
    },
    onError: (err) => {
      setError(getErrorMessage(err));
      setResult(null);
    },
  });

  const quantity = Number(watch("quantity")) || 0;
  const paymentMethod = Number(watch("paymentMethod"));
  const unitPrice = effectivePrice(book.price, book.discountPrice);

  const step = (delta: number) => {
    const next = Math.min(
      99,
      Math.max(1, (Number(watch("quantity")) || 1) + delta),
    );
    setValue("quantity", next, { shouldValidate: true });
  };

  if (result) {
    return (
      <section
        id="dat-hang"
        aria-labelledby="order-success-title"
        className="rounded-card border border-primary bg-primary-light p-8 md:p-12"
      >
        <div
          className="mx-auto max-w-xl text-center"
          data-testid="order-success"
        >
          <h2 id="order-success-title" className="font-display text-3xl">
            Cảm ơn bạn đã đặt hàng!
          </h2>

          <p className="mt-5 text-lg">
            Mã đơn của bạn là{" "}
            <strong
              className="font-semibold text-primary"
              data-testid="order-code"
            >
              {result.orderCode}
            </strong>
          </p>

          <dl className="mt-6 grid gap-2 rounded-lg border border-border bg-surface p-5 text-left">
            <div className="flex items-baseline justify-between">
              <dt className="text-muted">Tổng tiền</dt>
              <dd className="text-xl font-semibold text-primary">
                {formatPrice(result.totalPrice)}
              </dd>
            </div>
            <div className="flex items-baseline justify-between">
              <dt className="text-muted">Đơn giá</dt>
              <dd>{formatPrice(result.unitPrice)}</dd>
            </div>
          </dl>

          <p className="mt-5 text-muted">
            Chúng tôi sẽ gọi theo số điện thoại bạn cung cấp để xác nhận đơn.
          </p>

          <Button
            variant="outline"
            className="mt-7"
            onClick={() => setResult(null)}
          >
            Đặt thêm đơn khác
          </Button>
        </div>
      </section>
    );
  }

  return (
    <section
      id="dat-hang"
      aria-labelledby="order-title"
      className="rounded-card border border-border bg-surface p-6 md:p-8"
    >
      <h2 id="order-title" className="font-display text-2xl md:text-3xl">
        Đặt mua sách
      </h2>
      <p className="mt-1 text-muted">
        Điền thông tin bên dưới, chúng tôi gọi xác nhận trước khi giao.
      </p>

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
        className="mt-6 grid max-w-3xl gap-5 sm:grid-cols-2"
        noValidate
      >
        <label className="block">
          <span className="mb-1.5 block text-[13px] font-medium">Họ tên</span>
          <input {...register("customerName")} className={FIELD} />
          {formState.errors.customerName && (
            <span className="mt-1.5 block text-[13px] text-danger">
              {formState.errors.customerName.message}
            </span>
          )}
        </label>

        <label className="block">
          <span className="mb-1.5 block text-[13px] font-medium">
            Số điện thoại
          </span>
          <input inputMode="tel" {...register("phone")} className={FIELD} />
          {formState.errors.phone && (
            <span className="mt-1.5 block text-[13px] text-danger">
              {formState.errors.phone.message}
            </span>
          )}
        </label>

        <label className="block sm:col-span-2">
          <span className="mb-1.5 block text-[13px] font-medium">
            Địa chỉ nhận hàng
          </span>
          <input {...register("address")} className={FIELD} />
          {formState.errors.address && (
            <span className="mt-1.5 block text-[13px] text-danger">
              {formState.errors.address.message}
            </span>
          )}
        </label>

        {/* htmlFor tường minh: <label> bọc nhiều control thì nó gắn vào control ĐẦU TIÊN
            (ở đây là nút trừ), ô nhập sẽ mất nhãn. */}
        <div className="block">
          <label
            htmlFor="order-quantity"
            className="mb-1.5 block text-[13px] font-medium"
          >
            Số lượng
          </label>
          <span className="flex h-11 w-36 items-center rounded-lg border border-border-strong bg-surface">
            <button
              type="button"
              aria-label="Bớt một cuốn"
              onClick={() => step(-1)}
              className="grid h-full w-10 place-items-center text-muted hover:text-foreground"
            >
              −
            </button>
            <input
              id="order-quantity"
              type="number"
              min={1}
              max={99}
              {...register("quantity")}
              className="h-full w-full min-w-0 border-x border-border-strong bg-transparent text-center font-medium [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none"
            />
            <button
              type="button"
              aria-label="Thêm một cuốn"
              onClick={() => step(1)}
              className="grid h-full w-10 place-items-center text-muted hover:text-foreground"
            >
              +
            </button>
          </span>
          {formState.errors.quantity && (
            <span className="mt-1.5 block text-[13px] text-danger">
              {formState.errors.quantity.message}
            </span>
          )}
        </div>

        <fieldset className="block">
          <legend className="mb-1.5 block text-[13px] font-medium">
            Thanh toán
          </legend>
          <div className="flex gap-2">
            {[PaymentMethod.Cod, PaymentMethod.BankTransfer].map((method) => {
              const active = paymentMethod === method;
              return (
                <label
                  key={method}
                  className={`flex h-11 flex-1 cursor-pointer items-center gap-2 rounded-lg border px-3 text-[14px] ${
                    active
                      ? "border-primary bg-primary-light font-medium"
                      : "border-border-strong bg-surface"
                  }`}
                >
                  <input
                    type="radio"
                    value={method}
                    {...register("paymentMethod")}
                    className="accent-primary"
                  />
                  <span className="truncate">
                    {method === PaymentMethod.Cod ? "COD" : "Chuyển khoản"}
                  </span>
                </label>
              );
            })}
          </div>
        </fieldset>

        {paymentMethod === PaymentMethod.BankTransfer && bankInfo && (
          <p className="rounded-lg border border-primary/40 bg-primary-light p-3 text-[14px] sm:col-span-2">
            Thông tin chuyển khoản: {bankInfo}
          </p>
        )}

        <label className="block sm:col-span-2">
          <span className="mb-1.5 block text-[13px] font-medium">
            Ghi chú (không bắt buộc)
          </span>
          <textarea
            rows={3}
            {...register("note")}
            className="w-full rounded-lg border border-border-strong bg-surface px-3 py-2 placeholder:text-muted"
          />
        </label>

        <div className="border-t border-border pt-5 sm:col-span-2">
          <p
            className="flex items-baseline justify-between"
            data-testid="order-total"
          >
            <span className="text-muted">
              Tạm tính · {quantity} cuốn × {formatPrice(unitPrice)}
            </span>
            <strong className="text-2xl text-primary">
              {formatPrice(unitPrice * quantity)}
            </strong>
          </p>
          <p className="mt-1 text-[13px] text-muted">
            Giá cuối do hệ thống xác nhận khi tạo đơn.
          </p>

          <Button
            type="submit"
            size="lg"
            className="mt-5 w-full sm:w-auto"
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Đang gửi..." : "Xác nhận đặt hàng"}
          </Button>

          <p className="mt-3 text-[13px] text-muted">
            Gọi xác nhận trước khi giao · Kiểm hàng rồi mới trả tiền
          </p>

          {error && (
            <p className="mt-3 text-[14px] text-danger" role="alert">
              {error}
            </p>
          )}
        </div>
      </form>
    </section>
  );
}
