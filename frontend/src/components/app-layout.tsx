import { BookOpenIcon, ListIcon, PlusIcon, PrinterIcon, SearchIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, Outlet, useLocation } from 'react-router'
import { LanguageSwitcher } from '@/components/language-switcher'
import { Button } from '@/components/ui/button'

const navItems = [
  { to: '/', label: 'nav.search', icon: SearchIcon },
  { to: '/words', label: 'nav.myWords', icon: ListIcon },
  { to: '/words/new', label: 'nav.add', icon: PlusIcon },
  { to: '/print', label: 'nav.print', icon: PrinterIcon },
] as const

export function AppLayout() {
  const { pathname } = useLocation()
  const { t } = useTranslation()

  return (
    <div className="flex min-h-svh flex-col">
      <header className="sticky top-0 z-10 border-b bg-background/90 backdrop-blur print:hidden">
        <nav className="mx-auto flex max-w-3xl items-center gap-1 px-4 py-2">
          <Link to="/" className="mr-auto flex items-center gap-2 font-heading font-semibold">
            <BookOpenIcon className="size-5" />
            Wörterbuch
          </Link>
          {navItems.map(({ to, label, icon: Icon }) => (
            <Button key={to} variant={pathname === to ? 'secondary' : 'ghost'} size="sm" asChild>
              <Link to={to} aria-label={t(label)}>
                <Icon data-icon="inline-start" />
                <span className="hidden sm:inline">{t(label)}</span>
              </Link>
            </Button>
          ))}
          <LanguageSwitcher />
        </nav>
      </header>

      <main className="mx-auto w-full max-w-3xl flex-1 px-4 py-6 print:max-w-none print:p-0">
        <Outlet />
      </main>

      <footer className="border-t px-4 py-3 text-center text-xs text-muted-foreground print:hidden">
        {t('footer.dictionary')}{' '}
        <a className="underline" href="https://freedict.org" target="_blank" rel="noreferrer">
          FreeDict deu-rus
        </a>{' '}
        {t('footer.data')}{' '}
        <a className="underline" href="https://creativecommons.org/licenses/by-sa/3.0/" target="_blank" rel="noreferrer">
          CC BY-SA 3.0
        </a>
        )
      </footer>
    </div>
  )
}
