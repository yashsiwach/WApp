import { Component, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';

import { CatalogService } from './catalog.service';
import { LoaderComponent } from '../../shared/components/loader/loader.component';
import { CatalogItemDto } from '../../shared/models/catalog.model';
import { ToastService } from '../../shared/services/toast.service';

const CATEGORY_ICONS: Record<string, string> = {
  GiftCard: 'Gift',
  Cashback: 'Cash',
  Voucher: 'Voucher',
  Merchandise: 'Item',
  Other: 'Reward',
};

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [DecimalPipe, LoaderComponent],
  template: `
    <div class="mx-auto max-w-5xl space-y-6 p-6 text-slate-900">
      <div>
        <h1 class="text-2xl font-display font-bold text-slate-900">Reward Catalog</h1>
        <p class="mt-1 text-sm text-slate-500">Redeem your points for exclusive rewards.</p>
      </div>

      <app-loader [show]="loading" />

      @if (!loading) {
        @if (lastRedemptionCode) {
          <div class="rounded-xl border border-emerald-300/70 bg-emerald-50 px-4 py-3">
            <p class="text-xs uppercase tracking-wide text-emerald-700/90">Latest coupon code</p>
            <p class="mt-1 font-mono text-lg font-semibold text-emerald-700">{{ lastRedemptionCode }}</p>
          </div>
        }

        @if (items.length === 0) {
          <div class="py-20 text-center text-slate-500">
            <p class="text-lg font-medium text-slate-600">No catalog items yet</p>
            <p class="text-sm mt-1">Check back soon, rewards are on the way.</p>
          </div>
        } @else {
          @if (categories.length > 1) {
            <div class="flex gap-2 flex-wrap">
              <button
                (click)="selectedCategory = null"
                class="px-3 py-1.5 rounded-full text-xs font-medium transition-colors"
                [class.bg-accent]="selectedCategory === null"
                [class.text-white]="selectedCategory === null"
                [class.bg-slate-100]="selectedCategory !== null"
                [class.text-slate-600]="selectedCategory !== null"
                [class.hover:bg-slate-200]="selectedCategory !== null"
              >All</button>
              @for (cat of categories; track cat) {
                <button
                  (click)="selectedCategory = cat"
                  class="px-3 py-1.5 rounded-full text-xs font-medium transition-colors"
                  [class.bg-accent]="selectedCategory === cat"
                  [class.text-white]="selectedCategory === cat"
                  [class.bg-slate-100]="selectedCategory !== cat"
                  [class.text-slate-600]="selectedCategory !== cat"
                >{{ categoryIcon(cat) }} {{ cat }}</button>
              }
            </div>
          }

          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            @for (item of filteredItems; track item.id) {
              <div class="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white/95 p-5 shadow-sm transition-colors hover:border-slate-300">
                <div class="flex items-start justify-between gap-2">
                  <div class="text-sm font-semibold text-slate-500">{{ categoryIcon(item.category) }}</div>
                  <span class="shrink-0 rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-600">{{ item.category }}</span>
                </div>
                <div class="flex-1">
                  <h3 class="text-sm font-semibold text-slate-900">{{ item.name }}</h3>
                  @if (item.description) {
                    <p class="mt-1 line-clamp-2 text-xs text-slate-500">{{ item.description }}</p>
                  }
                </div>
                <div class="mt-auto flex items-center justify-between border-t border-slate-200 pt-3">
                  <div>
                    <p class="text-xs text-slate-500">Points required</p>
                    <p class="text-lg font-bold text-blue-600">{{ item.pointsCost | number }}</p>
                  </div>
                  <button
                    class="rounded-lg border border-accent/30 bg-accent/10 px-4 py-2 text-sm font-medium text-accent transition-colors hover:bg-accent/20 disabled:cursor-not-allowed disabled:opacity-50"
                    [disabled]="redeemingId === item.id"
                    (click)="redeem(item)"
                  >{{ redeemingId === item.id ? 'Redeeming...' : 'Redeem' }}</button>
                </div>
              </div>
            }
          </div>

          <p class="text-center text-xs text-slate-500">Redeem rewards instantly. Coupon codes are shown after successful redemption.</p>
        }
      }
    </div>
  `,
})
export class CatalogComponent implements OnInit {
  loading = false;
  items: CatalogItemDto[] = [];
  selectedCategory: string | null = null;
  redeemingId: string | null = null;
  lastRedemptionCode: string | null = null;

  constructor(
    private readonly catalogService: CatalogService,
    private readonly toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.loadCatalog();
  }

  loadCatalog(): void {
    this.loading = true;
    this.catalogService.getCatalog().subscribe({
      next: (items) => {
        this.items = items ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.error('Failed to load catalog.');
      },
    });
  }

  get categories(): string[] {
    return [...new Set(this.items.map((i) => i.category))];
  }

  get filteredItems(): CatalogItemDto[] {
    if (!this.selectedCategory) return this.items;
    return this.items.filter((i) => i.category === this.selectedCategory);
  }

  categoryIcon(category: string): string {
    return CATEGORY_ICONS[category] ?? 'Reward';
  }

  redeem(item: CatalogItemDto): void {
    if (this.redeemingId) return;

    this.redeemingId = item.id;
    this.catalogService.redeem(item.id).subscribe({
      next: (result) => {
        this.redeemingId = null;
        this.lastRedemptionCode = result.fulfillmentCode ?? null;
        this.toastService.success(
          this.lastRedemptionCode
            ? `Redeemed. Coupon code: ${this.lastRedemptionCode}`
            : 'Redeemed successfully.',
        );
        this.loadCatalog();
      },
      error: (err: { message?: string }) => {
        this.redeemingId = null;
        this.toastService.error(err.message ?? 'Redemption failed.');
      },
    });
  }
}
