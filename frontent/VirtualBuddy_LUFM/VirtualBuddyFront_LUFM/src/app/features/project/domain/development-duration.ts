export function formatDevelopmentDuration(value: string, now = new Date()): string {
  const start = new Date(value);

  if (Number.isNaN(start.getTime()) || start.getTime() > now.getTime()) {
    return 'No disponible';
  }

  let months = (now.getFullYear() - start.getFullYear()) * 12 + now.getMonth() - start.getMonth();

  if (now.getDate() < start.getDate()) {
    months -= 1;
  }

  if (months < 1) return 'Menos de 1 mes';
  return months === 1 ? '1 mes' : `${months} meses`;
}
