import { useObservable } from '@hooks/useObservable';
import { rosterStore } from '@services/rosterStore';

export const useRoster = () => {
  const players = useObservable(rosterStore.players$, rosterStore.players$.value);

  return {
    players,
    load: rosterStore.load,
    reset: rosterStore.reset,
  };
};
