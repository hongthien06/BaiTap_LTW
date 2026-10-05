import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Form, Input, InputNumber, Skeleton, Switch, message } from 'antd'
import { useEffect } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import type { AdminBook } from '../../api/types'
import { AdminFormCard, AdminPageHeader } from '../../components/admin/AdminPage'
import { UploadField } from '../../components/admin/UploadField'
import { discountPercent, formatPrice } from '../../lib/format'

type FormValues = Omit<AdminBook, 'id' | 'updatedAt'>

export function BookPage() {
  const [form] = Form.useForm<FormValues>()
  const queryClient = useQueryClient()
  const [toast, contextHolder] = message.useMessage()

  const { data, isPending, isError, error, refetch } = useQuery({
    queryKey: ['admin', 'book'],
    queryFn: adminApi.getBook,
  })

  useEffect(() => {
    if (data) form.setFieldsValue(data)
  }, [data, form])

  const save = useMutation({
    mutationFn: adminApi.updateBook,
    onSuccess: () => {
      toast.success('Đã lưu thông tin sách.')
      queryClient.invalidateQueries({ queryKey: ['admin', 'book'] })
      queryClient.invalidateQueries({ queryKey: ['landing'] })
    },
    onError: (err) => toast.error(getErrorMessage(err)),
  })

  // Xem truoc dung cong thuc landing dung, de admin thay ngay gia nao se hien ra.
  const price = Form.useWatch('price', form) ?? 0
  const discount = Form.useWatch('discountPrice', form)
  const hasDiscount = typeof discount === 'number' && discount > 0 && discount < price

  if (isPending) return <AdminFormCard><Skeleton active paragraph={{ rows: 8 }} /></AdminFormCard>

  if (isError) {
    return (
      <>
        <AdminPageHeader title="Thông tin sách" />
        <div className="mt-4 grid place-items-center rounded-card border border-danger/30 bg-danger-bg/40 px-6 py-16 text-center">
          <p className="font-medium">Không tải được thông tin sách</p>
          <p className="mt-1 max-w-sm text-[14px] text-muted">{getErrorMessage(error)}</p>
          <Button className="mt-5" onClick={() => refetch()}>Thử lại</Button>
        </div>
      </>
    )
  }

  return (
    <>
      {contextHolder}

      <AdminPageHeader
        title="Thông tin sách"
        description="Mọi thay đổi ở đây hiện ngay trên landing page sau khi lưu."
      />

      <div className="mt-4">
        <AdminFormCard>
          <Form<FormValues> form={form} layout="vertical" onFinish={(values) => save.mutate(values)}>
            <Form.Item name="name" label="Tên sách" rules={[{ required: true, message: 'Vui lòng nhập tên sách' }]}>
              <Input />
            </Form.Item>

            <Form.Item name="category" label="Thể loại" rules={[{ required: true, message: 'Vui lòng nhập thể loại' }]}>
              <Input />
            </Form.Item>

            <Form.Item
              name="title"
              label="Title ở Hero"
              tooltip="Dòng chữ lớn nhất trên landing"
              rules={[{ required: true, message: 'Vui lòng nhập title' }]}
            >
              <Input />
            </Form.Item>

            <Form.Item name="subtitle" label="Subtitle ở Hero" tooltip="Câu mô tả ngắn ngay dưới title">
              <Input.TextArea rows={2} showCount maxLength={500} />
            </Form.Item>

            <Form.Item name="description" label="Mô tả">
              <Input.TextArea rows={5} />
            </Form.Item>

            <div className="grid gap-x-4 sm:grid-cols-2">
              <Form.Item name="price" label="Giá gốc (VND)" rules={[{ required: true, message: 'Vui lòng nhập giá' }]}>
                <InputNumber min={1} step={1000} className="w-full" />
              </Form.Item>

              <Form.Item
                name="discountPrice"
                label="Giá giảm (VND)"
                tooltip="Để trống nếu không giảm giá"
                dependencies={['price']}
                rules={[
                  ({ getFieldValue }) => ({
                    validator(_, value) {
                      if (value === null || value === undefined || value === '') return Promise.resolve()
                      if (value > 0 && value < getFieldValue('price')) return Promise.resolve()
                      return Promise.reject(new Error('Giá giảm phải lớn hơn 0 và nhỏ hơn giá gốc'))
                    },
                  }),
                ]}
              >
                <InputNumber min={0} step={1000} className="w-full" />
              </Form.Item>
            </div>

            <Alert
              type="info"
              showIcon
              className="mb-6"
              message={
                hasDiscount
                  ? `Landing sẽ hiện ${formatPrice(discount)}, gạch ngang ${formatPrice(price)}, badge -${discountPercent(price, discount)}%`
                  : `Landing sẽ hiện ${formatPrice(price)}, không có badge giảm giá`
              }
            />

            <Form.Item name="coverImageUrl" label="Ảnh bìa">
              <UploadField kind="Image" placeholder="/uploads/bia.jpg" />
            </Form.Item>

            <Form.Item name="mockupImageUrl" label="Ảnh mockup ở Hero">
              <UploadField kind="Image" placeholder="/uploads/mockup.png" />
            </Form.Item>

            <Form.Item
              name="isActive"
              label="Đang mở bán"
              valuePropName="checked"
              tooltip="Tắt là landing page ngừng hoạt động. Không tắt được nếu đây là sách duy nhất đang mở bán."
            >
              <Switch />
            </Form.Item>

            <Button type="primary" htmlType="submit" loading={save.isPending}>Lưu thay đổi</Button>
          </Form>
        </AdminFormCard>
      </div>
    </>
  )
}
