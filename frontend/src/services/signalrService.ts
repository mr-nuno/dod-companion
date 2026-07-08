import * as signalR from '@microsoft/signalr';
import { Subject, type Observable } from 'rxjs';
import { apiBaseUrl } from '@services/config';
import type { LogEntry, PlayerInfo } from '@/types';

class SignalrService {
  private connection?: signalR.HubConnection;
  private readonly logEntrySubject = new Subject<LogEntry>();
  private readonly logUpdatedSubject = new Subject<LogEntry>();
  private readonly logDeletedSubject = new Subject<string>();
  private readonly playerJoinedSubject = new Subject<PlayerInfo>();

  readonly logEntries$: Observable<LogEntry> = this.logEntrySubject.asObservable();
  readonly logUpdated$: Observable<LogEntry> = this.logUpdatedSubject.asObservable();
  readonly logDeleted$: Observable<string> = this.logDeletedSubject.asObservable();
  readonly playerJoined$: Observable<PlayerInfo> = this.playerJoinedSubject.asObservable();

  async start(): Promise<void> {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiBaseUrl}/hubs/timeline`, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    this.connection.on('LogEntryCreated', (entry: LogEntry) => this.logEntrySubject.next(entry));
    this.connection.on('LogEntryUpdated', (entry: LogEntry) => this.logUpdatedSubject.next(entry));
    this.connection.on('LogEntryDeleted', (id: string) => this.logDeletedSubject.next(id));
    this.connection.on('PlayerJoined', (player: PlayerInfo) => this.playerJoinedSubject.next(player));

    await this.connection.start();
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
  }
}

export const signalrService = new SignalrService();
