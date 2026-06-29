import { useRoster } from '@hooks/useRoster';

export const PlayerRoster = () => {
  const { players } = useRoster();

  if (players.length === 0) {
    return (
      <p className="py-4 text-center text-sm text-charcoal-400 dark:text-bonewhite-300/60">
        No players yet.
      </p>
    );
  }

  return (
    <div className="grid gap-2 sm:grid-cols-2">
      {players.map((player) => (
        <div
          key={player.name}
          className="rounded-lg border border-bonewhite-200 bg-white p-3 dark:border-charcoal-700 dark:bg-charcoal-900/60"
        >
          <div className="font-semibold text-dodred-600 dark:text-dodred-400">{player.name}</div>
          <div className="mt-1.5 grid grid-cols-3 gap-1 text-xs text-charcoal-500 dark:text-bonewhite-300/60">
            <div className="flex flex-col items-center rounded bg-bonewhite-100 py-1 dark:bg-charcoal-800">
              <span className="font-bold text-charcoal-800 dark:text-bonewhite-200">{player.kp}</span>
              <span>KP</span>
            </div>
            <div className="flex flex-col items-center rounded bg-bonewhite-100 py-1 dark:bg-charcoal-800">
              <span className="font-bold text-charcoal-800 dark:text-bonewhite-200">{player.upptackFara}</span>
              <span>Uppt.</span>
            </div>
            <div className="flex flex-col items-center rounded bg-bonewhite-100 py-1 dark:bg-charcoal-800">
              <span className="font-bold text-charcoal-800 dark:text-bonewhite-200">{player.finnaDoldaTing}</span>
              <span>Finna</span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};
