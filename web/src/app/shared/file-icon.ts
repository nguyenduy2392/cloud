export type FileIconInfo = { icon: string; colorClass: string }

const EXT_ICON_MAP: Record<string, FileIconInfo> = {
  pdf:  { icon: 'ti-file-type-pdf', colorClass: 'text-danger' },
  doc:  { icon: 'ti-file-type-doc', colorClass: 'text-primary' },
  docx: { icon: 'ti-file-type-docx', colorClass: 'text-primary' },
  xls:  { icon: 'ti-file-type-xls', colorClass: 'text-success' },
  xlsx: { icon: 'ti-file-type-xls', colorClass: 'text-success' },
  ppt:  { icon: 'ti-file-type-ppt', colorClass: 'text-warning' },
  pptx: { icon: 'ti-file-type-ppt', colorClass: 'text-warning' },
  jpg:  { icon: 'ti-file-type-jpg', colorClass: 'text-info' },
  jpeg: { icon: 'ti-file-type-jpg', colorClass: 'text-info' },
  png:  { icon: 'ti-file-type-png', colorClass: 'text-info' },
  gif:  { icon: 'ti-photo', colorClass: 'text-info' },
  svg:  { icon: 'ti-file-type-svg', colorClass: 'text-info' },
  mp4:  { icon: 'ti-player-play', colorClass: 'text-purple' },
  avi:  { icon: 'ti-player-play', colorClass: 'text-purple' },
  mov:  { icon: 'ti-player-play', colorClass: 'text-purple' },
  mp3:  { icon: 'ti-music', colorClass: 'text-purple' },
  zip:  { icon: 'ti-file-zip', colorClass: 'text-secondary' },
  rar:  { icon: 'ti-file-zip', colorClass: 'text-secondary' },
  '7z': { icon: 'ti-file-zip', colorClass: 'text-secondary' },
  txt:  { icon: 'ti-file-text', colorClass: 'text-muted' },
  csv:  { icon: 'ti-file-spreadsheet', colorClass: 'text-success' },
}

const DEFAULT_ICON: FileIconInfo = { icon: 'ti-file', colorClass: 'text-muted' }

export function getFileIcon(fileName: string): FileIconInfo {
  const parts = fileName.split('.')
  if (parts.length < 2) return DEFAULT_ICON
  const ext = parts[parts.length - 1].toLowerCase()
  return EXT_ICON_MAP[ext] ?? DEFAULT_ICON
}
