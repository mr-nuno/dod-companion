export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  validationErrors?: { identifier: string; errorMessage: string }[];
}

export interface Session {
  sessionId: string;
  playerName: string;
}

export interface LogEntry {
  id: string;
  sessionId: string;
  playerName: string;
  content: string;
  timestamp: string;
}

export interface TimelineResponse {
  entries: LogEntry[];
}

export interface RuleSearchHit {
  sourceFileName: string;
  physicalPageNumber: number;
  header?: string;
  content: string;
  tags: string[];
  searchScore: number;
}

export interface RuleSearchResult {
  query: string;
  totalHits: number;
  results: RuleSearchHit[];
}
