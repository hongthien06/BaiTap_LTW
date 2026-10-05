import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Empty, Input, Segmented, Skeleton, Table, Tag, message } from 'antd'
import { useState } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi, type OrderFilter } from '../../api/endpoints'
import {
  ORDER_NEXT_STATUSES, ORDER_STATUS_LABEL, OrderStatus, PAYMENT_METHOD_SHORT,
  type AdminOrder, type OrderStatusValue,
} from '../../api/types'
import { formatDateTime, formatPrice } from '../../lib/format'

const STATUS_COLOR: Record<OrderStatusValue, string> = {
  [OrderStatus.New]: 'blue',
  [OrderStatus.Confirmed]: 'cyan',
  [OrderStatus.Shipping]: 'orange',
  [OrderStatus.Completed]: 'green',
  [OrderStatus.Cancelled]: 'red',
}

/**
 * Đơn "Mới" để quá 4 giờ mà chưa xác nhận = quá hạn xử lý.
 * Trạng thái này KHÔNG nằm trong database (ở đó chỉ có New/Confirmed/...), nó suy từ giờ
 * hiện tại — mà đây lại đúng là việc chính của nhân viên: gọi những đơn để lâu chưa ai đụng.
 */
const OVERDUE_HOURS = 4
const isOverdue = (o: AdminOrder) =>
  o.status === OrderStatus.New && Date.now() - new Date(o.createdAt).getTime() > OVERDUE_HOURS * 3600_000

type QuickFilter = 'all' | 'overdue' | OrderStatusValue

