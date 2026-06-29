// Resolves the BFF base URL: runtime-injected window.APP_CONFIG (Docker) wins,
// then a Vite build-time env var, then same-origin (when served behind a reverse proxy).
const fromWindow = typeof window !== 'undefined' ? window.APP_CONFIG?.apiBaseUrl : undefined;

export const apiBaseUrl = (fromWindow || import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '');
