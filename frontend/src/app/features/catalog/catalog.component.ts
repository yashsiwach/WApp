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
  templateUrl: './catalog.component.html',
  
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

  // Load catalog items and surface a toast if the rewards feed fails.
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

  // Build the category filter options from the items currently loaded in memory.
  get categories(): string[] {
    return [...new Set(this.items.map((i) => i.category))];
  }

  // Apply the active category filter without mutating the source item list.
  get filteredItems(): CatalogItemDto[] {
    if (!this.selectedCategory) return this.items;
    return this.items.filter((i) => i.category === this.selectedCategory);
  }

  // Map backend category names to the short labels shown in the UI.
  categoryIcon(category: string): string {
    return CATEGORY_ICONS[category] ?? 'Reward';
  }

  // Redeem one item at a time, then reload the catalog to reflect updated balances or availability.
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
