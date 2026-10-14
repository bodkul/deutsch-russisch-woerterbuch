import { BookmarkIcon, PlusIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router'
import { useTags, useWords } from '@/api/client'
import { ErrorAlert } from '@/components/error-alert'
import { WordBackupButtons } from '@/components/word-backup-buttons'
import { WordList } from '@/components/word-list'
import { Button } from '@/components/ui/button'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Skeleton } from '@/components/ui/skeleton'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'

export function WordsPage() {
  const { t } = useTranslation()
  const [params, setParams] = useSearchParams()
  const tag = params.get('tag') ?? undefined
  const words = useWords(tag)
  const tags = useTags()

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center gap-2">
        <h1 className="mr-auto font-heading text-2xl font-semibold">
          {t('words.title')}
          {words.data && (
            <span className="ml-2 text-base font-normal text-muted-foreground">{words.data.length}</span>
          )}
        </h1>
        <WordBackupButtons />
        <Button size="sm" asChild>
          <Link to="/words/new">
            <PlusIcon data-icon="inline-start" />
            {t('words.add')}
          </Link>
        </Button>
      </div>

      {tags.data && tags.data.length > 0 && (
        <ToggleGroup
          type="single"
          variant="outline"
          size="sm"
          value={tag ?? ''}
          onValueChange={(v) => setParams(v ? { tag: v } : {})}
          className="flex-wrap"
        >
          {tags.data.map((t) => (
            <ToggleGroupItem key={t} value={t}>
              #{t}
            </ToggleGroupItem>
          ))}
        </ToggleGroup>
      )}

      {words.isPending && <Skeleton className="h-28 w-full rounded-xl" />}
      {words.isError && <ErrorAlert error={words.error} />}

      {words.data?.length === 0 && (
        <Empty>
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <BookmarkIcon />
            </EmptyMedia>
            <EmptyTitle>{tag ? t('words.emptyTag', { tag }) : t('words.empty')}</EmptyTitle>
            <EmptyDescription>
              {t('words.emptyHint')}
            </EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            <Button variant="outline" asChild>
              <Link to="/">{t('words.toSearch')}</Link>
            </Button>
          </EmptyContent>
        </Empty>
      )}

      {words.data && words.data.length > 0 && <WordList words={words.data} />}
    </div>
  )
}
