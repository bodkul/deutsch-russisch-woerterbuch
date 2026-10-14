import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import i18n from '@/i18n'
import type { components } from './schema'

// Types are generated from the backend OpenAPI: `npm run gen:api` (the API must be running).
export type WordDto = components['schemas']['WordDto']
export type WordInput = components['schemas']['WordInput']
export type DictEntryDto = components['schemas']['DictEntryDto']
export type SearchResult = components['schemas']['SearchResult']
export type WordExportFile = components['schemas']['WordExportFile']
export type ImportResult = components['schemas']['ImportResult']

type ProblemDetails = { title?: string; detail?: string; errors?: Record<string, string[]> }

export class ApiError extends Error {
  readonly status: number
  readonly errors?: Record<string, string[]>

  constructor(status: number, problem: ProblemDetails | null) {
    super(problem?.detail ?? problem?.title ?? i18n.t('errors.api', { status }))
    this.status = status
    this.errors = problem?.errors
  }
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    ...init,
    headers: init?.body ? { 'Content-Type': 'application/json' } : undefined,
  })
  if (!res.ok) {
    const problem = (await res.json().catch(() => null)) as ProblemDetails | null
    throw new ApiError(res.status, problem)
  }
  return (res.status === 204 ? undefined : await res.json()) as T
}

export const api = {
  search: (q: string) => request<SearchResult>(`/api/search?q=${encodeURIComponent(q)}`),
  dictEntry: (id: number) => request<DictEntryDto>(`/api/dict/${id}`),
  words: (tag?: string) =>
    request<WordDto[]>(tag ? `/api/words?tag=${encodeURIComponent(tag)}` : '/api/words'),
  word: (id: number) => request<WordDto>(`/api/words/${id}`),
  tags: () => request<string[]>('/api/tags'),
  createWord: (input: WordInput) =>
    request<WordDto>('/api/words', { method: 'POST', body: JSON.stringify(input) }),
  updateWord: (id: number, input: WordInput) =>
    request<WordDto>(`/api/words/${id}`, { method: 'PUT', body: JSON.stringify(input) }),
  deleteWord: (id: number) => request<void>(`/api/words/${id}`, { method: 'DELETE' }),
  /** Download link: the browser saves the file itself. */
  exportUrl: '/api/words/export',
  importWords: (file: WordExportFile) =>
    request<ImportResult>('/api/words/import', { method: 'POST', body: JSON.stringify(file) }),
}

export function useSearch(q: string) {
  const query = q.trim()
  return useQuery({
    queryKey: ['search', query],
    queryFn: () => api.search(query),
    enabled: query.length > 0,
    placeholderData: keepPreviousData,
  })
}

export function useDictEntry(id: number | undefined) {
  return useQuery({
    queryKey: ['dict', id],
    queryFn: () => api.dictEntry(id!),
    enabled: id !== undefined,
  })
}

export function useWords(tag?: string) {
  return useQuery({ queryKey: ['words', tag ?? null], queryFn: () => api.words(tag) })
}

export function useWord(id: number | undefined) {
  return useQuery({ queryKey: ['word', id], queryFn: () => api.word(id!), enabled: id !== undefined })
}

export function useTags() {
  return useQuery({ queryKey: ['tags'], queryFn: api.tags })
}

/** Save a word: create (no id) or update. */
export function useSaveWord(id?: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (input: WordInput) => (id ? api.updateWord(id, input) : api.createWord(input)),
    onSuccess: () => qc.invalidateQueries(),
  })
}

export function useDeleteWord() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: api.deleteWord,
    onSuccess: () => qc.invalidateQueries(),
  })
}

export function useImportWords() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: api.importWords,
    onSuccess: () => qc.invalidateQueries(),
  })
}
