import { useQuery } from '@tanstack/react-query'
import { Table, Tag } from 'antd'
import { adminApi } from '../../api/endpoints'
import type { AdminUser } from '../../api/types'
import { formatDateTime } from '../../lib/format'

export function UsersPage() {
  const { data, isFetching } = useQuery({ queryKey: ['admin', 'users'], queryFn: adminApi.getUsers })

  const columns = [
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
  ]

  return (
    <>
      <h1 className="mb-4 text-xl font-semibold">Tài khoản quản trị</h1>
      <Table<AdminUser>
        rowKey="id"
        loading={isFetching}
        columns={columns}
        dataSource={data ?? []}
        pagination={false}
      />
    </>
  )
}
