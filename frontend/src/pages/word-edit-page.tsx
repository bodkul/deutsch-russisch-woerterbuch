import { useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { ArrowLeftIcon, Trash2Icon } from 'lucide-react'
import { useNavigate, useParams, useSearchParams } from 'react-router'
import { toast } from 'sonner'
import { api, ApiError, useDeleteWord, useDictEntry, useSaveWord, useWord, type DictEntryDto, type WordDto } from '@/api/client'
import { ErrorAlert } from '@/components/error-alert'
import { WordForm } from '@/components/word-form'
import { emptyWordForm, toWordInput, type WordFormValues } from '@/lib/word-form-values'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { isArticle, toPosOption } from '@/lib/grammar'

function fromWord(w: WordDto): WordFormValues {
  return {
    de: w.de,
    article: isArticle(w.article) ? w.article : '',
    plural: w.plural ?? '',
    pos: w.pos ?? '',
    ru: w.ru,
    tags: w.tags.join(', '),
  }
}

function fromDictEntry(e: DictEntryDto): WordFormValues {
  return {
    ...emptyWordForm,
    de: e.de,
    article: isArticle(e.gender) ? e.gender : '',
    pos: toPosOption(e.pos),
    ru: e.senses.map((s) => s.translations.join(', ')).join('; '),
  }
}

/** /words/new (?from=<dictionary entry id> | ?de=<word>) and /words/:id. */
export function WordEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { id: idParam } = useParams()
  const [params] = useSearchParams()

  const id = idParam ? Number(idParam) : undefined
  const fromId = params.get('from') ? Number(params.get('from')) : undefined

  const word = useWord(id)
  const dict = useDictEntry(id ? undefined : fromId)
  const save = useSaveWord(id)
  const remove = useDeleteWord()
  const queryClient = useQueryClient()

  const loading = word.isLoading || dict.isLoading
  const loadError = word.error ?? dict.error

  const defaults: WordFormValues | undefined = word.data
    ? fromWord(word.data)
    : dict.data
      ? fromDictEntry(dict.data)
      : id || fromId
        ? undefined
        : { ...emptyWordForm, de: params.get('de') ?? '' }

  const dictEntryId = word.data?.dictEntryId ?? dict.data?.id ?? null

  const handleSubmit = (values: WordFormValues) =>
    save.mutate(toWordInput(values, dictEntryId), {
      onSuccess: (saved) => {
        toast.success(id ? t('edit.saved') : t('edit.added', { word: saved.de }))
        if (id) navigate(-1)
        else navigate('/words')
      },
      onError: (e) => toast.error(e.message),
    })

  const handleDelete = () => {
    const deleted = word.data!
    remove.mutate(deleted.id, {
      onSuccess: () => {
        toast.success(t('edit.deleted', { word: deleted.de }), {
          duration: 10_000,
          // Restore as a new word (the id will differ). By now the page is
          // already closed, so work with api and queryClient directly rather than the form hooks.
          action: {
            label: t('edit.undo'),
            onClick: () =>
              api
                .createWord({ ...deleted })
                .then(() => {
                  queryClient.invalidateQueries()
                  toast.success(t('edit.restored', { word: deleted.de }))
                })
                .catch((e: Error) => toast.error(e.message)),
          },
        })
        navigate('/words')
      },
      onError: (e) => toast.error(e.message),
    })
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label={t('edit.back')}>
          <ArrowLeftIcon />
        </Button>
        <h1 className="mr-auto font-heading text-2xl font-semibold">
          {id ? t('edit.titleEdit') : t('edit.titleNew')}
        </h1>

        {id && word.data && (
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button variant="destructive" size="sm">
                <Trash2Icon data-icon="inline-start" />
                {t('edit.delete')}
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>{t('edit.deleteTitle', { word: word.data.de })}</AlertDialogTitle>
                <AlertDialogDescription>{t('edit.deleteDescription')}</AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>{t('edit.cancel')}</AlertDialogCancel>
                <AlertDialogAction variant="destructive" onClick={handleDelete}>
                  {t('edit.delete')}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        )}
      </div>

      {loading && <Skeleton className="h-96 w-full rounded-xl" />}
      {loadError && <ErrorAlert error={loadError} />}

      {defaults && (
        <WordForm
          key={`${id ?? 'new'}-${fromId ?? ''}`}
          defaultValues={defaults}
          submitLabel={id ? t('edit.save') : t('edit.addToList')}
          submitting={save.isPending}
          onSubmit={handleSubmit}
          serverErrors={save.error instanceof ApiError ? save.error.errors : undefined}
        />
      )}
    </div>
  )
}
