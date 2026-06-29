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
      subscribed = true;
    }

    await signalrService.start();
  },

  /** Post a new entry; it arrives back (and to everyone) via the SignalR broadcast. */
  async post(content: string): Promise<void> {
    await apiClient.createLogEntry(content);
  },

  async reset(): Promise<void> {
    entries$.next([]);
    await signalrService.stop();
  },
};
