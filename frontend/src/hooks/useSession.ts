import { useObservable } from '@hooks/useObservable';
import { sessionStore } from '@services/sessionStore';

export const useSession = () => {
  const session = useObservable(sessionStore.session$, sessionStore.current);

  return {
    session,
    join: sessionStore.join,
    logout: sessionStore.logout,
    loadMe: sessionStore.loadMe,
  };
};
