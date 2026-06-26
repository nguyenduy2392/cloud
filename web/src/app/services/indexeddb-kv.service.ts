import { Injectable } from '@angular/core'

export type IndexedDbKvOptions = {
  dbName?: string
  storeName?: string
  version?: number
  /**
   * Nếu IndexedDB không sẵn sàng/lỗi, service có thể fallback sang localStorage.
   * Mặc định: `true`.
   */
  fallbackToLocalStorage?: boolean
}

@Injectable({
  providedIn: 'root',
})
export class IndexedDbKvService {
  private readonly defaultDbName = 'miniCrm'
  private readonly defaultStoreName = 'kv'
  private readonly defaultVersion = 1

  private readonly openPromises = new Map<string, Promise<IDBDatabase>>()

  async get<T>(key: string, options: IndexedDbKvOptions = {}): Promise<T | null> {
    const { dbName, storeName, fallbackToLocalStorage } = this.normalizeOptions(
      options
    )

    if (typeof indexedDB === 'undefined') {
      return this.getFromLocalStorage<T>(key, fallbackToLocalStorage)
    }

    try {
      const db = await this.openDb(dbName, storeName, this.defaultVersion)
      return await new Promise<T | null>((resolve, reject) => {
        const tx = db.transaction(storeName, 'readonly')
        const store = tx.objectStore(storeName)
        const req = store.get(key)

        req.onsuccess = () => {
          const result = req.result as { value?: T } | undefined
          resolve(result?.value ?? null)
        }
        req.onerror = () => reject(req.error)
      })
    } catch {
      return this.getFromLocalStorage<T>(key, fallbackToLocalStorage)
    }
  }

  async set<T>(
    key: string,
    value: T,
    options: IndexedDbKvOptions = {}
  ): Promise<void> {
    const { dbName, storeName, fallbackToLocalStorage } = this.normalizeOptions(
      options
    )

    if (typeof indexedDB === 'undefined') {
      this.setToLocalStorage(key, value, fallbackToLocalStorage)
      return
    }

    try {
      const db = await this.openDb(dbName, storeName, this.defaultVersion)
      await new Promise<void>((resolve, reject) => {
        const tx = db.transaction(storeName, 'readwrite')
        const store = tx.objectStore(storeName)
        const req = store.put({ key, value })

        req.onerror = () => reject(req.error)
        tx.oncomplete = () => resolve()
        tx.onerror = () => reject(tx.error)
        tx.onabort = () => reject(tx.error)
      })
    } catch {
      this.setToLocalStorage(key, value, fallbackToLocalStorage)
    }
  }

  private normalizeOptions(options: IndexedDbKvOptions): {
    dbName: string
    storeName: string
    fallbackToLocalStorage: boolean
  } {
    return {
      dbName: options.dbName ?? this.defaultDbName,
      storeName: options.storeName ?? this.defaultStoreName,
      fallbackToLocalStorage: options.fallbackToLocalStorage ?? true,
    }
  }

  private getFromLocalStorage<T>(
    key: string,
    fallbackToLocalStorage: boolean
  ): T | null {
    if (!fallbackToLocalStorage || typeof localStorage === 'undefined') return null
    try {
      const raw = localStorage.getItem(key)
      if (!raw) return null
      return JSON.parse(raw) as T
    } catch {
      return null
    }
  }

  private setToLocalStorage<T>(
    key: string,
    value: T,
    fallbackToLocalStorage: boolean
  ): void {
    if (!fallbackToLocalStorage || typeof localStorage === 'undefined') return
    try {
      localStorage.setItem(key, JSON.stringify(value))
    } catch {
      /* ignore */
    }
  }

  private openDb(dbName: string, storeName: string, version: number): Promise<IDBDatabase> {
    const cacheKey = `${dbName}:${storeName}`
    const cached = this.openPromises.get(cacheKey)
    if (cached) return cached

    const p = new Promise<IDBDatabase>((resolve, reject) => {
      const req = indexedDB.open(dbName, version)

      req.onupgradeneeded = () => {
        const db = req.result
        if (!db.objectStoreNames.contains(storeName)) {
          db.createObjectStore(storeName, { keyPath: 'key' })
        }
      }

      req.onsuccess = () => resolve(req.result)
      req.onerror = () => reject(req.error)
    })

    this.openPromises.set(cacheKey, p)
    return p
  }
}

