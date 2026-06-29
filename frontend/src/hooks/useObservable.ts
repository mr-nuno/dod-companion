import { useEffect, useState } from 'react';
import type { Observable } from 'rxjs';

/** Subscribes to an observable and re-renders with its latest emitted value. */
export const useObservable = <T>(observable: Observable<T>, initial: T): T => {
  const [value, setValue] = useState<T>(initial);

  useEffect(() => {
    const subscription = observable.subscribe(setValue);
    return () => subscription.unsubscribe();
  }, [observable]);

  return value;
};
