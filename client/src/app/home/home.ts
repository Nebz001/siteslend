import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { Health } from '../health/health.model';
import { HealthService } from '../health/health.service';

type LoadState = 'loading' | 'error' | 'ready';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
})
export class Home {
  private readonly healthService = inject(HealthService);
  private request?: Subscription;

  protected readonly state = signal<LoadState>('loading');
  private readonly health = signal<Health | null>(null);

  // One row per thing we check, so the template stays a plain loop.
  protected readonly rows = computed(() => {
    const health = this.health();
    if (!health) {
      return [];
    }
    return [
      { label: 'API', ok: health.status === 'ok', okText: 'Running', failText: 'Not responding' },
      {
        label: 'Database',
        ok: health.database === 'up',
        okText: 'Connected',
        failText: 'Not connected',
      },
    ];
  });

  protected readonly databaseDown = computed(() => this.health()?.database === 'down');

  constructor() {
    inject(DestroyRef).onDestroy(() => this.request?.unsubscribe());
    this.load();
  }

  protected load(): void {
    this.request?.unsubscribe();
    this.state.set('loading');
    this.request = this.healthService.getHealth().subscribe({
      next: (health) => {
        this.health.set(health);
        this.state.set('ready');
      },
      error: () => this.state.set('error'),
    });
  }
}
