import type { HeadConfig } from 'vitepress';

export const head: HeadConfig[] = [
   // favicon
   ['link', { rel: 'icon', href: '/Transceiver/transceiver.png', type: "image/x-icon" }],
   ['link', { rel: 'shortcut icon', href: '/Transceiver/transceiver.png', type: "image/x-icon" }],

   // social media image
   ['meta', { property: 'og:image', content: 'https://glacorSoul.github.io/Transceiver/transceiver.png' }],
   ['meta', { property: 'og:image:type', content: 'image/png' }],
   ['meta', { property: 'og:image:width', content: '512' }],
   ['meta', { property: 'og:image:height', content: '512' }],
   ['meta', { property: 'og:title', content: 'Transceiver' }],
   ['meta', { property: 'og:type', content: 'website' }],
   ['meta', { property: 'og:url', content: 'https://glacorSoul.github.io/Transceiver/' }],
   ['meta', { property: 'og:description', content: 'A message exchange library for .NET with multiple integrations' }],
   ['meta', { property: 'twitter:card', content: 'summary_large_image' }],
]
