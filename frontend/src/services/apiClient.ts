import { apiBaseUrl } from '@services/config';
import type {
  ApiResponse,
  CreatedRoom,
  LogEntry,
  PlayersResponse,
  RuleSearchResult,
  Session,
  SessionSummary,
  TimelineResponse,
} from '@/types';

export class ApiError extends Error {
  constructor(message: string, readonly status: number) {
    super(message);
    this.name = 'ApiError';
  }
}

const request = async <T>(path: string, init?: RequestInit): Promise<T> => {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    credentials: 'include', // send/receive the HttpOnly auth cookie
    ...init,
  });

  const envelope = (await response.json().catch(() => undefined)) as ApiResponse<T> | undefined;

  if (!response.ok || !envelope?.success) {
    const message =
      envelope?.error ??
      envelope?.validationErrors?.map((v) => v.errorMessage).join(' ') ??
      `Request failed (${response.status}).`;
    throw new ApiError(message, response.status);
  }

  return envelope.data as T;
};

// JSON bodies require Content-Type for the BFF to bind them (deviates from the usual no-header rule).
const jsonBody = (payload: unknown, method = 'POST'): RequestInit => ({
  method,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(payload),
});

export const apiClient = {
  createSession: (roomName: string, hostKey: string) =>
    request<CreatedRoom>('/sessions/create', jsonBody({ roomName, hostKey })),

  joinSession: (joinToken: string, playerName: string, kp: number, upptackFara: number, finnaDoldaTing: number, isDm: boolean) =>
    request<Session>('/sessions/join', jsonBody({ joinToken, playerName, kp, upptackFara, finnaDoldaTing, isDm })),

  getMe: () => request<Session>('/sessions/me'),

  logout: () => request<unknown>('/sessions/logout', { method: 'POST' }),

  getPlayers: () => request<PlayersResponse>('/sessions/players'),

  createLogEntry: (title: string, content: string, tags: string[]) =>
    request<LogEntry>('/log-entries', jsonBody({ title, content, tags })),

  updateLogEntry: (id: string, title: string, content: string, tags: string[]) =>
    request<LogEntry>(`/log-entries/${encodeURIComponent(id)}`, jsonBody({ title, content, tags }, 'PUT')),

  deleteLogEntry: (id: string) =>
    request<boolean>(`/log-entries/${encodeURIComponent(id)}`, { method: 'DELETE' }),

  getTimeline: () => request<TimelineResponse>('/log-entries'),

  searchRules: (query: string) =>
    request<RuleSearchResult>(`/rules/search?query=${encodeURIComponent(query)}`),

  generateSummary: () =>
    request<SessionSummary>('/sessions/summary', { method: 'POST' }),

  getSummary: () =>
    request<SessionSummary>('/sessions/summary'),
};