export function OrdersPage() {
  const [filter, setFilter] = useState<OrderFilter>({ page: 1, pageSize: 10 })
  const [quick, setQuick] = useState<QuickFilter>('all')
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const queryClient = useQueryClient()
  const [toast, contextHolder] = message.useMessage()

  const { data, isPending, isError, error, refetch } = useQuery({
    queryKey: ['admin', 'orders', filter],
    queryFn: () => adminApi.getOrders(filter),
  })

  const updateStatus = useMutation({
    mutationFn: ({ id, status }: { id: number; status: OrderStatusValue }) =>
      adminApi.updateOrderStatus(id, status),
    onSuccess: () => {
      toast.success('Đã cập nhật trạng thái đơn.')
      queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] })
    },
    onError: (err) => toast.error(getErrorMessage(err)),
  })

  const rows = (data?.items ?? []).filter((o) =>
    quick === 'all' ? true : quick === 'overdue' ? isOverdue(o) : o.status === quick,
  )
  const selected = rows.find((o) => o.id === selectedId) ?? rows[0] ?? null
  const overdueCount = (data?.items ?? []).filter(isOverdue).length

  const statusTag = (o: AdminOrder) =>
    isOverdue(o)
      ? <Tag color="red">Quá hạn xử lý</Tag>
      : <Tag color={STATUS_COLOR[o.status]}>{ORDER_STATUS_LABEL[o.status]}</Tag>

  // Panel bên phải giữ địa chỉ, số lượng, hình thức thanh toán nên bảng chỉ cần 5 cột —
  // vừa khung ~664px ở 1280 (sidebar 232 + panel 320), không phải cuộn ngang.
  const columns = [
    {
      title: 'Mã đơn', dataIndex: 'orderCode', key: 'orderCode',
      render: (v: string) => <span className="whitespace-nowrap font-medium">{v}</span>,
    },
    {
      title: 'Khách hàng', key: 'customer',
      render: (_: unknown, o: AdminOrder) => (
        <div className="min-w-0">
          <div className="truncate">{o.customerName}</div>
          <div className="text-[13px] text-muted">{o.phone}</div>
        </div>
      ),
    },
    {
      title: 'Tổng tiền', dataIndex: 'totalPrice', key: 'totalPrice', align: 'right' as const,
      render: (v: number) => <span className="whitespace-nowrap font-medium">{formatPrice(v)}</span>,
    },
    { title: 'Trạng thái', key: 'status', render: (_: unknown, o: AdminOrder) => statusTag(o) },
    {
      title: 'Ngày đặt', dataIndex: 'createdAt', key: 'createdAt',
      render: (v: string) => <span className="whitespace-nowrap text-[13px] text-muted">{formatDateTime(v)}</span>,
    },
  ]

  return (
    <>
      {contextHolder}

      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-2xl">Đơn đặt hàng</h1>
          <p className="mt-1 text-[14px] text-muted">
            {overdueCount > 0 && <span className="font-medium text-danger">{overdueCount} đơn quá hạn xử lý · </span>}
            {data?.totalCount ?? 0} đơn tất cả
          </p>
        </div>
      </div>

      <div className="mt-4 flex flex-wrap items-center gap-3 rounded-card border border-border bg-surface p-3">
        <Input.Search
          allowClear
          placeholder="Tìm mã đơn hoặc số điện thoại"
          className="min-w-[220px] flex-1"
          onSearch={(phone) => setFilter((f) => ({ ...f, phone: phone || undefined, page: 1 }))}
        />
        {/* Khung cuộn riêng: 6 chip không vừa 375px, nhưng không được đẩy tràn cả trang. */}
        <div className="-mx-1 max-w-full overflow-x-auto px-1">
        <Segmented<QuickFilter>
          value={quick}
          onChange={setQuick}
          options={[
            { label: 'Tất cả', value: 'all' },
            { label: `Quá hạn${overdueCount ? ` (${overdueCount})` : ''}`, value: 'overdue' },
            { label: 'Mới', value: OrderStatus.New },
            { label: 'Đã xác nhận', value: OrderStatus.Confirmed },
            { label: 'Đang giao', value: OrderStatus.Shipping },
            { label: 'Hoàn thành', value: OrderStatus.Completed },
          ]}
        />
        </div>
      </div>

      {isPending ? (
        <div className="mt-4 rounded-card border border-border bg-surface p-5">
          <Skeleton active paragraph={{ rows: 6 }} />
        </div>
      ) : isError ? (
        <div className="mt-4 grid place-items-center rounded-card border border-danger/30 bg-danger-bg/40 px-6 py-16 text-center">
          <p className="font-medium">Không tải được danh sách đơn</p>
          <p className="mt-1 max-w-sm text-[14px] text-muted">{getErrorMessage(error)}</p>
          <Button className="mt-5" onClick={() => refetch()}>Thử lại</Button>
        </div>
      ) : rows.length === 0 ? (
        <div className="mt-4 rounded-card border border-border bg-surface px-6 py-16">
          <Empty description="Chưa có đơn nào khớp bộ lọc">
            <Button onClick={() => { setQuick('all'); setFilter({ page: 1, pageSize: 10 }) }}>Xoá bộ lọc</Button>
          </Empty>
        </div>
      ) : (
        <div className="mt-4 grid gap-4 xl:grid-cols-[minmax(0,1fr)_320px]">
          {/* Mobile: mỗi đơn một thẻ. Bảng cuộn ngang trên điện thoại là không dùng được. */}
          <ul className="grid gap-2 md:hidden">
            {rows.map((o) => (
              <li key={o.id}>
                <button
                  type="button"
                  onClick={() => setSelectedId(o.id)}
                  className={`w-full rounded-card border bg-surface p-4 text-left ${
                    isOverdue(o) ? 'border-danger/40' : 'border-border'
                  }`}
                >
                  <div className="flex items-center justify-between gap-3">
                    <span className="font-medium">{o.orderCode}</span>
                    {statusTag(o)}
                  </div>
                  <div className="mt-1.5 flex items-center justify-between gap-3">
                    <span className="min-w-0 flex-1 truncate">{o.customerName} · {o.phone}</span>
                    <span className="shrink-0 font-medium">{formatPrice(o.totalPrice)}</span>
                  </div>
                  <div className="mt-1 text-[13px] text-muted">
                    {o.quantity} cuốn · {PAYMENT_METHOD_SHORT[o.paymentMethod]} · {formatDateTime(o.createdAt)}
                  </div>
                </button>
              </li>
            ))}
          </ul>

          <div className="hidden overflow-hidden rounded-card border border-border bg-surface md:block">
            <Table<AdminOrder>
              rowKey="id"
              columns={columns}
              dataSource={rows}
              size="middle"
              onRow={(o) => ({
                onClick: () => setSelectedId(o.id),
                className: `cursor-pointer ${isOverdue(o) ? 'bg-danger-bg/40' : ''} ${o.id === selected?.id ? 'bg-primary-light' : ''}`,
              })}
              pagination={{
                current: data.page,
                pageSize: data.pageSize,
                total: data.totalCount,
                showSizeChanger: false,
                onChange: (page) => setFilter((f) => ({ ...f, page })),
              }}
            />
          </div>

          {selected && <OrderDetailPanel order={selected} statusTag={statusTag} onChange={updateStatus.mutate} pending={updateStatus.isPending} />}
        </div>
      )}
    </>
  )
}

