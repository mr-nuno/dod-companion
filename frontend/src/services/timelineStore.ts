import { BehaviorSubject } from 'rxjs';
import { apiClient } from '@services/apiClient';
import { signalrService } from '@services/signalrService';
import type { LogEntry } from '@/types';

const entries$ = new BehaviorSubject<LogEntry[]>([]);
let subscribed = false;

const byTimestamp = (a: LogEntry, b: LogEntry) => a.timestamp.localeCompare(b.timestamp);

const upsert = (entry: LogEntry): void => {
  const current = entries$.value;
  const next = current.some((e) => e.id === entry.id)
    ? current.map((e) => (e.id === entry.id ? entry : e))
    : [...current, entry];
  entries$.next(next.sort(byTimestamp));
};

const removeEntry = (id: string): void => {
  entries$.next(entries$.value.filter((e) => e.id !== id));
};

export const timelineStore = {
  entries$,

  /** Seed from the REST timeline, then attach the live SignalR stream. */
  async load(): Promise<void> {
    const response = await apiClient.getTimeline();
    entries$.next([...response.entries].sort(byTimestamp));

    if (!subscribed) {
      signalrService.logEntries$.subscribe(upsert);
      signalrService.logUpdated$.subscribe(upsert);
      signalrService.logDeleted$.subscribe(removeEntry);
      signalrService.playerJoined$.subscribe((player) => {
        const entry: LogEntry = {
          id: `join-${player.name}-${Date.now()}-${Math.random()}`,
          sessionId: '',
          playerName: player.name,
          title: '',
          content: 'joined the session',
          heroImage: '',
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
  async post(title: string, content: string, tags: string[]): Promise<void> {
    await apiClient.createLogEntry(title, content, tags);
  },

  /** Edit an own entry; the change arrives back (and to everyone) via the SignalR broadcast. */
  async update(id: string, title: string, content: string, tags: string[]): Promise<void> {
    await apiClient.updateLogEntry(id, title, content, tags);
  },

  /** Delete an own entry; the removal arrives back (and to everyone) via the SignalR broadcast. */
  async remove(id: string): Promise<void> {
    await apiClient.deleteLogEntry(id);
  },

  async reset(): Promise<void> {
    entries$.next([]);
    await signalrService.stop();
  },
};
