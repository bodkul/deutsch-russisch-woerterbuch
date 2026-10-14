import { PencilIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { WordDto } from '@/api/client'
import { ArticleBadge } from '@/components/article-badge'
import { Button } from '@/components/ui/button'

/** Compact list of own words: one row per word. */
export function WordList({ words }: { words: WordDto[] }) {
  return (
    <ul className="divide-y rounded-xl ring-1 ring-foreground/10">
      {words.map((w) => (
        <WordRow key={w.id} word={w} />
      ))}
    </ul>
  )
}

function WordRow({ word }: { word: WordDto }) {
  const { t } = useTranslation()
  return (
    <li className="flex items-center gap-2 px-3 py-2">
      <div className="flex min-w-0 flex-1 flex-wrap items-baseline gap-x-2 gap-y-0.5">
        <span className="flex items-center gap-1.5 font-medium">
          <ArticleBadge article={word.article} />
          {word.de}
        </span>
        {word.plural && <span className="text-xs text-muted-foreground">{t('words.plural', { plural: word.plural })}</span>}
        <span className="text-muted-foreground">—</span>
        <span>{word.ru}</span>
        {word.tags.map((tag) => (
          <Link
            key={tag}
            to={`/words?tag=${encodeURIComponent(tag)}`}
            className="text-xs text-muted-foreground hover:text-foreground"
          >
            #{tag}
          </Link>
        ))}
      </div>
      <Button variant="ghost" size="icon-sm" asChild>
        <Link to={`/words/${word.id}`} aria-label={t('words.edit', { word: word.de })}>
          <PencilIcon />
        </Link>
      </Button>
    </li>
  )
}
