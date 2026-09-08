import { formatDevelopmentDuration } from './development-duration';

describe('formatDevelopmentDuration', () => {
  const now = new Date(2026, 8, 7, 12);

  it.each([
    ['2026-08-08T10:00:00', 'Menos de 1 mes'],
    ['2026-08-07T10:00:00', '1 mes'],
    ['2025-03-07T10:00:00', '18 meses'],
  ])('formats %s as %s', (start, expected) => {
    expect(formatDevelopmentDuration(start, now)).toBe(expected);
  });

  it.each(['not-a-date', '2026-09-08T10:00:00'])('rejects unavailable date %s', (start) => {
    expect(formatDevelopmentDuration(start, now)).toBe('No disponible');
  });
});
