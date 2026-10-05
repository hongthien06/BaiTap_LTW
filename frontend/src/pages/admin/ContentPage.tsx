import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Input, InputNumber, Modal, Popconfirm, Space, Table, Tabs, message } from 'antd'
import { useEffect, useState } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import type { AdminPressQuote, PressQuoteRequest, UpdateAuthorRequest, UpdateReviewRequest } from '../../api/types'
import { AdminCard, AdminPageHeader } from '../../components/admin/AdminPage'
import { UploadField } from '../../components/admin/UploadField'

export function ContentPage() {
  return (
    <>
      <AdminPageHeader
        title="Nội dung landing page"
        description="Tác giả, trích dẫn báo chí và bài review hiện trên trang bán hàng."
      />
      <AdminCard className="mt-4">
      <Tabs
        items={[
          { key: 'author', label: 'Tác giả', children: <AuthorTab /> },
          { key: 'press', label: 'Báo chí', children: <PressTab /> },
          { key: 'review', label: 'Review nội dung', children: <ReviewTab /> },
        ]}
      />
      </AdminCard>
    </>
  )
}

/** Gom phan lam moi cache dung chung: sua noi dung xong thi landing phai doi theo. */
function useContentInvalidator() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['admin', 'content'] })
    queryClient.invalidateQueries({ queryKey: ['landing'] })
  }
}

