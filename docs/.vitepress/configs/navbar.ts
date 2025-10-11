import type { DefaultTheme } from 'vitepress'

export const navbar: DefaultTheme.NavItem[] = [
   {
      text: 'Welcome',
      link: '/',
   },
   {
      text: 'Guide',
      link: '/guide/',
      activeMatch: '^\/guide\/*.*$'
      
   },
   {
      text: 'Examples',
      items: [
         {
            text: 'Using Transceiver in API Controllers',
            link: '/example/transceiver',
            activeMatch: '/example/vuepress/',
         }
      ]
   },
   {
      text: 'Contribution',
      link: '/contribution/',
   }
]
