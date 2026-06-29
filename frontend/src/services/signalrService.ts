import * as signalR from '@microsoft/signalr';
import { Subject, type Observable } from 'rxjs';
import { apiBaseUrl } from '@services/config';
import type { LogEntry } from '@/types';

class SignalrService {
  private connection?: signalR.HubConnection;
  private readonly logEntrySubject = new Subject<LogEntry>();

  readonly logEntries$: Observable<LogEntry> = this.logEntrySubject.asObservable();

  async start(): Promise<void> {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiBaseUrl}/hubs/timeline`, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    this.connection.on('LogEntryCreated', (entry: LogEntry) => this.logEntrySubject.next(entry));

    await this.connection.start();
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
  }
}

export const signalrService = new SignalrService();
