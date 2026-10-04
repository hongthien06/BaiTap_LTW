import { QueryClient } from '@tanstack/react-query'

/**
 * Mot instance dung chung cho ca ung dung, tach ra file rieng de interceptor cua axios
 * cung xoa duoc cache khi phien het hieu luc (khong thi du lieu cua nguoi dang nhap truoc
 * van duoc phuc vu tu cache cho nguoi dang nhap sau trong cung tab).
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 30_000,
    },
  },
})
