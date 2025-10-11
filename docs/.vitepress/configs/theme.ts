import { DefaultTheme } from 'vitepress'
import { navbar } from './navbar'
import { sidebar } from './sidebar'

export const theme: DefaultTheme.Config = {
  logo: {
    light: '/transceiver.png',
    dark: '/transceiver.png'
  },
  siteTitle: '',
  sidebar: sidebar,
  nav: navbar,
  search: {
    provider: 'local',
  },
  outline: {
    level: [2, 3],
  },
  footer: {
    copyright:
      'Copyright © 2025-present <a target="_blank" href="https://github.com/glacorsoul">Glacor</a>, Released under <a target="_blank" href="https://github.com/glacorsoul/transceiver/blob/master/COPYING">GPL V3</a>',
  },
  socialLinks: [
    { icon: 'github', link: 'https://github.com/glacorSoul/Transceiver' },
  ],
  editLink: {
    pattern: 'https://github.com/glacorSoul/Transceiver/edit/master/docs/pages/:path',
  },
  // lastUpdated: LastUpdatedOptions
}
