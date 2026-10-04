import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Popconfirm, Segmented, Space, Table, Tag, message } from 'antd'
import { useState } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import type { AdminFeedback } from '../../api/types'
import { formatDateTime } from '../../lib/format'
import { useAuth } from '../../hooks/useAuth'

type FilterKey = 'pending' | 'approved' | 'all'

const FILTER_TO_PARAM: Record<FilterKey, boolean | undefined> = {
  pending: false,
  approved: true,
  all: undefined,
}

export function FeedbacksPage() {
  const [filter, setFilter] = useState<FilterKey>('pending')
  const queryClient = useQueryClient()
  const [toast, contextHolder] = message.useMessage()
  const { isAdmin } = useAuth()

  const { data, isFetching } = useQuery({
    queryKey: ['admin', 'feedbacks', filter],
    queryFn: () => adminApi.getFeedbacks(FILTER_TO_PARAM[filter], 1, 50),
  })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['admin', 'feedbacks'] })

  const approve = useMutation({
    mutationFn: adminApi.approveFeedback,
    onSuccess: () => { toast.success('Đã duyệt đánh giá.'); invalidate() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  const hide = useMutation({
    mutationFn: adminApi.hideFeedback,
    onSuccess: () => { toast.success('Đã ẩn đánh giá.'); invalidate() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  const remove = useMutation({
    mutationFn: adminApi.deleteFeedback,
    onSuccess: () => { toast.success('Đã xoá đánh giá.'); invalidate() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  const columns = [
    { title: 'Khách hàng', dataIndex: 'customerName', key: 'customerName' },
    { title: 'Số sao', dataIndex: 'rating', key: 'rating', width: 90, render: (v: number) => `${v}/5` },
    { title: 'Nội dung', dataIndex: 'content', key: 'content' },
    {
      title: 'Trạng thái',
      dataIndex: 'isApproved',
      key: 'isApproved',
      render: (value: boolean) =>
        value ? <Tag color="green">Đã duyệt</Tag> : <Tag color="gold">Chờ duyệt</Tag>,
    },
    {
      title: 'Ngày gửi',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (value: string) => formatDateTime(value),
    },
    {
      title: 'Thao tác',
      key: 'actions',
      render: (_: unknown, record: AdminFeedback) => (
        <Space>
          {record.isApproved ? (
            <Button size="small" onClick={() => hide.mutate(record.id)}>Ẩn</Button>
          ) : (
            <Button size="small" type="primary" onClick={() => approve.mutate(record.id)}>Duyệt</Button>
          )}

          {isAdmin && (
            <Popconfirm title="Xoá vĩnh viễn đánh giá này?" onConfirm={() => remove.mutate(record.id)}>
              <Button size="small" danger>Xoá</Button>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ]

  return (
    <>
      {contextHolder}
      <h1 className="mb-4 text-xl font-semibold">Đánh giá của độc giả</h1>

      <Segmented<FilterKey>
        className="mb-4"
        value={filter}
        onChange={setFilter}
        options={[
          { label: 'Chờ duyệt', value: 'pending' },
          { label: 'Đã duyệt', value: 'approved' },
          { label: 'Tất cả', value: 'all' },
        ]}
      />

      <Table<AdminFeedback>
        rowKey="id"
        loading={isFetching}
        columns={columns}
        dataSource={data?.items ?? []}
        pagination={false}
      />
    </>
  )
}
