import { NgTemplateOutlet } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideArchive,
  lucideCircleAlert,
  lucideCircleCheck,
  lucideClock,
  lucidePackage,
  lucideUser,
  lucideWrench,
} from '@ng-icons/lucide';
import { HlmAlertImports } from '@spartan-ng/helm/alert';
import { HlmBadge } from '@spartan-ng/helm/badge';
import { HlmButton } from '@spartan-ng/helm/button';
import { HlmEmptyImports } from '@spartan-ng/helm/empty';
import { HlmSkeleton } from '@spartan-ng/helm/skeleton';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { Subscription } from 'rxjs';
import { Asset, AssetState, PagedResult } from './asset.model';
import { AssetService } from './asset.service';

type LoadState = 'loading' | 'error' | 'ready';

// Each state has a label, an icon and a colour. The label and icon carry the meaning,
// so the badge never depends on colour alone. Colours are tokens from styles.css.
const STATE_BADGES: Record<AssetState, { label: string; icon: string; classes: string }> = {
  Available: { label: 'Available', icon: 'lucideCircleCheck', classes: 'border-success/40 bg-success-muted text-success' },
  Reserved: { label: 'Reserved', icon: 'lucideClock', classes: 'border-warning/40 bg-warning-muted text-warning' },
  Assigned: { label: 'Assigned', icon: 'lucideUser', classes: 'border-info/40 bg-info-muted text-info' },
  UnderRepair: { label: 'Under repair', icon: 'lucideWrench', classes: 'border-destructive/40 bg-destructive-muted text-destructive' },
  Archived: { label: 'Archived', icon: 'lucideArchive', classes: 'border-border bg-muted text-muted-foreground' },
};

@Component({
  selector: 'app-asset-list',
  imports: [
    NgIcon,
    NgTemplateOutlet,
    HlmAlertImports,
    HlmBadge,
    HlmButton,
    HlmEmptyImports,
    HlmSkeleton,
    HlmTableImports,
  ],
  providers: [
    provideIcons({
      lucideArchive,
      lucideCircleAlert,
      lucideCircleCheck,
      lucideClock,
      lucidePackage,
      lucideUser,
      lucideWrench,
    }),
  ],
  templateUrl: './asset-list.html',
})
export class AssetList {
  private readonly assetService = inject(AssetService);
  private request?: Subscription;

  protected readonly state = signal<LoadState>('loading');
  private readonly result = signal<PagedResult<Asset> | null>(null);

  protected readonly assets = computed(() => this.result()?.items ?? []);
  protected readonly totalCount = computed(() => this.result()?.totalCount ?? 0);
  protected readonly isEmpty = computed(() => this.state() === 'ready' && this.assets().length === 0);

  constructor() {
    inject(DestroyRef).onDestroy(() => this.request?.unsubscribe());
    this.load();
  }

  protected badge(state: AssetState) {
    return STATE_BADGES[state];
  }

  protected load(): void {
    this.request?.unsubscribe();
    this.state.set('loading');
    this.request = this.assetService.getAssets().subscribe({
      next: (result) => {
        this.result.set(result);
        this.state.set('ready');
      },
      error: () => this.state.set('error'),
    });
  }
}
