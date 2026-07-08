import { BehaviorSubject } from 'rxjs';
import { apiClient } from '@services/apiClient';
import type { Session } from '@/types';

const session$ = new BehaviorSubject<Session | null>(null);

export const sessionStore = {
  session$,

  get current(): Session | null {
    return session$.value;
  },

  // Emails a single-use magic link to an allowlisted Game Master. No session state changes yet — the
  // link is consumed later (in another tab/device) via consumeCreateLink().
  requestCreateLink(email: string, roomName: string): Promise<boolean> {
    return apiClient.requestCreateLink(email, roomName);
  },

  // Consumes the magic link: creates the room and signs in as SL, then sets the session state.
  async consumeCreateLink(token: string): Promise<Session> {
    const session = await apiClient.consumeCreateLink(token);
    session$.next(session);
    return session;
  },

  async join(joinToken: string, playerName: string, kp: number, upptackFara: number, finnaDoldaTing: number, isDm: boolean = false): Promise<Session> {
    const session = await apiClient.joinSession(joinToken, playerName, kp, upptackFara, finnaDoldaTing, isDm);
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
