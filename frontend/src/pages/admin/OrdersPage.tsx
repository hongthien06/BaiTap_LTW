import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Input, Select, Space, Table, Tag, message } from 'antd'
import { useState } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi, type OrderFilter } from '../../api/endpoints'
import {
  ORDER_NEXT_STATUSES, ORDER_STATUS_LABEL, OrderStatus, PAYMENT_METHOD_LABEL,
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

export function OrdersPage() {
  const [filter, setFilter] = useState<OrderFilter>({ page: 1, pageSize: 10 })
  const queryClient = useQueryClient()
  const [toast, contextHolder] = message.useMessage()

  const { data, isFetching } = useQuery({
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
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  const columns = [
    { title: 'Mã đơn', dataIndex: 'orderCode', key: 'orderCode' },
    { title: 'Khách hàng', dataIndex: 'customerName', key: 'customerName' },
    { title: 'SĐT', dataIndex: 'phone', key: 'phone' },
    { title: 'Địa chỉ', dataIndex: 'address', key: 'address', ellipsis: true },
    { title: 'SL', dataIndex: 'quantity', key: 'quantity', width: 60 },
    {
      title: 'Tổng tiền',
      dataIndex: 'totalPrice',
      key: 'totalPrice',
      render: (value: number) => formatPrice(value),
    },
    {
      title: 'Thanh toán',
      dataIndex: 'paymentMethod',
      key: 'paymentMethod',
      render: (value: 0 | 1) => PAYMENT_METHOD_LABEL[value],
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      render: (value: OrderStatusValue) => (
        <Tag color={STATUS_COLOR[value]}>{ORDER_STATUS_LABEL[value]}</Tag>
      ),
    },
    {
      title: 'Ngày đặt',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (value: string) => formatDateTime(value),
    },
    {
      title: 'Chuyển trạng thái',
      key: 'actions',
      render: (_: unknown, record: AdminOrder) => {
        // Chi hien cac buoc hop le theo state machine; backend van kiem tra lai.
        const next = ORDER_NEXT_STATUSES[record.status]
        if (next.length === 0) return <span className="text-black/40">Đã kết thúc</span>

        return (
          <Space>
            {next.map((status) => (
              <Button
                key={status}
                size="small"
                loading={updateStatus.isPending}
                onClick={() => updateStatus.mutate({ id: record.id, status })}
              >
                {ORDER_STATUS_LABEL[status]}
              </Button>
            ))}
          </Space>
        )
      },
    },
  ]

  return (
    <>
      {contextHolder}
      <h1 className="mb-4 text-xl font-semibold">Đơn đặt hàng</h1>

      <Space className="mb-4" wrap>
        <Select<OrderStatusValue | undefined>
          allowClear
          placeholder="Lọc theo trạng thái"
          style={{ width: 180 }}
          value={filter.status}
          onChange={(status) => setFilter((f) => ({ ...f, status, page: 1 }))}
          options={Object.entries(ORDER_STATUS_LABEL).map(([value, label]) => ({
            value: Number(value) as OrderStatusValue,
            label,
          }))}
        />

        <Input.Search
          allowClear
          placeholder="Tìm theo số điện thoại"
          style={{ width: 240 }}
          onSearch={(phone) => setFilter((f) => ({ ...f, phone: phone || undefined, page: 1 }))}
        />
      </Space>

      <Table<AdminOrder>
        rowKey="id"
        loading={isFetching}
        columns={columns}
        dataSource={data?.items ?? []}
        scroll={{ x: 1200 }}
        pagination={{
          current: data?.page ?? 1,
          pageSize: data?.pageSize ?? 10,
          total: data?.totalCount ?? 0,
          showSizeChanger: false,
          onChange: (page) => setFilter((f) => ({ ...f, page })),
        }}
      />
    </>
  )
}
