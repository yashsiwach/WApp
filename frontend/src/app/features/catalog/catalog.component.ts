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
    <div class="p-6 space-y-6 max-w-5xl mx-auto">
      <div>
        <h1 class="text-2xl font-display font-bold text-zinc-100">Reward Catalog</h1>
        <p class="text-zinc-400 text-sm mt-1">Redeem your points for exclusive rewards.</p>
      </div>

      <app-loader [show]="loading" />

      @if (!loading) {
        @if (lastRedemptionCode) {
          <div class="rounded-xl border border-emerald-500/30 bg-emerald-500/10 px-4 py-3">
            <p class="text-xs uppercase tracking-wide text-emerald-300/90">Latest coupon code</p>
            <p class="mt-1 font-mono text-lg font-semibold text-emerald-200">{{ lastRedemptionCode }}</p>
          </div>
        }

        @if (items.length === 0) {
          <div class="text-center py-20 text-zinc-500">
            <p class="font-medium text-zinc-400 text-lg">No catalog items yet</p>
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
                [class.bg-zinc-800]="selectedCategory !== null"
                [class.text-zinc-400]="selectedCategory !== null"
                [class.hover:bg-zinc-700]="selectedCategory !== null"
              >All</button>
              @for (cat of categories; track cat) {
                <button
                  (click)="selectedCategory = cat"
                  class="px-3 py-1.5 rounded-full text-xs font-medium transition-colors"
                  [class.bg-accent]="selectedCategory === cat"
                  [class.text-white]="selectedCategory === cat"
                  [class.bg-zinc-800]="selectedCategory !== cat"
                  [class.text-zinc-400]="selectedCategory !== cat"
                >{{ categoryIcon(cat) }} {{ cat }}</button>
              }
            </div>
          }

          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            @for (item of filteredItems; track item.id) {
              <div class="bg-zinc-900/80 border border-zinc-800 rounded-xl p-5 flex flex-col gap-3 hover:border-zinc-700 transition-colors">
                <div class="flex items-start justify-between gap-2">
                  <div class="text-sm text-zinc-400 font-semibold">{{ categoryIcon(item.category) }}</div>
                  <span class="px-2 py-0.5 bg-zinc-700 text-zinc-300 rounded-full text-xs shrink-0">{{ item.category }}</span>
                </div>
                <div class="flex-1">
                  <h3 class="font-semibold text-zinc-100 text-sm">{{ item.name }}</h3>
                  @if (item.description) {
                    <p class="text-zinc-500 text-xs mt-1 line-clamp-2">{{ item.description }}</p>
                  }
                </div>
                <div class="flex items-center justify-between mt-auto pt-3 border-t border-zinc-800">
                  <div>
                    <p class="text-xs text-zinc-500">Points required</p>
                    <p class="text-amber-300 font-bold text-lg">{{ item.pointsCost | number }}</p>
                  </div>
                  <button
                    class="px-4 py-2 bg-accent/20 hover:bg-accent/30 text-accent rounded-lg text-sm font-medium transition-colors border border-accent/30 disabled:opacity-50 disabled:cursor-not-allowed"
                    [disabled]="redeemingId === item.id"
                    (click)="redeem(item)"
                  >{{ redeemingId === item.id ? 'Redeeming...' : 'Redeem' }}</button>
                </div>
              </div>
            }
          </div>

          <p class="text-zinc-600 text-xs text-center">Redeem rewards instantly. Coupon codes are shown after successful redemption.</p>
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
