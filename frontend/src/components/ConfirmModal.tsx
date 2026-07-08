import { useEffect } from 'react';
import { createPortal } from 'react-dom';

export interface ConfirmModalProps {
  isOpen: boolean;
  title: string;
  description: string;
  confirmLabel?: string;
  cancelLabel?: string;
  onConfirm: () => void;
  onClose: () => void;
  children?: React.ReactNode;
}

export const ConfirmModal = ({
  isOpen,
  title,
  description,
  confirmLabel = 'Delete',
  cancelLabel = 'Cancel',
  onConfirm,
  onClose,
  children,
}: ConfirmModalProps) => {
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onClose();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const modalContent = (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-charcoal-950/70 p-4 backdrop-blur-md transition-opacity"
      onClick={onClose}
    >
      <div
        className="relative w-full max-w-md overflow-hidden rounded-2xl border border-bonewhite-200/80 bg-white/95 p-6 shadow-2xl backdrop-blur-xl transition-all dark:border-charcoal-700 dark:bg-charcoal-900/95"
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-modal-title"
      >
        {/* Subtle decorative top glow */}
        <div className="absolute -top-12 left-1/2 h-24 w-48 -translate-x-1/2 rounded-full bg-dodred-500/20 blur-2xl pointer-events-none" />

        <div className="relative flex flex-col items-center text-center">
          {/* Icon Badge */}
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-dodred-500/15 text-dodred-600 ring-8 ring-dodred-500/10 dark:bg-dodred-500/20 dark:text-dodred-400 dark:ring-dodred-500/15">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              className="h-7 w-7"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
              />
            </svg>
          </div>

          {/* Title and Description */}
          <h3
            id="confirm-modal-title"
            className="mt-4 text-xl font-bold tracking-tight text-charcoal-900 dark:text-bonewhite-100"
          >
            {title}
          </h3>
          <p className="mt-2 text-sm leading-relaxed text-charcoal-600 dark:text-bonewhite-300">
            {description}
          </p>

          {/* Optional Preview or Custom Content */}
          {children && (
            <div className="mt-4 w-full rounded-xl border border-bonewhite-300 border-l-4 border-l-dodred-500 bg-bonewhite-100/80 p-3.5 text-left text-sm text-charcoal-900 shadow-inner dark:border-charcoal-700 dark:border-l-dodred-500 dark:bg-charcoal-950 dark:text-bonewhite-100">
              {children}
            </div>
          )}

          {/* Actions */}
          <div className="mt-6 flex w-full flex-col-reverse gap-3 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              className="w-full rounded-xl border border-bonewhite-300 bg-white px-5 py-2.5 text-sm font-semibold text-charcoal-700 shadow-sm transition hover:bg-bonewhite-100 hover:text-charcoal-900 focus:outline-none dark:border-charcoal-700 dark:bg-charcoal-800 dark:text-bonewhite-200 dark:hover:bg-charcoal-700 sm:w-auto"
            >
              {cancelLabel}
            </button>
            <button
              type="button"
              onClick={onConfirm}
              className="w-full rounded-xl bg-gradient-to-r from-dodred-600 to-dodred-500 px-5 py-2.5 text-sm font-semibold text-white shadow-md shadow-dodred-600/30 transition-all hover:from-dodred-500 hover:to-dodred-400 hover:shadow-lg hover:shadow-dodred-600/40 focus:outline-none active:scale-95 sm:w-auto"
            >
              {confirmLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  );

  return createPortal(modalContent, document.body);
};
