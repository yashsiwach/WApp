import { Component, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';

import { RewardsService } from './rewards.service';
import { LoaderComponent } from '../../shared/components/loader/loader.component';
import { RewardResponse, RewardTransactionResponse } from '../../shared/models/rewards.model';

const TIER_THRESHOLDS: Record<string, number> = {
  Bronze: 0,
  Silver: 1000,
  Gold: 5000,
  Platinum: 15000,
};

@Component({
  selector: 'app-rewards',
  standalone: true,
  imports: [DatePipe, DecimalPipe, LoaderComponent],
  template: `
    <div class="mx-auto max-w-5xl space-y-6 p-6 text-slate-900">
      <h1 class="text-2xl font-display font-bold text-slate-900">Rewards</h1>

      <div class="rounded-xl border border-slate-200 p-6 text-slate-900 shadow-sm" [class]="tierBg">
        <app-loader [show]="loadingAccount" />
        @if (!loadingAccount && rewards) {
          <div class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-6">
            <div>
              <p class="mb-1 text-sm text-slate-500">Points Balance</p>
              <p class="text-4xl font-bold">{{ rewards.pointsBalance | number }}</p>
              <p class="mt-1 text-sm text-slate-500">Lifetime Earned: {{ rewards.totalEarned | number }} pts</p>
            </div>
            <div class="flex flex-col items-start sm:items-end gap-3">
              <span class="rounded-full px-4 py-1.5 text-sm font-bold" [class]="tierBadgeClass">
                {{ rewards.tier }} Tier
              </span>
              <p class="text-sm font-semibold text-slate-600">Keep earning points to move up tiers.</p>
            </div>
          </div>

          <div class="mt-5">
            <div class="mb-1 flex justify-between text-xs text-slate-500">
              <span>{{ rewards.tier }}</span>
              <span>{{ rewards.pointsBalance | number }} pts</span>
            </div>
            <div class="h-2.5 w-full rounded-full bg-slate-200">
              <div
                class="h-2.5 rounded-full bg-accent transition-all duration-500"
                [style.width.%]="tierProgress"
              ></div>
            </div>
          </div>
        }
      </div>

      <div class="rounded-xl border border-slate-200 bg-white/95 p-6 shadow-sm">
        <h2 class="mb-4 text-lg font-semibold text-slate-900">Points History</h2>
        <app-loader [show]="loadingHistory" />
        @if (!loadingHistory) {
          @if (history.length === 0) {
            <div class="py-12 text-center text-slate-500">
              <div class="text-4xl mb-2">Points</div>
              <p>No points earned yet. Make a transfer to earn points.</p>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-slate-200 text-left text-slate-500">
                    <th class="pb-3 font-medium">Date</th>
                    <th class="pb-3 font-medium">Points</th>
                    <th class="pb-3 font-medium">Reason</th>
                    <th class="pb-3 font-medium">Reference</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100">
                  @for (tx of history; track tx.id) {
                    <tr class="hover:bg-slate-100/80">
                      <td class="py-3 text-xs text-slate-500">{{ tx.createdAt | date:'dd MMM, HH:mm' }}</td>
                      <td
                        class="py-3 font-semibold"
                        [class.text-emerald-400]="tx.points >= 0"
                        [class.text-rose-400]="tx.points < 0"
                      >
                        {{ tx.points > 0 ? '+' : '' }}{{ tx.points | number }}
                      </td>
                      <td class="py-3 text-slate-700">{{ tx.reason }}</td>
                      <td class="py-3 text-xs font-mono text-slate-500">{{ tx.reference }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
      </div>
    </div>
  `,
})
export class RewardsComponent implements OnInit {
  loadingAccount = false;
  loadingHistory = false;
  rewards: RewardResponse | null = null;
  history: RewardTransactionResponse[] = [];

  constructor(private readonly rewardsService: RewardsService) {}

  ngOnInit(): void {
    this.loadAccount();
    this.loadHistory();
  }

  loadAccount(): void {
    this.loadingAccount = true;
    this.rewardsService.getRewards().subscribe({
      next: (r) => {
        this.rewards = r;
        this.loadingAccount = false;
      },
      error: () => (this.loadingAccount = false),
    });
  }

  loadHistory(): void {
    this.loadingHistory = true;
    this.rewardsService.getHistory().subscribe({
      next: (h) => {
        this.history = h;
        this.loadingHistory = false;
      },
      error: () => (this.loadingHistory = false),
    });
  }

  get tierBg(): string {
    const tier = this.rewards?.tier?.toLowerCase() ?? '';
    const map: Record<string, string> = {
      bronze: 'bg-gradient-to-br from-blue-50 to-blue-100/80',
      silver: 'bg-gradient-to-br from-slate-50 to-slate-100',
      gold: 'bg-gradient-to-br from-cyan-50 to-cyan-100/80',
      platinum: 'bg-gradient-to-br from-cyan-50 to-sky-100/70',
    };
    return map[tier] ?? 'bg-gradient-to-br from-slate-50 to-slate-100';
  }

  get tierBadgeClass(): string {
    const tier = this.rewards?.tier?.toLowerCase() ?? '';
    const map: Record<string, string> = {
      bronze: 'bg-blue-200/70 text-blue-800',
      silver: 'bg-slate-200/70 text-slate-700',
      gold: 'bg-cyan-200/80 text-cyan-800',
      platinum: 'bg-cyan-200/80 text-cyan-800',
    };
    return map[tier] ?? 'bg-slate-200 text-slate-700';
  }

  get tierProgress(): number {
    const pointsBalance = this.rewards?.pointsBalance ?? 0;
    const currentTierMin = TIER_THRESHOLDS[this.rewards?.tier ?? ''] ?? 0;
    const nextTierMin =
      Object.values(TIER_THRESHOLDS).find((threshold) => threshold > pointsBalance) ??
      Math.max(currentTierMin, pointsBalance);
    const range = nextTierMin - currentTierMin;
    if (range <= 0) return 100;
    const progress = pointsBalance - currentTierMin;
    return Math.min(100, Math.max(0, (progress / range) * 100));
  }
}
