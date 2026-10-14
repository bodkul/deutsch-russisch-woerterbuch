import type { TFunction } from 'i18next'
import { en } from '@/i18n/locales/en'

export const ARTICLES = ['der', 'die', 'das'] as const
export type Article = (typeof ARTICLES)[number]

export function isArticle(value: string | null | undefined): value is Article {
  return ARTICLES.includes(value as Article)
}

/** Article colors, textbook classic: der blue, die red, das green. */
export const ARTICLE_CLASSES: Record<Article, string> = {
  der: 'bg-blue-100 text-blue-800 dark:bg-blue-950 dark:text-blue-300',
  die: 'bg-rose-100 text-rose-800 dark:bg-rose-950 dark:text-rose-300',
  das: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300',
}

type PosShortCode = keyof typeof en.posShort

/** FreeDict codes that share a label with another code. */
const POS_ALIASES: Record<string, PosShortCode> = {
  Adverb: 'adv',
  indefinitePronoun: 'pronoun',
  demonstrativePronoun: 'pronoun',
}

/** Part-of-speech codes for the select in the word form; labels are in the locale (posLong). */
export const POS_OPTIONS = ['n', 'v', 'adj', 'adv', 'preposition', 'conjunction', 'pronoun', 'numeral', 'interjection', 'phrase'] as const

/** FreeDict part of speech → form option (or '' if none matches). */
export function toPosOption(pos: string | null | undefined): string {
  if (!pos) return ''
  if (pos === 'Adverb' || pos === 'pronominalAdverb') return 'adv'
  if (pos.endsWith('Pronoun')) return 'pronoun'
  return (POS_OPTIONS as readonly string[]).includes(pos) ? pos : ''
}

/** Short localized label; an unknown code is shown as is. */
export function posLabel(t: TFunction, pos: string | null | undefined): string | null {
  if (!pos) return null
  const code = POS_ALIASES[pos] ?? pos
  return code in en.posShort ? t(`posShort.${code as PosShortCode}`) : pos
}
