interface LoadingProps {
  label?: string;
  /** Vertical padding; lighter for inline sections, roomier for full-page states. */
  className?: string;
}

export function Loading({ label = "Loading…", className = "py-20" }: LoadingProps) {
  return (
    <div
      role="status"
      aria-live="polite"
      className={`flex flex-col items-center justify-center gap-3 text-gray-500 ${className}`}
    >
      <span className="h-9 w-9 animate-spin rounded-full border-[3px] border-gray-200 border-t-blue-600" />
      <span className="text-sm font-medium">{label}</span>
    </div>
  );
}