interface PanelProps {
  order: AdminOrder
  statusTag: (o: AdminOrder) => React.ReactNode
  onChange: (v: { id: number; status: OrderStatusValue }) => void
  pending: boolean
}

function OrderDetailPanel({ order, statusTag, onChange, pending }: PanelProps) {
  const next = ORDER_NEXT_STATUSES[order.status]

  return (
    <aside className="h-fit rounded-card border border-border-strong bg-surface p-5">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="font-display text-xl break-all">{order.orderCode}</p>
          <p className="mt-1 text-[13px] text-muted">Đặt lúc {formatDateTime(order.createdAt)}</p>
        </div>
        <span className="shrink-0">{statusTag(order)}</span>
      </div>

      <dl className="mt-5 grid gap-3 border-t border-border pt-4 text-[14px]">
        <div><dt className="text-[13px] text-muted">Khách hàng</dt><dd className="mt-0.5 font-medium">{order.customerName}</dd></div>
        <div><dt className="text-[13px] text-muted">Điện thoại</dt><dd className="mt-0.5 font-medium">{order.phone}</dd></div>
        <div><dt className="text-[13px] text-muted">Địa chỉ</dt><dd className="mt-0.5">{order.address}</dd></div>
        <div><dt className="text-[13px] text-muted">Ghi chú</dt><dd className="mt-0.5">{order.note || '—'}</dd></div>
        <div className="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1 border-t border-border pt-3">
          <dt className="text-[13px] text-muted">{order.quantity} cuốn · {PAYMENT_METHOD_SHORT[order.paymentMethod]}</dt>
          <dd className="text-lg font-semibold text-primary whitespace-nowrap">{formatPrice(order.totalPrice)}</dd>
        </div>
      </dl>

      <div className="mt-5 border-t border-border pt-4">
        <p className="text-[13px] font-medium">Chuyển trạng thái</p>
        {next.length === 0 ? (
          <p className="mt-2 text-[14px] text-muted">Đơn đã kết thúc, không chuyển được nữa.</p>
        ) : (
          <>
            <div className="mt-2 flex flex-wrap gap-2">
              {next.map((status) => (
                <Button
                  key={status}
                  type={status === OrderStatus.Cancelled ? 'default' : 'primary'}
                  danger={status === OrderStatus.Cancelled}
                  loading={pending}
                  onClick={() => onChange({ id: order.id, status })}
                >
                  {ORDER_STATUS_LABEL[status]}
                </Button>
              ))}
            </div>
            <p className="mt-2 text-[12px] text-muted">Chỉ hiện bước hợp lệ theo luồng đơn hàng.</p>
          </>
        )}
      </div>

      <a href={`tel:${order.phone}`} className="mt-4 block border-t border-border pt-4">
        <Button block>Gọi khách {order.phone}</Button>
      </a>
    </aside>
  )
}
