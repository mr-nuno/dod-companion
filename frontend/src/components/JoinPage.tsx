import { useState } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { useSession } from '@hooks/useSession';
import { ThemeToggle } from '@components/ThemeToggle';
import type { CreatedRoom } from '@/types';

const inputClass =
  'w-full rounded-xl border-2 border-bonewhite-200 bg-bonewhite-50/50 px-4 py-3 text-lg text-charcoal-900 outline-none transition-colors focus:border-dodred-500 dark:border-charcoal-700 dark:bg-charcoal-950/50 dark:text-bonewhite-500 dark:focus:border-dodred-500';
const labelClass =
  'text-sm font-semibold uppercase tracking-wider text-charcoal-700 dark:text-bonewhite-400';
const primaryButtonClass =
  'w-full rounded-xl bg-gradient-to-r from-dodred-500 to-dodred-600 px-4 py-3 text-lg font-bold tracking-wide text-white shadow-lg shadow-dodred-500/30 transition-all hover:scale-[1.02] hover:shadow-dodred-500/50 disabled:pointer-events-none disabled:opacity-50';

export const JoinPage = () => {
  const { create, join } = useSession();

  // Read the join token from the URL here (not at module level) so the value is always
  // in sync with the current URL, even if it changed since the module was first imported.
  const joinToken = new URLSearchParams(window.location.search).get('join') ?? '';
  const isJoining = joinToken.length > 0;

  // Create step 1 inputs.
  const [roomName, setRoomName] = useState('');
  const [hostKey, setHostKey] = useState('');
  // Set once the room is provisioned — drives the move to step 2 (QR + player name).
  const [createdRoom, setCreatedRoom] = useState<CreatedRoom | null>(null);

  const [playerName, setPlayerName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  // The token used to actually join: from the URL (scanned QR) or the room we just created.
  const enterToken = isJoining ? joinToken : createdRoom?.joinToken;
  const showQrStep = !isJoining && createdRoom !== null;

  const onCreate = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      setCreatedRoom(await create(roomName, hostKey));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not create the room.');
    } finally {
      setSubmitting(false);
    }
  };

  const onEnter = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!enterToken) return;
    setError(null);
    setSubmitting(true);
    try {
      await join(enterToken, playerName);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not join.');
    } finally {
      setSubmitting(false);
    }
  };

  const heading = isJoining ? 'Join' : showQrStep ? 'Share & enter' : 'Create a room';

  return (
    <div className="relative flex min-h-full items-center justify-center bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-bonewhite-200 via-bonewhite-100 to-bonewhite-50 p-6 dark:from-charcoal-800 dark:via-charcoal-900 dark:to-charcoal-950">
      <div className="absolute right-4 top-4">
        <ThemeToggle />
      </div>

      <form
        onSubmit={isJoining || showQrStep ? onEnter : onCreate}
        className="w-full max-w-md space-y-6 rounded-3xl border border-bonewhite-200 bg-white/80 p-6 sm:p-10 shadow-2xl backdrop-blur-sm transition-all dark:border-charcoal-700/50 dark:bg-charcoal-900/60 dark:shadow-[0_0_40px_-10px_rgba(227,28,35,0.3)]"
      >
        <div className="space-y-4 text-center">
          <img
            src="/dod.png"
            alt="Drakar och Demoner"
            className="mx-auto h-auto w-full max-w-[20rem] drop-shadow-md transition-transform hover:scale-105"
          />
          <p className="text-lg font-medium text-charcoal-500 dark:text-bonewhite-300">{heading}</p>
        </div>

        {/* Create — step 1: host key + room name */}
        {!isJoining && !showQrStep && (
          <>
            <label className="block space-y-2">
              <span className={labelClass}>Room name</span>
              <input
                value={roomName}
                onChange={(e) => setRoomName(e.target.value)}
                placeholder="DRAGON"
                autoCapitalize="characters"
                className={`${inputClass} uppercase tracking-widest`}
                required
              />
            </label>

            <label className="block space-y-2">
              <span className={labelClass}>Host key</span>
              <input
                type="password"
                value={hostKey}
                onChange={(e) => setHostKey(e.target.value)}
                placeholder="••••••"
                className={inputClass}
                required
              />
            </label>
          </>
        )}

        {/* Create — step 2: big QR for players to scan */}
        {showQrStep && createdRoom && (
          <div className="flex flex-col items-center justify-center space-y-3">
            <span className="text-sm font-bold uppercase tracking-wider text-charcoal-700 dark:text-bonewhite-400">
              Room:{' '}
              <span className="text-lg uppercase text-runecyan-600 dark:text-runecyan-500">
                {createdRoom.roomCode}
              </span>
            </span>
            <div className="overflow-hidden rounded-2xl bg-white p-3 sm:p-5 shadow-md flex justify-center w-full max-w-[280px]">
              <QRCodeSVG
                value={`${window.location.origin}/?join=${createdRoom.joinToken}`}
                size={240}
                className="w-full h-auto max-w-[240px]"
                bgColor="#ffffff"
                fgColor="#1c1c1c"
              />
            </div>
            <p className="text-xs font-medium text-charcoal-500 dark:text-bonewhite-300">
              Players scan this to join
            </p>
          </div>
        )}

        {/* Player name — required to actually enter (join-from-QR and create step 2) */}
        {(isJoining || showQrStep) && (
          <label className="block space-y-2">
            <span className={labelClass}>Player name</span>
            <input
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              placeholder="Aragorn"
              className={inputClass}
              required
            />
          </label>
        )}

        {error && (
          <div className="rounded-lg bg-dodred-50 p-3 text-sm font-medium text-dodred-700 dark:bg-dodred-900/30 dark:text-dodred-300">
            {error}
          </div>
        )}

        <button type="submit" disabled={submitting} className={primaryButtonClass}>
          {!isJoining && !showQrStep && (submitting ? 'Creating…' : 'Create room')}
          {(isJoining || showQrStep) && (submitting ? 'Joining…' : 'Join')}
        </button>

        {!isJoining && !showQrStep && (
          <p className="text-center text-xs font-medium text-charcoal-500 dark:text-bonewhite-300">
            Joining a room? Scan its QR code to get in.
          </p>
        )}
      </form>
    </div>
  );
};
