import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Input, Modal, Popconfirm, Select, Space, Switch, Table, Tag, message } from 'antd'
import { useState } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import type { AdminUser, CreateUserRequest, UpdateUserRequest } from '../../api/types'
import { formatDateTime } from '../../lib/format'
import { useAuth } from '../../hooks/useAuth'

interface FormValues {
  email: string
  fullName: string
  role: 'Admin' | 'Staff'
  password?: string
  isActive: boolean
}

export function UsersPage() {
  const [editing, setEditing] = useState<AdminUser | 'new' | null>(null)
  const [form] = Form.useForm<FormValues>()
  const [toast, contextHolder] = message.useMessage()
  const queryClient = useQueryClient()
  const { user: currentUser } = useAuth()

  const { data, isFetching } = useQuery({ queryKey: ['admin', 'users'], queryFn: adminApi.getUsers })

  const close = () => { setEditing(null); form.resetFields() }
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['admin', 'users'] })

  const save = useMutation({
    mutationFn: (values: FormValues) => {
      if (editing === 'new' || editing === null) {
        const payload: CreateUserRequest = {
          email: values.email,
          password: values.password ?? '',
          fullName: values.fullName,
          role: values.role,
        }
        return adminApi.createUser(payload)
      }

      const payload: UpdateUserRequest = {
        fullName: values.fullName,
        role: values.role,
        isActive: values.isActive,
        // De trong = giu nguyen mat khau cu.
        newPassword: values.password ? values.password : null,
      }
      return adminApi.updateUser(editing.id, payload)
    },
    onSuccess: () => { toast.success('Đã lưu tài khoản.'); invalidate(); close() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  const remove = useMutation({
    mutationFn: adminApi.deleteUser,
    onSuccess: () => { toast.success('Đã xoá tài khoản.'); invalidate() },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  const openCreate = () => {
    setEditing('new')
    form.setFieldsValue({ email: '', fullName: '', role: 'Staff', password: '', isActive: true })
  }

  const openEdit = (user: AdminUser) => {
    setEditing(user)
    form.setFieldsValue({
      email: user.email,
      fullName: user.fullName,
      role: user.role,
      isActive: user.isActive,
      password: '',
    })
  }

  const isCreating = editing === 'new'

  return (
    <>
      {contextHolder}
      <h1 className="mb-4 text-xl font-semibold">Tài khoản quản trị</h1>

      <Button type="primary" className="mb-4" onClick={openCreate}>Thêm tài khoản</Button>

      <Table<AdminUser>
        rowKey="id"
        loading={isFetching}
        dataSource={data ?? []}
        pagination={false}
        columns={[
          { title: 'Email', dataIndex: 'email', key: 'email' },
          { title: 'Họ tên', dataIndex: 'fullName', key: 'fullName' },
          {
            title: 'Vai trò',
            dataIndex: 'role',
            key: 'role',
            render: (role: string) => <Tag color={role === 'Admin' ? 'red' : 'blue'}>{role}</Tag>,
          },
          {
            title: 'Trạng thái',
            dataIndex: 'isActive',
            key: 'isActive',
            render: (value: boolean) => (value ? 'Đang hoạt động' : 'Đã khoá'),
          },
          {
            title: 'Đăng nhập gần nhất',
            dataIndex: 'lastLoginAt',
            key: 'lastLoginAt',
            render: (value: string | null) => (value ? formatDateTime(value) : 'Chưa đăng nhập'),
          },
          {
            title: 'Thao tác',
            key: 'actions',
            render: (_: unknown, record: AdminUser) => (
              <Space>
                <Button size="small" onClick={() => openEdit(record)}>Sửa</Button>

                {/* Backend cung chan tu xoa chinh minh; day chi la chan som cho de hieu. */}
                {record.id !== currentUser?.id && (
                  <Popconfirm title="Xoá tài khoản này?" onConfirm={() => remove.mutate(record.id)}>
                    <Button size="small" danger>Xoá</Button>
                  </Popconfirm>
                )}
              </Space>
            ),
          },
        ]}
      />

      <Modal
        open={editing !== null}
        title={isCreating ? 'Thêm tài khoản' : 'Sửa tài khoản'}
        onCancel={close}
        onOk={() => form.submit()}
        confirmLoading={save.isPending}
        destroyOnHidden
      >
        <Form<FormValues> form={form} layout="vertical" onFinish={(values) => save.mutate(values)}>
          <Form.Item
            name="email"
            label="Email"
            rules={[
              { required: true, message: 'Vui lòng nhập email' },
              { type: 'email', message: 'Email không hợp lệ' },
            ]}
          >
            <Input disabled={!isCreating} />
          </Form.Item>

          <Form.Item name="fullName" label="Họ tên" rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}>
            <Input />
          </Form.Item>

          <Form.Item name="role" label="Vai trò" rules={[{ required: true }]}>
            <Select
              options={[
                { value: 'Admin', label: 'Admin — toàn quyền' },
                { value: 'Staff', label: 'Staff — chỉ xem đơn và duyệt đánh giá' },
              ]}
            />
          </Form.Item>

          <Form.Item
            name="password"
            label={isCreating ? 'Mật khẩu' : 'Mật khẩu mới (để trống nếu không đổi)'}
            rules={[
              { required: isCreating, message: 'Vui lòng nhập mật khẩu' },
              {
                validator: (_, value) =>
                  !value || value.length >= 8
                    ? Promise.resolve()
                    : Promise.reject(new Error('Mật khẩu tối thiểu 8 ký tự')),
              },
            ]}
          >
            <Input.Password autoComplete="new-password" />
          </Form.Item>

          {!isCreating && (
            <Form.Item name="isActive" label="Đang hoạt động" valuePropName="checked">
              <Switch />
            </Form.Item>
          )}
        </Form>
      </Modal>
    </>
  )
}
