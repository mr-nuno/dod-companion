export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  validationErrors?: { identifier: string; errorMessage: string }[];
}

export interface Session {
  sessionId: string;
  playerName: string;
  roomCode: string;
  joinToken: string;
}

// A freshly provisioned room, before any player has joined it.
export interface CreatedRoom {
  sessionId: string;
  roomCode: string;
  joinToken: string;
}

export interface LogEntry {
  id: string;
  sessionId: string;
  playerName: string;
  content: string;
  timestamp: string;
  tags: string[];
}

export interface TimelineResponse {
  entries: LogEntry[];
}

export interface PlayerInfo {
  name: string;
  kp: number;
  upptackFara: number;
  finnaDoldaTing: number;
}

export interface PlayersResponse {
  players: PlayerInfo[];
}

export interface RuleSearchHit {
  sourceFileName: string;
  physicalPageNumber: number;
  header?: string;
  content: string;
  tags: string[];
  searchScore: number;
  pageModifier: number;
}

export interface RuleSearchResult {
  query: string;
  processedQuery?: string;
  totalHits: number;
  results: RuleSearchHit[];
}

export interface SessionSummary {
  id: string;
  sessionId: string;
  roomCode: string;
  content: string;
  entryCount: number;
  generatedAt: string;
}
