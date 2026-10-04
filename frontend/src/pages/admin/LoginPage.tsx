import { Alert, Button, Form, Input } from 'antd'
import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { getErrorMessage } from '../../api/client'
import { useAuth } from '../../hooks/useAuth'

interface FormValues {
  email: string
  password: string
}

export function LoginPage() {
  const { user, isLoading, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (isLoading) return <p className="p-10 text-center">Đang tải...</p>
  if (user) return <Navigate to={(location.state as { from?: string } | null)?.from ?? '/admin/orders'} replace />

  const onFinish = async (values: FormValues) => {
    setSubmitting(true)
    setError(null)
    try {
      await login(values.email, values.password)
      navigate('/admin/orders', { replace: true })
    } catch (err) {
      setError(getErrorMessage(err, 'Đăng nhập thất bại.'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-sand px-4">
      <div className="w-full max-w-sm rounded-lg bg-white p-8 shadow">
        <h1 className="font-display text-2xl">Đăng nhập quản trị</h1>

        {error && <Alert type="error" message={error} showIcon className="mt-4" />}

        <Form<FormValues> layout="vertical" onFinish={onFinish} className="mt-6" requiredMark={false}>
          <Form.Item
            name="email"
            label="Email"
            rules={[
              { required: true, message: 'Vui lòng nhập email' },
              { type: 'email', message: 'Email không hợp lệ' },
            ]}
          >
            <Input autoComplete="username" />
          </Form.Item>

          <Form.Item
            name="password"
            label="Mật khẩu"
            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu' }]}
          >
            <Input.Password autoComplete="current-password" />
          </Form.Item>

          <Button type="primary" htmlType="submit" loading={submitting} block>
            Đăng nhập
          </Button>
        </Form>
      </div>
    </div>
  )
}
