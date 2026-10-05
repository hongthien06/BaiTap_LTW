import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Empty, Popconfirm, Segmented, Skeleton, Tag, message } from 'antd'
import { useState } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import { AdminCard, AdminPageHeader } from '../../components/admin/AdminPage'
import { StarRating } from '../../components/landing/StarRating'
import { useAuth } from '../../hooks/useAuth'
import { formatDateTime } from '../../lib/format'

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

  const { data, isPending, isError, error, refetch } = useQuery({
    queryKey: ['admin', 'feedbacks', filter],
    queryFn: () => adminApi.getFeedbacks(FILTER_TO_PARAM[filter], 1, 50),
  })

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['admin', 'feedbacks'] })
    queryClient.invalidateQueries({ queryKey: ['landing'] })
  }

  // Khai bao thang ba useMutation: goi hook trong mot ham thuong la pha luat hook cua React.
  const onError = (e: unknown) => toast.error(getErrorMessage(e))

  const approve = useMutation({
    mutationFn: adminApi.approveFeedback,
    onSuccess: () => { toast.success('Đã duyệt đánh giá.'); invalidate() },
    onError,
  })
  const hide = useMutation({
    mutationFn: adminApi.hideFeedback,
    onSuccess: () => { toast.success('Đã ẩn đánh giá.'); invalidate() },
    onError,
  })
  const remove = useMutation({
    mutationFn: adminApi.deleteFeedback,
    onSuccess: () => { toast.success('Đã xoá đánh giá.'); invalidate() },
    onError,
  })

  const items = data?.items ?? []
  const pendingCount = filter === 'pending' ? items.length : undefined

  return (
    <>
      {contextHolder}

      <AdminPageHeader
        title="Đánh giá của độc giả"
        description={
          pendingCount !== undefined && pendingCount > 0
            ? <span className="font-medium text-warning">{pendingCount} đánh giá đang chờ duyệt</span>
            : 'Chỉ đánh giá đã duyệt mới hiện trên landing và được tính vào điểm trung bình.'
        }
      />

      <div className="mt-4">
        <Segmented<FilterKey>
          value={filter}
          onChange={setFilter}
          options={[
            { label: 'Chờ duyệt', value: 'pending' },
            { label: 'Đã duyệt', value: 'approved' },
            { label: 'Tất cả', value: 'all' },
          ]}
        />
      </div>

      {isPending ? (
        <AdminCard className="mt-4"><Skeleton active paragraph={{ rows: 5 }} /></AdminCard>
      ) : isError ? (
        <div className="mt-4 grid place-items-center rounded-card border border-danger/30 bg-danger-bg/40 px-6 py-16 text-center">
          <p className="font-medium">Không tải được danh sách đánh giá</p>
          <p className="mt-1 max-w-sm text-[14px] text-muted">{getErrorMessage(error)}</p>
          <Button className="mt-5" onClick={() => refetch()}>Thử lại</Button>
        </div>
      ) : items.length === 0 ? (
        <AdminCard className="mt-4 px-6 py-16">
          <Empty
            description={
              filter === 'pending' ? 'Không còn đánh giá nào chờ duyệt' : 'Chưa có đánh giá nào'
            }
          />
        </AdminCard>
      ) : (
        <ul className="mt-4 grid gap-3">
          {items.map((feedback) => (
            <li key={feedback.id}>
              <AdminCard>
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-medium break-words">{feedback.customerName}</span>
                      <StarRating value={feedback.rating} />
                      {feedback.isApproved
                        ? <Tag color="green">Đã duyệt</Tag>
                        : <Tag color="gold">Chờ duyệt</Tag>}
                    </div>

                    <p className="mt-2 text-muted">{feedback.content}</p>

                    <p className="mt-2 text-[13px] text-muted">
                      Gửi lúc {formatDateTime(feedback.createdAt)}
                      {feedback.approvedAt && ` · duyệt lúc ${formatDateTime(feedback.approvedAt)}`}
                    </p>
                  </div>

                  <div className="flex shrink-0 gap-2">
                    {feedback.isApproved ? (
                      <Button onClick={() => hide.mutate(feedback.id)} loading={hide.isPending}>Ẩn</Button>
                    ) : (
                      <Button type="primary" onClick={() => approve.mutate(feedback.id)} loading={approve.isPending}>
                        Duyệt
                      </Button>
                    )}

                    {isAdmin && (
                      <Popconfirm
                        title="Xoá vĩnh viễn đánh giá này?"
                        okText="Xoá"
                        cancelText="Thôi"
                        onConfirm={() => remove.mutate(feedback.id)}
                      >
                        <Button danger>Xoá</Button>
                      </Popconfirm>
                    )}
                  </div>
                </div>
              </AdminCard>
            </li>
          ))}
        </ul>
      )}
    </>
  )
}
