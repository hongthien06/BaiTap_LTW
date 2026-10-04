import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Input, InputNumber, Switch, message } from 'antd'
import { useEffect } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import type { AdminBook } from '../../api/types'

type FormValues = Omit<AdminBook, 'id' | 'updatedAt'>

export function BookPage() {
  const [form] = Form.useForm<FormValues>()
  const queryClient = useQueryClient()
  const [toast, contextHolder] = message.useMessage()

  const { data, isPending } = useQuery({ queryKey: ['admin', 'book'], queryFn: adminApi.getBook })

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
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  if (isPending) return <p>Đang tải...</p>

  return (
    <>
      {contextHolder}
      <h1 className="mb-4 text-xl font-semibold">Thông tin sách</h1>

      <Form<FormValues>
        form={form}
        layout="vertical"
        style={{ maxWidth: 720 }}
        onFinish={(values) => save.mutate(values)}
      >
        <Form.Item name="name" label="Tên sách" rules={[{ required: true, message: 'Vui lòng nhập tên sách' }]}>
          <Input />
        </Form.Item>

        <Form.Item name="category" label="Thể loại" rules={[{ required: true, message: 'Vui lòng nhập thể loại' }]}>
          <Input />
        </Form.Item>

        <Form.Item name="title" label="Title (Hero)" rules={[{ required: true, message: 'Vui lòng nhập title' }]}>
          <Input />
        </Form.Item>

        <Form.Item name="subtitle" label="Subtitle (Hero)">
          <Input />
        </Form.Item>

        <Form.Item name="description" label="Mô tả">
          <Input.TextArea rows={5} />
        </Form.Item>

        <Form.Item
          name="price"
          label="Giá gốc (VND)"
          rules={[{ required: true, message: 'Vui lòng nhập giá' }]}
        >
          <InputNumber min={1} step={1000} style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item
          name="discountPrice"
          label="Giá giảm (VND) — để trống nếu không giảm"
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
          <InputNumber min={0} step={1000} style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item name="coverImageUrl" label="Ảnh bìa (URL)">
          <Input placeholder="/uploads/xxx.jpg" />
        </Form.Item>

        <Form.Item name="mockupImageUrl" label="Ảnh mockup Hero (URL)">
          <Input placeholder="/uploads/xxx.png" />
        </Form.Item>

        <Form.Item name="isActive" label="Đang mở bán" valuePropName="checked">
          <Switch />
        </Form.Item>

        <Button type="primary" htmlType="submit" loading={save.isPending}>Lưu thay đổi</Button>
      </Form>
    </>
  )
}
