import { DownloadIcon, UploadIcon } from 'lucide-react'
import { useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { api, ApiError, useImportWords, type WordExportFile } from '@/api/client'
import { Button } from '@/components/ui/button'

/** Export own words to JSON and import them back (import only adds). */
export function WordBackupButtons() {
  const fileInput = useRef<HTMLInputElement>(null)
  const { t } = useTranslation()
  const importWords = useImportWords()

  const handleFile = async (file: File | undefined) => {
    if (!file) return
    try {
      const data = JSON.parse(await file.text()) as WordExportFile
      const result = await importWords.mutateAsync(data)
      toast.success(t('backup.added', { count: result.added }), {
        description: result.skipped ? t('backup.skipped', { count: result.skipped }) : undefined,
      })
    } catch (e) {
      // The server rejects a foreign file with 400 and an English message, so show our own.
      const notExport = e instanceof SyntaxError || (e instanceof ApiError && e.status === 400)
      toast.error(notExport ? t('backup.notExport') : (e as Error).message)
    } finally {
      if (fileInput.current) fileInput.current.value = ''
    }
  }

  return (
    <>
      <Button variant="outline" size="sm" asChild>
        <a href={api.exportUrl} download>
          <DownloadIcon data-icon="inline-start" />
          {t('backup.export')}
        </a>
      </Button>
      <Button variant="outline" size="sm" disabled={importWords.isPending} onClick={() => fileInput.current?.click()}>
        <UploadIcon data-icon="inline-start" />
        {t('backup.import')}
      </Button>
      <input
        ref={fileInput}
        type="file"
        accept="application/json,.json"
        className="hidden"
        onChange={(e) => handleFile(e.target.files?.[0])}
      />
    </>
  )
}
