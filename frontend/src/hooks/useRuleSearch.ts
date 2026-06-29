import { useState } from 'react';
import { apiClient } from '@services/apiClient';
import type { RuleSearchResult } from '@/types';

interface RuleSearchState {
  loading: boolean;
  result: RuleSearchResult | null;
  error: string | null;
}

const initialState: RuleSearchState = { loading: false, result: null, error: null };

export const useRuleSearch = () => {
  const [state, setState] = useState(initialState);

  const search = async (query: string): Promise<void> => {
    const trimmed = query.trim();
    if (!trimmed) {
      return;
    }

    setState((prev) => ({ ...prev, loading: true, error: null }));

    try {
      const result = await apiClient.searchRules(trimmed);
      setState({ loading: false, result, error: null });
    } catch (error) {
      setState({
        loading: false,
        result: null,
        error: error instanceof Error ? error.message : 'Search failed.',
      });
    }
  };

  return { ...state, search };
};
