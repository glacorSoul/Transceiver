import type { DefaultTheme } from 'vitepress'

export const sidebar: DefaultTheme.Sidebar = {
   '/guide/': [
      {
         text: 'Introduction',
         items: [
            { text: 'Introduction', link: '/guide/' },
         ],
      },
      {
         text: 'Service Discovery',
         items: [
            { text: 'Service Discovey', link: '/guide/discovery' },
            { text: 'Code generation', link: '/guide/codeGen' },
         ]
      },
      {
         text: 'Telemetry',
         items: [
            { text: 'Metrics and Counters', link: '/guide/telemetry' },
         ]
      },
      {
         text: 'Configuration',
         items: [
            { text: 'Configuration', link: '/guide/configuration' },
         ]
      },
      {
         text: 'Integrations',
         items: [
            { text: 'Domain Sockets', link: '/guide/Integrations/domainSockets' },
            { text: 'Amazon Sqs', link: '/guide/Integrations/awsSqs' },
            { text: 'Azure Queues', link: '/guide/Integrations/azureQueues' },
            { text: 'Google Pub/Sub', link: '/guide/Integrations/googlePubSub' },
            { text: 'RabbitMQ', link: '/guide/Integrations/rabbitMQ' },
            { text: 'Kafka', link: '/guide/Integrations/kafka' },
            { text: 'ZeroMQ', link: '/guide/Integrations/zeroMQ' },
         ]
      }
   ],

}
