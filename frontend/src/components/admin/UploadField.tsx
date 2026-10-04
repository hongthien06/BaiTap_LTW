import { UploadOutlined } from '@ant-design/icons'
import { Button, Input, Space, Upload, message } from 'antd'
import { useState } from 'react'
import { getErrorMessage } from '../../api/client'
import { adminApi } from '../../api/endpoints'
import type { UploadKind } from '../../api/types'

interface Props {
  /** Do Ant Design Form truyen vao khi dung ben trong Form.Item. */
  value?: string | null
  onChange?: (value: string | null) => void
  kind: UploadKind
  placeholder?: string
}

/**
 * O nhap duong dan file, kem nut tai len.
 * Van cho sua tay duong dan de dung lai anh da co san trong /img.
 */
export function UploadField({ value, onChange, kind, placeholder }: Props) {
  const [uploading, setUploading] = useState(false)
  const [toast, contextHolder] = message.useMessage()

  const accept = kind === 'Pdf' ? '.pdf' : '.jpg,.jpeg,.png,.webp'

  return (
    <>
      {contextHolder}
      <Space.Compact style={{ width: '100%' }}>
        <Input
          value={value ?? ''}
          placeholder={placeholder}
          onChange={(event) => onChange?.(event.target.value || null)}
        />

        <Upload
          accept={accept}
          showUploadList={false}
          beforeUpload={async (file) => {
            setUploading(true)
            try {
              const stored = await adminApi.upload(file, kind)
              onChange?.(stored.url)
              toast.success('Tải lên thành công.')
            } catch (error) {
              toast.error(getErrorMessage(error, 'Tải lên thất bại.'))
            } finally {
              setUploading(false)
            }
            return false // tu goi API, khong de Upload tu gui
          }}
        >
          <Button icon={<UploadOutlined />} loading={uploading}>Tải lên</Button>
        </Upload>
      </Space.Compact>

      {value && kind === 'Image' && (
        <img src={value} alt="Xem trước" className="mt-2 h-24 rounded border object-contain" />
      )}
    </>
  )
}
