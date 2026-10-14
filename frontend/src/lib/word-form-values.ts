import { z } from 'zod'
import type { WordInput } from '@/api/client'
import { ARTICLES } from '@/lib/grammar'

/** Validation messages are locale keys; the form translates them. */
export type WordFormMessage = 'form.enterWord' | 'form.enterTranslation'

export const wordFormSchema = z.object({
  de: z.string().trim().min(1, 'form.enterWord' satisfies WordFormMessage).max(200),
  article: z.enum(ARTICLES).or(z.literal('')),
  plural: z.string().trim().max(200),
  pos: z.string(),
  ru: z.string().trim().min(1, 'form.enterTranslation' satisfies WordFormMessage).max(2000),
  tags: z.string(),
})

export type WordFormValues = z.infer<typeof wordFormSchema>

export const emptyWordForm: WordFormValues = {
  de: '',
  article: '',
  plural: '',
  pos: '',
  ru: '',
  tags: '',
}

/** Form values → API request body (empty strings → null, comma-separated tags → array). */
export function toWordInput(v: WordFormValues, dictEntryId: number | null): WordInput {
  const orNull = (s: string) => (s.trim() ? s.trim() : null)
  return {
    de: v.de.trim(),
    article: v.article || null,
    plural: orNull(v.plural),
    pos: orNull(v.pos),
    ru: v.ru.trim(),
    tags: v.tags.split(',').map((t) => t.trim()).filter(Boolean),
    dictEntryId,
  }
}
