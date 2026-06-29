import { apiBaseUrl } from '@services/config';
import type {
  ApiResponse,
  LogEntry,
  RuleSearchResult,
  Session,
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
const jsonBody = (payload: unknown): RequestInit => ({
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(payload),
});

export const apiClient = {
  joinSession: (roomCode: string, playerName: string) =>
    request<Session>('/sessions/join', jsonBody({ roomCode, playerName })),

  getMe: () => request<Session>('/sessions/me'),

  logout: () => request<unknown>('/sessions/logout', { method: 'POST' }),

  createLogEntry: (content: string) =>
    request<LogEntry>('/log-entries', jsonBody({ content })),

  getTimeline: () => request<TimelineResponse>('/log-entries'),

  searchRules: (query: string) =>
    request<RuleSearchResult>(`/rules/search?query=${encodeURIComponent(query)}`),
};
