import { cn } from 'cn'
import { Badge } from '@/components/ui/badge'
import { ARTICLE_CLASSES, isArticle } from '@/lib/grammar'

export function ArticleBadge({ article }: { article: string | null | undefined }) {
  if (!isArticle(article)) return null
  return (
    <Badge variant="secondary" className={cn('font-mono', ARTICLE_CLASSES[article])}>
      {article}
    </Badge>
  )
}
