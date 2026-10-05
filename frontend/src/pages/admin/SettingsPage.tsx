import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Form, Input, Skeleton, message } from 'antd'
import { useEffect } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import type { SettingItem } from '../../api/types'
import { AdminFormCard, AdminPageHeader } from '../../components/admin/AdminPage'
import { UploadField } from '../../components/admin/UploadField'

/**
 * Gom setting thanh nhom thay vi do het ra mot danh sach phang.
 * Key khong nam trong bang nay van hien, o nhom "Khac" — de them key moi o backend
 * khong lam mat o nhap tren giao dien.
 */
const GROUPS: { title: string; hint?: string; keys: string[] }[] = [
  { title: 'Liên hệ', keys: ['contact.hotline', 'contact.email', 'contact.address'] },
  { title: 'Mạng xã hội', hint: 'Chỉ nhận link http hoặc https', keys: ['social.facebook', 'social.youtube'] },
  { title: 'Thanh toán', hint: 'Hiện ra khi khách chọn chuyển khoản', keys: ['payment.bankInfo'] },
  { title: 'Thương hiệu', keys: ['site.logo', 'footer.text'] },
]

export function SettingsPage() {
  const [form] = Form.useForm<Record<string, string>>()
  const queryClient = useQueryClient()
  const [toast, contextHolder] = message.useMessage()

  const { data, isPending, isError, error, refetch } = useQuery({
    queryKey: ['admin', 'settings'],
    queryFn: adminApi.getSettings,
  })

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
    onError: (err) => toast.error(getErrorMessage(err)),
  })

  if (isPending) return <AdminFormCard><Skeleton active paragraph={{ rows: 8 }} /></AdminFormCard>

  if (isError || !data) {
    return (
      <>
        <AdminPageHeader title="Cấu hình site" />
        <div className="mt-4 grid place-items-center rounded-card border border-danger/30 bg-danger-bg/40 px-6 py-16 text-center">
          <p className="font-medium">Không tải được cấu hình</p>
          <p className="mt-1 max-w-sm text-[14px] text-muted">{getErrorMessage(error)}</p>
          <Button className="mt-5" onClick={() => refetch()}>Thử lại</Button>
        </div>
      </>
    )
  }

  const byKey = new Map(data.map((item) => [item.key, item]))
  const grouped = new Set(GROUPS.flatMap((g) => g.keys))
  const others = data.filter((item) => !grouped.has(item.key))

  const onFinish = (values: Record<string, string>) => {
    const items: SettingItem[] = data.map((item) => ({
      key: item.key,
      value: values[item.key] ?? item.value,
      description: item.description,
    }))
    save.mutate(items)
  }

  const field = (item: SettingItem) => (
    <Form.Item key={item.key} name={item.key} label={item.description ?? item.key} tooltip={item.key}>
      {item.key === 'site.logo' ? <UploadField kind="Image" placeholder="/uploads/logo.svg" /> : <Input />}
    </Form.Item>
  )

  return (
    <>
      {contextHolder}

      <AdminPageHeader
        title="Cấu hình site"
        description="Những thông tin này hiện ở chân trang và trong form đặt hàng."
      />

      <Form form={form} layout="vertical" onFinish={onFinish} className="mt-4 grid max-w-[720px] gap-4">
        {GROUPS.map((group) => {
          const items = group.keys.map((k) => byKey.get(k)).filter(Boolean) as SettingItem[]
          if (items.length === 0) return null

          return (
            <AdminFormCard key={group.title} className="max-w-none">
              <h2 className="font-display text-xl">{group.title}</h2>
              {group.hint && <p className="mt-0.5 mb-4 text-[13px] text-muted">{group.hint}</p>}
              <div className={group.hint ? '' : 'mt-4'}>{items.map(field)}</div>
            </AdminFormCard>
          )
        })}

        {others.length > 0 && (
          <AdminFormCard className="max-w-none">
            <h2 className="font-display text-xl">Khác</h2>
            <div className="mt-4">{others.map(field)}</div>
          </AdminFormCard>
        )}

        <div>
          <Button type="primary" htmlType="submit" loading={save.isPending}>Lưu thay đổi</Button>
        </div>
      </Form>
    </>
  )
}
