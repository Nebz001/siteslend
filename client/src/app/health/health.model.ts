/** Shape returned by GET /api/health. */
export interface Health {
  status: string;
  database: 'up' | 'down';
}
