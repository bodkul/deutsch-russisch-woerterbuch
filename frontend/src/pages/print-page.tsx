import { PrinterIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import { useTags, useWords, type WordDto } from '@/api/client'
import { ErrorAlert } from '@/components/error-alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import { posLabel } from '@/lib/grammar'

const collator = new Intl.Collator('de', { sensitivity: 'base' })

/** Section letter: Ä → A, Ü → U, etc. */
function letterOf(word: string): string {
  const first = word.trim().charAt(0).normalize('NFD').charAt(0)
  return /\p{L}/u.test(first) ? first.toUpperCase() : '#'
}

function groupByLetter(words: WordDto[]): [string, WordDto[]][] {
  const sorted = [...words].sort((a, b) => collator.compare(a.de, b.de))
  const groups = new Map<string, WordDto[]>()
  for (const w of sorted) {
    const letter = letterOf(w.de)
    groups.set(letter, [...(groups.get(letter) ?? []), w])
  }
  return [...groups]
}

/** Printable DE→RU glossary from own words: A4, two columns, alphabetical. */
export function PrintPage() {
  const { t } = useTranslation()
  const [params, setParams] = useSearchParams()
  const tag = params.get('tag') ?? undefined
  const words = useWords(tag)
  const tags = useTags()

  const groups = words.data ? groupByLetter(words.data) : []

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center gap-2 print:hidden">
        <h1 className="mr-auto font-heading text-2xl font-semibold">
          {t('print.title')}
          {words.data && (
            <span className="ml-2 text-base font-normal text-muted-foreground">{t('print.count', { count: words.data.length })}</span>
          )}
        </h1>
        <Button onClick={() => window.print()} disabled={!words.data?.length}>
          <PrinterIcon data-icon="inline-start" />
          {t('print.button')}
        </Button>
      </div>

      {tags.data && tags.data.length > 0 && (
        <ToggleGroup
          type="single"
          variant="outline"
          size="sm"
          value={tag ?? ''}
          onValueChange={(v) => setParams(v ? { tag: v } : {})}
          className="flex-wrap print:hidden"
        >
          {tags.data.map((t) => (
            <ToggleGroupItem key={t} value={t}>
              #{t}
            </ToggleGroupItem>
          ))}
        </ToggleGroup>
      )}

      <p className="text-sm text-muted-foreground print:hidden">
        {t('print.hint')}
      </p>

      {words.isPending && <Skeleton className="h-96 w-full rounded-xl" />}
      {words.isError && <ErrorAlert error={words.error} />}

      {words.data && (
        // The sheet is always black and white, so what is on screen matches the printout in any theme.
        <article className="mx-auto w-full max-w-[210mm] bg-white p-[12mm] text-[10pt] leading-snug text-black shadow-lg ring-1 ring-black/10 print:max-w-none print:p-0 print:shadow-none print:ring-0">
          <header className="mb-3 flex items-baseline justify-between border-b border-black pb-1">
            <span className="font-heading text-[13pt] font-semibold">Wörterbuch Deutsch – Russisch</span>
            <span className="text-[8pt] text-neutral-600">
              {tag ? `#${tag} · ` : ''}
              {words.data.length} Wörter
            </span>
          </header>

          {groups.length === 0 ? (
            <p className="text-neutral-600">{t('words.empty')}</p>
          ) : (
            <div className="columns-2 gap-[8mm] [column-rule:1px_solid_#ddd]">
              {groups.map(([letter, items]) => (
                <section key={letter}>
                  <h2 className="mt-2 mb-0.5 break-after-avoid border-b border-neutral-300 font-heading text-[11pt] font-bold first:mt-0">
                    {letter}
                  </h2>
                  {items.map((w) => (
                    <PrintEntry key={w.id} word={w} />
                  ))}
                </section>
              ))}
            </div>
          )}
        </article>
      )}
    </div>
  )
}

function PrintEntry({ word }: { word: WordDto }) {
  const { t } = useTranslation()
  // For nouns the gender is already shown by the article; part of speech is shown for the rest.
  const pos = word.article ? null : posLabel(t, word.pos)

  return (
    <p className="break-inside-avoid py-px pl-3 -indent-3">
      <b>{word.de}</b>
      {word.article && <i className="text-neutral-700">, {word.article}</i>}
      {word.plural && <span className="text-neutral-700">; Pl. {word.plural}</span>}
      {pos && <i className="text-neutral-600"> {pos}</i>}
      {' — '}
      {word.ru}
    </p>
  )
}
