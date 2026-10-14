import { zodResolver } from '@hookform/resolvers/zod'
import { cn } from 'cn'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Spinner } from '@/components/ui/spinner'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import { ARTICLE_CLASSES, ARTICLES, POS_OPTIONS } from '@/lib/grammar'
import { wordFormSchema, type WordFormMessage, type WordFormValues } from '@/lib/word-form-values'

type Props = {
  defaultValues: WordFormValues
  submitLabel: string
  submitting: boolean
  onSubmit: (values: WordFormValues) => void
  /** Validation errors from the server: field name → messages. */
  serverErrors?: Record<string, string[]>
}

export function WordForm({ defaultValues, submitLabel, submitting, onSubmit, serverErrors }: Props) {
  const form = useForm<WordFormValues>({ resolver: zodResolver(wordFormSchema), defaultValues })
  const { t } = useTranslation()
  const { errors } = form.formState
  // Client-side messages are locale keys (see wordFormSchema); server messages are shown as is.
  const errorOf = (name: keyof WordFormValues) => {
    const message = errors[name]?.message
    if (message) return { message: t(message as WordFormMessage) }
    return serverErrors?.[name[0].toUpperCase() + name.slice(1)]?.map((m) => ({ message: m }))[0]
  }

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} noValidate>
      <FieldGroup>
        <div className="grid gap-4 sm:grid-cols-[1fr_auto]">
          <Field data-invalid={!!errorOf('de')}>
            <FieldLabel htmlFor="de">{t('form.word')}</FieldLabel>
            <Input id="de" autoFocus={!defaultValues.de} aria-invalid={!!errorOf('de')} {...form.register('de')} />
            <FieldError errors={[errorOf('de')]} />
          </Field>

          <Controller
            control={form.control}
            name="article"
            render={({ field }) => (
              <Field>
                <FieldLabel>{t('form.article')}</FieldLabel>
                <ToggleGroup
                  type="single"
                  variant="outline"
                  value={field.value}
                  onValueChange={(v) => field.onChange(v)}
                >
                  {ARTICLES.map((a) => (
                    <ToggleGroupItem
                      key={a}
                      value={a}
                      className={cn('font-mono', field.value === a && ARTICLE_CLASSES[a])}
                    >
                      {a}
                    </ToggleGroupItem>
                  ))}
                </ToggleGroup>
              </Field>
            )}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Field>
            <FieldLabel htmlFor="plural">{t('form.plural')}</FieldLabel>
            <Input id="plural" placeholder="die Häuser" {...form.register('plural')} />
          </Field>

          <Controller
            control={form.control}
            name="pos"
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="pos">{t('form.pos')}</FieldLabel>
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger id="pos" className="w-full">
                    <SelectValue placeholder="—" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectGroup>
                      {POS_OPTIONS.map((o) => (
                        <SelectItem key={o} value={o}>
                          {t(`posLong.${o}`)}
                        </SelectItem>
                      ))}
                    </SelectGroup>
                  </SelectContent>
                </Select>
              </Field>
            )}
          />
        </div>

        <Field data-invalid={!!errorOf('ru')}>
          <FieldLabel htmlFor="ru">{t('form.translation')}</FieldLabel>
          <Input id="ru" aria-invalid={!!errorOf('ru')} {...form.register('ru')} />
          <FieldDescription>{t('form.translationHint')}</FieldDescription>
          <FieldError errors={[errorOf('ru')]} />
        </Field>

        <Field>
          <FieldLabel htmlFor="tags">{t('form.tags')}</FieldLabel>
          <Input id="tags" placeholder={t('form.tagsPlaceholder')} {...form.register('tags')} />
          <FieldDescription>{t('form.tagsHint')}</FieldDescription>
        </Field>

        <Button type="submit" disabled={submitting} className="self-start">
          {submitting && <Spinner data-icon="inline-start" aria-label={t('common.loading')} />}
          {submitLabel}
        </Button>
      </FieldGroup>
    </form>
  )
}
