import { CircleAlertIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'

/** Load error callout: a generic title plus the message from the API. */
export function ErrorAlert({ error }: { error: Error }) {
  const { t } = useTranslation()
  return (
    <Alert variant="destructive">
      <CircleAlertIcon />
      <AlertTitle>{t('errors.title')}</AlertTitle>
      <AlertDescription>{error.message}</AlertDescription>
    </Alert>
  )
}
