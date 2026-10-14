import { CheckIcon, ChevronDownIcon, StarIcon } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { DictEntryDto } from '@/api/client'
import { ArticleBadge } from '@/components/article-badge'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { posLabel } from '@/lib/grammar'

/** How many senses to show before the "more" button. */
const VISIBLE_SENSES = 5

export function DictEntryCard({ entry }: { entry: DictEntryDto }) {
  const { t } = useTranslation()
  const pos = posLabel(t, entry.pos)
  const [expanded, setExpanded] = useState(false)
  const hidden = entry.senses.length - VISIBLE_SENSES
  const senses = expanded || hidden <= 0 ? entry.senses : entry.senses.slice(0, VISIBLE_SENSES)

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex flex-wrap items-center gap-2 text-lg">
          <ArticleBadge article={entry.gender} />
          <span>{entry.de}</span>
        </CardTitle>
        <CardDescription className="flex flex-wrap items-center gap-2">
          {pos && <Badge variant="outline">{pos}</Badge>}
          {entry.pron && <span className="font-mono text-xs">[{entry.pron}]</span>}
        </CardDescription>
        <CardAction>
          {entry.myWordId ? (
            <Button variant="ghost" size="sm" asChild>
              <Link to={`/words/${entry.myWordId}`}>
                <CheckIcon data-icon="inline-start" />
                {t('dict.inList')}
              </Link>
            </Button>
          ) : (
            <Button variant="outline" size="sm" asChild>
              <Link to={`/words/new?from=${entry.id}`}>
                <StarIcon data-icon="inline-start" />
                {t('dict.addToList')}
              </Link>
            </Button>
          )}
        </CardAction>
      </CardHeader>
      <CardContent>
        <ol className="flex flex-col gap-1.5">
          {senses.map((sense, i) => (
            <li key={i} className="flex gap-2">
              {entry.senses.length > 1 && <span className="text-muted-foreground tabular-nums">{i + 1}.</span>}
              <div className="flex flex-col">
                <span>{sense.translations.join(', ')}</span>
                {sense.definition && (
                  <span className="text-xs text-muted-foreground">{sense.definition}</span>
                )}
              </div>
            </li>
          ))}
        </ol>
        {hidden > 0 && !expanded && (
          <Button variant="link" size="sm" className="mt-1 px-0" onClick={() => setExpanded(true)}>
            <ChevronDownIcon data-icon="inline-start" />
            {t('dict.more', { count: hidden })}
          </Button>
        )}
      </CardContent>
    </Card>
  )
}
