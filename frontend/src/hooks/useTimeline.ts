import { useObservable } from '@hooks/useObservable';
import { timelineStore } from '@services/timelineStore';

export const useTimeline = () => {
  const entries = useObservable(timelineStore.entries$, timelineStore.entries$.value);

  return {
    entries,
    load: timelineStore.load,
    post: timelineStore.post,
    reset: timelineStore.reset,
  };
};
