import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createBrowserRouter, RouterProvider } from 'react-router'
import { AppLayout } from '@/components/app-layout'
import { Toaster } from '@/components/ui/sonner'
import { PrintPage } from '@/pages/print-page'
import { SearchPage } from '@/pages/search-page'
import { WordEditPage } from '@/pages/word-edit-page'
import { WordsPage } from '@/pages/words-page'

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, retry: 1 } },
})

const router = createBrowserRouter([
  {
    element: <AppLayout />,
    children: [
      { path: '/', element: <SearchPage /> },
      { path: '/words', element: <WordsPage /> },
      { path: '/words/new', element: <WordEditPage /> },
      { path: '/words/:id', element: <WordEditPage /> },
      { path: '/print', element: <PrintPage /> },
    ],
  },
])

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
      <Toaster position="top-center" richColors />
    </QueryClientProvider>
  )
}
