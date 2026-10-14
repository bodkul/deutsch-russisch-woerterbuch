import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'
import { de } from './locales/de'
import { en } from './locales/en'
import { ru } from './locales/ru'

/** UI languages; labels are autonyms so each one is recognizable in any UI language. */
export const LANGUAGES = [
  { code: 'en', label: 'English' },
  { code: 'de', label: 'Deutsch' },
  { code: 'ru', label: 'Русский' },
] as const

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: { en: { translation: en }, de: { translation: de }, ru: { translation: ru } },
    supportedLngs: LANGUAGES.map((l) => l.code),
    load: 'languageOnly',
    fallbackLng: 'en',
    // React already escapes output.
    interpolation: { escapeValue: false },
    // The choice is remembered; on the first visit the browser language is used.
    detection: { order: ['localStorage', 'navigator'], lookupLocalStorage: 'lang', caches: ['localStorage'] },
  })

const syncHtmlLang = (lng: string) => {
  document.documentElement.lang = lng
}
syncHtmlLang(i18n.resolvedLanguage ?? 'en')
i18n.on('languageChanged', () => syncHtmlLang(i18n.resolvedLanguage ?? 'en'))

export default i18n
