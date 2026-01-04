/**
 * Password Strength Indicator Component
 * Shows visual feedback on password strength
 */

import { useMemo } from 'react';
import { cn } from '@/lib/utils';

interface PasswordStrengthIndicatorProps {
  password: string;
}

interface StrengthLevel {
  label: string;
  color: string;
  bgColor: string;
  score: number;
}

const STRENGTH_LEVELS: StrengthLevel[] = [
  { label: 'Muy débil', color: 'text-red-600', bgColor: 'bg-red-500', score: 0 },
  { label: 'Débil', color: 'text-orange-500', bgColor: 'bg-orange-500', score: 1 },
  { label: 'Regular', color: 'text-yellow-500', bgColor: 'bg-yellow-500', score: 2 },
  { label: 'Fuerte', color: 'text-green-500', bgColor: 'bg-green-500', score: 3 },
  { label: 'Muy fuerte', color: 'text-green-600', bgColor: 'bg-green-600', score: 4 },
];

function calculateStrength(password: string): number {
  if (!password) return 0;

  let score = 0;

  // Length checks
  if (password.length >= 8) score++;
  if (password.length >= 12) score++;

  // Character type checks
  if (/[a-z]/.test(password)) score += 0.5;
  if (/[A-Z]/.test(password)) score += 0.5;
  if (/[0-9]/.test(password)) score += 0.5;
  if (/[^a-zA-Z0-9]/.test(password)) score += 0.5;

  return Math.min(4, Math.floor(score));
}

export function PasswordStrengthIndicator({
  password,
}: PasswordStrengthIndicatorProps) {
  const strength = useMemo(() => calculateStrength(password), [password]);
  const level = STRENGTH_LEVELS[strength];

  if (!password) return null;

  return (
    <div className="space-y-1.5">
      {/* Strength bars */}
      <div className="flex gap-1">
        {Array.from({ length: 4 }).map((_, index) => (
          <div
            key={index}
            className={cn(
              'h-1.5 flex-1 rounded-full transition-colors',
              index < strength + 1 ? level.bgColor : 'bg-muted'
            )}
          />
        ))}
      </div>

      {/* Strength label */}
      <p className={cn('text-xs', level.color)}>
        Seguridad: {level.label}
      </p>

      {/* Tips for weak passwords */}
      {strength < 2 && (
        <ul className="text-xs text-muted-foreground space-y-0.5 mt-1">
          {password.length < 8 && (
            <li>• Usa al menos 8 caracteres</li>
          )}
          {!/[A-Z]/.test(password) && (
            <li>• Incluye una letra mayúscula</li>
          )}
          {!/[0-9]/.test(password) && (
            <li>• Incluye un número</li>
          )}
          {!/[^a-zA-Z0-9]/.test(password) && (
            <li>• Incluye un carácter especial</li>
          )}
        </ul>
      )}
    </div>
  );
}
