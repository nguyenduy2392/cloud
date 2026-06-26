const p = (n: number) => String(n).padStart(2, '0')

/**
 * Chuyển Date giờ địa phương FE → chuỗi giờ server (UTC+offset, không có Z).
 * Dùng trước khi gửi datetime lên API.
 */
export function toServerTime(localDate: Date, serverUtcOffset: number): string {
  const d = new Date(localDate.getTime() + serverUtcOffset * 3_600_000)
  return `${d.getUTCFullYear()}-${p(d.getUTCMonth() + 1)}-${p(d.getUTCDate())}` +
    `T${p(d.getUTCHours())}:${p(d.getUTCMinutes())}:${p(d.getUTCSeconds())}`
}

/**
 * Chuyển chuỗi giờ server (UTC+offset, không có Z) → Date JS giờ địa phương FE.
 * Dùng sau khi nhận datetime từ API để hiển thị hoặc điền vào input.
 */
export function fromServerTime(serverDateStr: string, serverUtcOffset: number): Date {
  const clean = serverDateStr.replace(/Z$/, '').replace(/[+-]\d{2}:?\d{2}$/, '')
  return new Date(Date.parse(clean + 'Z') - serverUtcOffset * 3_600_000)
}

/**
 * "Từ ngày" (filter): chuỗi ngày "YYYY-MM-DD" → 00:00:00 giờ địa phương → server time.
 */
export function dateToServerTime(dateStr: string, serverUtcOffset: number): string {
  return toServerTime(new Date(dateStr + 'T00:00:00'), serverUtcOffset)
}

/**
 * "Đến ngày" (filter): chuỗi ngày "YYYY-MM-DD" → 23:59:59.999 giờ địa phương → server time.
 */
export function dateToServerTimeEnd(dateStr: string, serverUtcOffset: number): string {
  const d = new Date(dateStr + 'T23:59:59.999')
  const s = new Date(d.getTime() + serverUtcOffset * 3_600_000)
  return `${s.getUTCFullYear()}-${p(s.getUTCMonth() + 1)}-${p(s.getUTCDate())}` +
    `T${p(s.getUTCHours())}:${p(s.getUTCMinutes())}:${p(s.getUTCSeconds())}`
}

/**
 * Chuyển chuỗi giờ server → chuỗi ngày "YYYY-MM-DD" để điền vào <input type="date">.
 */
export function serverTimeToDateInput(serverDateStr: string, serverUtcOffset: number): string {
  const d = fromServerTime(serverDateStr, serverUtcOffset)
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}`
}
