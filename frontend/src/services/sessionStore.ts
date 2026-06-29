import { BehaviorSubject } from 'rxjs';
import { apiClient } from '@services/apiClient';
import type { CreatedRoom, Session } from '@/types';

const session$ = new BehaviorSubject<Session | null>(null);

export const sessionStore = {
  session$,

  get current(): Session | null {
    return session$.value;
  },

  // Provisions the room only — no player joins yet, so the session state is untouched. The caller
  // shows the QR, then calls join() with a player name to actually enter (and authenticate).
  create(roomName: string, hostKey: string): Promise<CreatedRoom> {
    return apiClient.createSession(roomName, hostKey);
  },

  async join(joinToken: string, playerName: string, kp: number, upptackFara: number, finnaDoldaTing: number): Promise<Session> {
    const session = await apiClient.joinSession(joinToken, playerName, kp, upptackFara, finnaDoldaTing);
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
