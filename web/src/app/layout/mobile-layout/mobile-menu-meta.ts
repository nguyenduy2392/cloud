/**
 * Menu bottom mobile — doc lap desktop.
 * Dien `url` khi da co route; bo trong `url` = tab tam chua dieu huong.
 */
/** Nut FAB giua: hanh dong khong qua router */
export type MobileNavFabAction = 'quick-interaction'

/** Tab "Khac" / drawer phu — khong dung cung luc voi `url` */
export type MobileNavMoreAction = 'profile-drawer'

export type MobileNavItem = {
  label: string
  /** Tabler, vi du `ti ti-home` */
  icon?: string
  /** Khi co gia tri moi dung routerLink */
  url?: string
  exact?: boolean
  /** true = nut tron noi giua thanh (chi nen co mot muc) */
  center?: boolean
  /** Khi co: uu tien hon `url` (mo drawer / luong tuy chinh) */
  fabAction?: MobileNavFabAction
  /** Drawer tu canh phai (ho so / dang xuat) */
  moreAction?: MobileNavMoreAction
  permissionCode?: string
}

export const MOBILE_APP_NAV_ITEMS: MobileNavItem[] = [
  { label: 'Trang chủ', icon: 'ti ti-home', url: '/', exact: true },
  {
    label: 'Khác',
    icon: 'ti ti-menu-2',
    moreAction: 'profile-drawer',
  },
]
