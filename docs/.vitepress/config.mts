import { defineConfig } from "vitepress";
import { theme, head } from "./configs";

// https://vitepress.dev/reference/site-config
export default defineConfig({
   title: "Transceiver",
   lang: "en-US",
   description: "A message exchange library for .NET with multiple integrations",
   base: "/Transceiver/",
   themeConfig: theme,
   cleanUrls: true,
   srcDir: "./pages",
   outDir: "./dist",
   head: head,
   sitemap: {
      hostname: "https://glacorsoul.github.io/Transceiver/",
   }
});
