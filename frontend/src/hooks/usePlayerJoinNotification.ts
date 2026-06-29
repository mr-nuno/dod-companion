import { useEffect, useState } from 'react';
import { signalrService } from '@services/signalrService';

export const usePlayerJoinNotification = (): string | null => {
  const [notification, setNotification] = useState<string | null>(null);

  useEffect(() => {
    const sub = signalrService.playerJoined$.subscribe((player) => {
      setNotification(`${player.name} joined the session`);
      setTimeout(() => setNotification(null), 4000);
    });
    return () => sub.unsubscribe();
  }, []);

  return notification;
};
