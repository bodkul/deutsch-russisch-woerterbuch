import { SearchIcon, SearchXIcon } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router'
import { useSearch } from '@/api/client'
import { ErrorAlert } from '@/components/error-alert'
import { DictEntryCard } from '@/components/dict-entry-card'
import { WordList } from '@/components/word-list'
import { Button } from '@/components/ui/button'
import { Empty, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { InputGroup, InputGroupAddon, InputGroupInput } from '@/components/ui/input-group'
import { Skeleton } from '@/components/ui/skeleton'
import { Spinner } from '@/components/ui/spinner'
import { useDebouncedValue } from '@/hooks/use-debounced-value'

export function SearchPage() {
  const { t } = useTranslation()
  const [params, setParams] = useSearchParams()
  const [text, setText] = useState(params.get('q') ?? '')
  const query = useDebouncedValue(text.trim(), 200)

  // The query lives in the URL so that "back" from an entry returns to the results.
  useEffect(() => {
    setParams(query ? { q: query } : {}, { replace: true })
  }, [query, setParams])

  const { data, isFetching, isError, error } = useSearch(query)
  const hasResults = !!data && (data.words.length > 0 || data.dict.length > 0)

  return (
    <div className="flex flex-col gap-6">
      <InputGroup className="h-12">
        <InputGroupInput
          autoFocus
          type="search"
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder="Haus, Straße, дом…"
          aria-label={t('search.ariaLabel')}
          className="text-lg md:text-lg"
        />
        <InputGroupAddon>
          <SearchIcon />
        </InputGroupAddon>
        {isFetching && (
          <InputGroupAddon align="inline-end">
            <Spinner aria-label={t('common.loading')} />
          </InputGroupAddon>
        )}
      </InputGroup>

      {!query && (
        <p className="text-center text-sm text-muted-foreground">
          {t('search.hint')}
        </p>
      )}

      {isError && <ErrorAlert error={error} />}

      {query && !data && isFetching && <ResultsSkeleton />}

      {query && data && !hasResults && !isFetching && (
        <Empty>
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <SearchXIcon />
            </EmptyMedia>
            <EmptyTitle>{t('search.nothingFound')}</EmptyTitle>
            <EmptyDescription>
              {t('search.baseFormsHint')}
            </EmptyDescription>
          </EmptyHeader>
          {data.direction === 'de' && (
            <Button asChild>
              <Link to={`/words/new?de=${encodeURIComponent(query)}`}>{t('search.addToList', { query })}</Link>
            </Button>
          )}
        </Empty>
      )}

      {data && data.words.length > 0 && (
        <section className="flex flex-col gap-3">
          <h2 className="text-sm font-medium text-muted-foreground">{t('search.myWords')}</h2>
          <WordList words={data.words} />
        </section>
      )}

      {data && data.dict.length > 0 && (
        <section className="flex flex-col gap-3">
          <h2 className="text-sm font-medium text-muted-foreground">{t('search.dictionary')}</h2>
          {data.dict.map((e) => (
            <DictEntryCard key={e.id} entry={e} />
          ))}
        </section>
      )}
    </div>
  )
}

function ResultsSkeleton() {
  return (
    <div className="flex flex-col gap-3">
      {[0, 1, 2].map((i) => (
        <Skeleton key={i} className="h-28 w-full rounded-xl" />
      ))}
    </div>
  )
}
