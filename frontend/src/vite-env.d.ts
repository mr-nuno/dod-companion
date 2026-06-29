/// <reference types="vite/client" />

interface AppConfig {
  apiBaseUrl: string;
}

interface Window {
  APP_CONFIG?: AppConfig;
}

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
