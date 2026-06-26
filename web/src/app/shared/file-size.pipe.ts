import { Pipe, PipeTransform } from '@angular/core'

const UNITS = ['B', 'KB', 'MB', 'GB', 'TB']

export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B'
  const k = 1024
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  const idx = Math.min(i, UNITS.length - 1)
  const value = bytes / Math.pow(k, idx)
  return `${parseFloat(value.toFixed(idx === 0 ? 0 : 1))} ${UNITS[idx]}`
}

@Pipe({ name: 'fileSize', standalone: true })
export class FileSizePipe implements PipeTransform {
  transform(bytes: number | null | undefined): string {
    if (bytes == null) return '0 B'
    return formatFileSize(bytes)
  }
}
