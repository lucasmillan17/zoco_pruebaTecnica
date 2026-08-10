const TONOS_FEEDBACK = {
  neutral: 'text-muted',
  ok: 'text-success',
  error: 'text-danger',
};

export default function FormField({ label, required, feedback, feedbackTono = 'neutral', className = '', children }) {
  return (
    <label className={`block text-sm font-medium text-gray-700 ${className}`}>
      {label}
      {required && <span className="ml-0.5 text-red-500">*</span>}
      <div className="mt-1.5">{children}</div>
      {feedback && (
        <span className={`mt-1 block text-xs font-normal ${TONOS_FEEDBACK[feedbackTono] ?? TONOS_FEEDBACK.neutral}`}>
          {feedback}
        </span>
      )}
    </label>
  );
}
