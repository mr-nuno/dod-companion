import { BehaviorSubject } from 'rxjs';
import { apiClient } from '@services/apiClient';
import type { Session } from '@/types';

const session$ = new BehaviorSubject<Session | null>(null);

export const sessionStore = {
  session$,

  get current(): Session | null {
    return session$.value;
  },

  async join(roomCode: string, playerName: string): Promise<Session> {
    const session = await apiClient.joinSession(roomCode, playerName);
    session$.next(session);
    return session;
  },

  async loadMe(): Promise<void> {
    try {
      session$.next(await apiClient.getMe());
    } catch {
      session$.next(null);
    }
  },

  async logout(): Promise<void> {
    await apiClient.logout();
    session$.next(null);
  },
};
