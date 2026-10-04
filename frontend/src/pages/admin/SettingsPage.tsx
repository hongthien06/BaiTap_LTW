import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Input, message } from 'antd'
import { useEffect } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import type { SettingItem } from '../../api/types'

export function SettingsPage() {
  const [form] = Form.useForm<Record<string, string>>()
  const queryClient = useQueryClient()
  const [toast, contextHolder] = message.useMessage()

  const { data, isPending } = useQuery({ queryKey: ['admin', 'settings'], queryFn: adminApi.getSettings })

  useEffect(() => {
    if (!data) return
    form.setFieldsValue(Object.fromEntries(data.map((item) => [item.key, item.value])))
  }, [data, form])

  const save = useMutation({
    mutationFn: adminApi.updateSettings,
    onSuccess: () => {
      toast.success('Đã lưu cấu hình.')
      queryClient.invalidateQueries({ queryKey: ['admin', 'settings'] })
      queryClient.invalidateQueries({ queryKey: ['landing'] })
    },
    onError: (error) => toast.error(getErrorMessage(error)),
  })

  if (isPending || !data) return <p>Đang tải...</p>

  const onFinish = (values: Record<string, string>) => {
    const items: SettingItem[] = data.map((item) => ({
      key: item.key,
      value: values[item.key] ?? item.value,
      description: item.description,
    }))
    save.mutate(items)
  }

  return (
    <>
      {contextHolder}
      <h1 className="mb-4 text-xl font-semibold">Cấu hình site</h1>

      <Form form={form} layout="vertical" style={{ maxWidth: 720 }} onFinish={onFinish}>
        {data.map((item) => (
          <Form.Item key={item.key} name={item.key} label={item.description ?? item.key} tooltip={item.key}>
            <Input />
          </Form.Item>
        ))}

        <Button type="primary" htmlType="submit" loading={save.isPending}>Lưu thay đổi</Button>
      </Form>
    </>
  )
}
