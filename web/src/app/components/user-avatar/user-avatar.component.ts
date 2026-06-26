import { Component, Input, OnChanges } from '@angular/core'
import { CommonModule } from '@angular/common'

const PALETTE = [
  '#4e79a7', '#f28e2b', '#e15759', '#76b7b2', '#59a14f',
  '#edc948', '#b07aa1', '#ff9da7', '#9c755f', '#bab0ac',
  '#6a3d9a', '#1f78b4', '#33a02c', '#e31a1c', '#ff7f00',
  '#a6cee3', '#b2df8a', '#fb9a99', '#fdbf6f', '#cab2d6',
]

function hashCode(str: string): number {
  let hash = 0
  for (let i = 0; i < str.length; i++) {
    hash = ((hash << 5) - hash + str.charCodeAt(i)) | 0
  }
  return Math.abs(hash)
}

@Component({
  selector: 'app-user-avatar',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (src) {
      <img
        [src]="src"
        [alt]="name"
        [width]="size"
        [height]="size"
        class="user-avatar user-avatar--img"
        [style.border-radius.%]="50"
        (error)="onImageError()"
      />
    } @else {
      <span
        class="user-avatar user-avatar--initials"
        [style.width.px]="size"
        [style.height.px]="size"
        [style.font-size.px]="size * 0.4"
        [style.line-height.px]="size"
        [style.background-color]="bgColor"
      >{{ initials }}</span>
    }
  `,
  styles: `
    .user-avatar--img {
      object-fit: cover;
      display: inline-block;
      vertical-align: middle;
    }
    .user-avatar--initials {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      color: #fff;
      font-weight: 600;
      text-transform: uppercase;
      vertical-align: middle;
      user-select: none;
    }
  `,
})
export class UserAvatarComponent implements OnChanges {
  @Input() userId = ''
  @Input() name = ''
  @Input() avatarUrl: string | null | undefined = null
  @Input() size = 32

  src: string | null = null
  initials = ''
  bgColor = PALETTE[0]

  ngOnChanges(): void {
    this.src = this.avatarUrl?.trim() || null
    this.initials = this.buildInitials(this.name)
    this.bgColor = this.getColor(this.userId || this.name)
  }

  onImageError(): void {
    this.src = null
  }

  private buildInitials(name: string): string {
    if (!name?.trim()) return '?'
    const parts = name.trim().split(/\s+/)
    if (parts.length === 1) return parts[0][0]
    return parts[0][0] + parts[parts.length - 1][0]
  }

  private getColor(id: string): string {
    if (!id) return PALETTE[0]
    return PALETTE[hashCode(id) % PALETTE.length]
  }
}
