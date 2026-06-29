import { BehaviorSubject } from 'rxjs';
import { apiClient } from '@services/apiClient';
import { signalrService } from '@services/signalrService';
import type { LogEntry } from '@/types';

const entries$ = new BehaviorSubject<LogEntry[]>([]);
let subscribed = false;

const byTimestamp = (a: LogEntry, b: LogEntry) => a.timestamp.localeCompare(b.timestamp);

const upsert = (entry: LogEntry): void => {
  const current = entries$.value;
  if (current.some((e) => e.id === entry.id)) {
    return;
  }
  entries$.next([...current, entry].sort(byTimestamp));
};

export const timelineStore = {
  entries$,

  /** Seed from the REST timeline, then attach the live SignalR stream. */
  async load(): Promise<void> {
    const response = await apiClient.getTimeline();
    entries$.next([...response.entries].sort(byTimestamp));

    if (!subscribed) {
      signalrService.logEntries$.subscribe(upsert);
      signalrService.playerJoined$.subscribe((player) => {
        const entry: LogEntry = {
          id: `join-${player.name}-${Date.now()}-${Math.random()}`,
          sessionId: '',
          playerName: player.name,
          content: 'joined the session',
          timestamp: new Date().toISOString(),
          tags: ['info'],
        };
        upsert(entry);
      });
      subscribed = true;
    }

    await signalrService.start();
  },

  /** Post a new entry; it arrives back (and to everyone) via the SignalR broadcast. */
  async post(content: string, tags: string[]): Promise<void> {
    await apiClient.createLogEntry(content, tags);
  },

  async reset(): Promise<void> {
    entries$.next([]);
    await signalrService.stop();
  },
};
