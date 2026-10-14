import { LanguagesIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger } from '@/components/ui/select'
import { LANGUAGES } from '@/i18n'

/** UI language picker: shows the short code, the list shows full names. */
export function LanguageSwitcher() {
  const { t, i18n } = useTranslation()
  const current = i18n.resolvedLanguage ?? 'en'

  return (
    <Select value={current} onValueChange={(lng) => i18n.changeLanguage(lng)}>
      <SelectTrigger size="sm" aria-label={t('nav.language')} className="ml-1 uppercase">
        <LanguagesIcon />
        {current}
      </SelectTrigger>
      <SelectContent position="popper" align="end">
        <SelectGroup>
          {LANGUAGES.map((l) => (
            <SelectItem key={l.code} value={l.code} lang={l.code}>
              {l.label}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  )
}
