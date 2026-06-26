import { CommonModule } from '@angular/common'
import { Component, OnInit, OnDestroy, inject } from '@angular/core'
import { Router, RouterModule } from '@angular/router'
import { NgbCollapseModule } from '@ng-bootstrap/ng-bootstrap'
import { Subject, takeUntil, forkJoin } from 'rxjs'
import { getFileIcon, type FileIconInfo } from '@/app/shared/file-icon'
import {
  FolderService,
  type FolderItem,
  type FileItem,
} from '@/app/services/folder.service'
import { ResourcePermissionService } from '@/app/services/resource-permission.service'

interface HomeSection {
  key: string
  title: string
  icon: string
  collapsed: boolean
  folders: FolderItem[]
  files: FileItem[]
  loading: boolean
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, NgbCollapseModule],
  templateUrl: './home.component.html',
  styles: `
    :host { display: block; }
    .home-section { margin-bottom: 0.5rem; }
    .home-section__header {
      display: flex;
      align-items: center;
      padding: 10px 16px;
      cursor: pointer;
      user-select: none;
      font-weight: 600;
      font-size: 0.9375rem;
      border-radius: 6px;
      transition: background 0.15s;
    }
    .home-section__header:hover {
      background: var(--greeva-tertiary-bg);
    }
    .home-section__arrow {
      transition: transform 0.2s;
      margin-right: 8px;
      font-size: 1.125rem;
    }
    .home-section__arrow--open {
      transform: rotate(90deg);
    }
    .home-section__count {
      font-weight: 400;
      font-size: 0.8125rem;
      margin-left: 6px;
    }
    .home-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 8px;
      padding: 8px 16px 16px;
    }
    @media (min-width: 576px) { .home-grid { grid-template-columns: repeat(3, 1fr); } }
    @media (min-width: 992px) { .home-grid { grid-template-columns: repeat(5, 1fr); } }
    @media (min-width: 1400px) { .home-grid { grid-template-columns: repeat(6, 1fr); } }
    .home-item {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 12px;
      border-radius: 6px;
      cursor: pointer;
      transition: background 0.15s;
      overflow: hidden;
      border: 1px solid var(--greeva-border-color-translucent);
    }
    .home-item:hover {
      background: var(--greeva-tertiary-bg);
    }
    .home-item__icon {
      font-size: 1.25rem;
      flex-shrink: 0;
    }
    .home-item__name {
      font-size: 0.8125rem;
      font-weight: 500;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .folder-icon { color: #f0ad4e; }
  `,
})
export class HomeComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>()
  private router = inject(Router)
  private folderService = inject(FolderService)
  private permissionService = inject(ResourcePermissionService)

  sections: HomeSection[] = [
    { key: 'my-folders', title: 'Thư mục của tôi', icon: 'ti ti-folder', collapsed: false, folders: [], files: [], loading: true },
    { key: 'shared-folders', title: 'Thư mục được chia sẻ', icon: 'ti ti-folder-share', collapsed: false, folders: [], files: [], loading: true },
    { key: 'my-files', title: 'Tệp của tôi', icon: 'ti ti-file', collapsed: false, folders: [], files: [], loading: true },
    { key: 'shared-files', title: 'Tệp được chia sẻ', icon: 'ti ti-file-symlink', collapsed: false, folders: [], files: [], loading: true },
  ]

  ngOnInit(): void {
    this.loadData()
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  private loadData(): void {
    const myData$ = this.folderService.getContents(undefined, 1, 50)
    const sharedData$ = this.permissionService.getSharedWithMe(1, 50)

    forkJoin([myData$, sharedData$])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([myRes, sharedRes]) => {
        const myFolders = this.section('my-folders')
        const myFiles = this.section('my-files')
        const sharedFolders = this.section('shared-folders')
        const sharedFiles = this.section('shared-files')

        if (myRes?.isSuccess) {
          const d: any = myRes.data
          myFolders.folders = d?.folders?.items ?? []
          myFiles.files = d?.files?.items ?? []
        }
        myFolders.loading = false
        myFiles.loading = false

        if (sharedRes?.isSuccess) {
          const d: any = sharedRes.data
          sharedFolders.folders = d?.folders?.items ?? []
          sharedFiles.files = d?.files?.items ?? []
        }
        sharedFolders.loading = false
        sharedFiles.loading = false
      })
  }

  private section(key: string): HomeSection {
    return this.sections.find((s) => s.key === key)!
  }

  toggle(section: HomeSection): void {
    section.collapsed = !section.collapsed
  }

  itemCount(section: HomeSection): number {
    return section.folders.length + section.files.length
  }

  openFolder(folder: FolderItem, shared: boolean): void {
    const qp = shared ? { queryParams: { from: 'shared' } } : {}
    this.router.navigate(['/folder', folder.id], qp)
  }

  getFileIcon(fileName: string): FileIconInfo {
    return getFileIcon(fileName)
  }
}
