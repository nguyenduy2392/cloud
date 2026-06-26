export type MenuItemType = {
  key: string
  label: string
  isTitle?: boolean
  icon?: string
  url?: string
  permissionCode?: string
  badge?: {
    variant: string
    text: string
  }
  parentKey?: string
  isDisabled?: boolean
  collapsed?: boolean
  children?: MenuItemType[]
}

export type SubMenus = {
  item: MenuItemType
  linkClassName?: string
  subMenuClassName?: string
  activeMenuItems?: Array<string>
  toggleMenu?: (item: MenuItemType, status: boolean) => void
  className?: string
}

/**
 * Menu ung dung chinh — dung chung cho sidebar (`MENU_ITEMS`) va menu ngang (`HORIZONTAL_MENU_ITEM`).
 */
export const APP_PRIMARY_MENU: MenuItemType[] = [
  {
    key: 'home',
    label: 'Home',
    icon: 'ti ti-home',
    url: '/',
  },
  {
    key: 'my-cloud',
    label: 'Cloud của tôi',
    icon: 'ti ti-cloud',
    url: '/my-cloud',
  },
  {
    key: 'shared-with-me',
    label: 'Chia sẻ với tôi',
    icon: 'ti ti-share',
    url: '/shared-with-me',
  },
  {
    key: 'recent',
    label: 'Gần đây',
    icon: 'ti ti-clock',
    url: '/recent',
  },
]

export const MENU_ITEMS: MenuItemType[] = [
  {
    key: 'nav',
    label: 'Menu',
    isTitle: true,
  },
  ...APP_PRIMARY_MENU
]

/** Menu ngang: cung noi dung voi phan dau sidebar; bo `collapsed` (chi dung sidebar). */
export const HORIZONTAL_MENU_ITEM: MenuItemType[] = APP_PRIMARY_MENU.map((item) => {
  const { collapsed: _c, ...rest } = item
  if (!rest.children?.length) {
    return { ...rest }
  }
  return {
    ...rest,
    children: rest.children.map((ch) => {
      const { collapsed: _cc, ...childRest } = ch
      return { ...childRest }
    }),
  }
})