function AuthorTab() {
  const [form] = Form.useForm<UpdateAuthorRequest>()
  const [toast, contextHolder] = message.useMessage()
  const invalidate = useContentInvalidator()

  const { data, isPending } = useQuery({
    queryKey: ['admin', 'content', 'author'],
    queryFn: adminApi.getAuthor,
    // Chua co tac gia thi API tra 404 - day la trang thai hop le, khong retry.
    retry: false,
  })

  useEffect(() => {
    if (data) form.setFieldsValue(data)
  }, [data, form])

  const save = useMutation({
    mutationFn: adminApi.upsertAuthor,
    onSuccess: () => { toast.success('Đã lưu thông tin tác giả.'); invalidate() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  if (isPending) return <p>Đang tải...</p>

  return (
    <>
      {contextHolder}
      <Form<UpdateAuthorRequest>
        form={form}
        layout="vertical"
        style={{ maxWidth: 640 }}
        onFinish={(values) => save.mutate(values)}
      >
        <Form.Item name="fullName" label="Họ tên" rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}>
          <Input />
        </Form.Item>

        <Form.Item name="avatarUrl" label="Ảnh tác giả">
          <UploadField kind="Image" placeholder="/uploads/anh.jpg" />
        </Form.Item>

        <Form.Item name="bio" label="Tiểu sử">
          <Input.TextArea rows={6} />
        </Form.Item>

        <Button type="primary" htmlType="submit" loading={save.isPending}>Lưu</Button>
      </Form>
    </>
  )
}

function PressTab() {
  const [editing, setEditing] = useState<AdminPressQuote | 'new' | null>(null)
  const [form] = Form.useForm<PressQuoteRequest>()
  const [toast, contextHolder] = message.useMessage()
  const invalidate = useContentInvalidator()

  const { data, isFetching } = useQuery({
    queryKey: ['admin', 'content', 'press'],
    queryFn: adminApi.getPressQuotes,
  })

  const close = () => { setEditing(null); form.resetFields() }

  const save = useMutation({
    mutationFn: (values: PressQuoteRequest) =>
      editing === 'new' || editing === null
        ? adminApi.createPressQuote(values)
        : adminApi.updatePressQuote(editing.id, values),
    onSuccess: () => { toast.success('Đã lưu trích dẫn.'); invalidate(); close() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  const remove = useMutation({
    mutationFn: adminApi.deletePressQuote,
    onSuccess: () => { toast.success('Đã xoá trích dẫn.'); invalidate() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  const openEdit = (quote: AdminPressQuote) => {
    setEditing(quote)
    form.setFieldsValue(quote)
  }

  const openCreate = () => {
    setEditing('new')
    form.setFieldsValue({
      pressName: '', logoUrl: null, quote: '', sourceUrl: null,
      sortOrder: (data?.length ?? 0) + 1,
    })
  }

  return (
    <>
      {contextHolder}
      <Button type="primary" className="mb-4" onClick={openCreate}>Thêm trích dẫn</Button>

      <Table<AdminPressQuote>
        rowKey="id"
        loading={isFetching}
        dataSource={data ?? []}
        pagination={false}
        scroll={{ x: 640 }}
        columns={[
          { title: 'Thứ tự', dataIndex: 'sortOrder', key: 'sortOrder', width: 80 },
          { title: 'Tên báo', dataIndex: 'pressName', key: 'pressName' },
          { title: 'Trích đoạn', dataIndex: 'quote', key: 'quote' },
          {
            title: 'Link',
            dataIndex: 'sourceUrl',
            key: 'sourceUrl',
            render: (url: string | null) =>
              url ? <a href={url} target="_blank" rel="noopener noreferrer">Mở</a> : '-',
          },
          {
            title: 'Thao tác',
            key: 'actions',
            render: (_: unknown, record: AdminPressQuote) => (
              <Space>
                <Button size="small" onClick={() => openEdit(record)}>Sửa</Button>
                <Popconfirm title="Xoá trích dẫn này?" onConfirm={() => remove.mutate(record.id)}>
                  <Button size="small" danger>Xoá</Button>
                </Popconfirm>
              </Space>
            ),
          },
        ]}
      />

      <Modal
        open={editing !== null}
        title={editing === 'new' ? 'Thêm trích dẫn' : 'Sửa trích dẫn'}
        onCancel={close}
        onOk={() => form.submit()}
        confirmLoading={save.isPending}
        destroyOnHidden
      >
        <Form<PressQuoteRequest> form={form} layout="vertical" onFinish={(values) => save.mutate(values)}>
          <Form.Item name="pressName" label="Tên báo" rules={[{ required: true, message: 'Vui lòng nhập tên báo' }]}>
            <Input />
          </Form.Item>

          <Form.Item name="quote" label="Trích đoạn" rules={[{ required: true, message: 'Vui lòng nhập trích đoạn' }]}>
            <Input.TextArea rows={3} />
          </Form.Item>

          <Form.Item name="sourceUrl" label="Link bài gốc">
            <Input placeholder="https://..." />
          </Form.Item>

          <Form.Item name="logoUrl" label="Logo báo">
            <UploadField kind="Image" placeholder="/uploads/logo.png" />
          </Form.Item>

          <Form.Item name="sortOrder" label="Thứ tự hiển thị">
            <InputNumber min={0} style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

function ReviewTab() {
  const [form] = Form.useForm<UpdateReviewRequest>()
  const [toast, contextHolder] = message.useMessage()
  const invalidate = useContentInvalidator()

  const { data, isPending } = useQuery({
    queryKey: ['admin', 'content', 'review'],
    queryFn: adminApi.getReview,
    retry: false,
  })

  useEffect(() => {
    if (data) form.setFieldsValue(data)
  }, [data, form])

  const save = useMutation({
    mutationFn: adminApi.upsertReview,
    onSuccess: () => { toast.success('Đã lưu review.'); invalidate() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  if (isPending) return <p>Đang tải...</p>

  return (
    <>
      {contextHolder}
      <Form<UpdateReviewRequest>
        form={form}
        layout="vertical"
        style={{ maxWidth: 720 }}
        onFinish={(values) => save.mutate(values)}
      >
        <Form.Item name="title" label="Tiêu đề" rules={[{ required: true, message: 'Vui lòng nhập tiêu đề' }]}>
          <Input />
        </Form.Item>

        <Form.Item name="content" label="Nội dung review">
          <Input.TextArea rows={8} />
        </Form.Item>

        <Form.Item name="fileUrl" label="File review (PDF) — để trống thì nút tải sẽ không hiện">
          <UploadField kind="Pdf" placeholder="/uploads/review.pdf" />
        </Form.Item>

        <Button type="primary" htmlType="submit" loading={save.isPending}>Lưu</Button>
      </Form>
    </>
  )
}
