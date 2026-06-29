import { BehaviorSubject } from 'rxjs';
import { apiClient } from '@services/apiClient';
import { signalrService } from '@services/signalrService';
import type { PlayerInfo } from '@/types';

const players$ = new BehaviorSubject<PlayerInfo[]>([]);
let subscribed = false;

const upsert = (player: PlayerInfo): void => {
  const current = players$.value;
  const idx = current.findIndex((p) => p.name.toLowerCase() === player.name.toLowerCase());
  if (idx >= 0) {
    const updated = [...current];
    updated[idx] = player;
    players$.next(updated);
  } else {
    players$.next([...current, player]);
  }
};

export const rosterStore = {
  players$,

  async load(): Promise<void> {
    const response = await apiClient.getPlayers();
    players$.next(response.players);

    if (!subscribed) {
      signalrService.playerJoined$.subscribe(upsert);
      subscribed = true;
    }
  },

  reset(): void {
    players$.next([]);
    subscribed = false;
  },
};
