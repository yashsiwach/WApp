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
    <div class="p-6 space-y-6 max-w-5xl mx-auto">
      <h1 class="text-2xl font-display font-bold text-zinc-100">Rewards</h1>

      <div class="rounded-xl p-6 text-white shadow-lg" [class]="tierBg">
        <app-loader [show]="loadingAccount" />
        @if (!loadingAccount && rewards) {
          <div class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-6">
            <div>
              <p class="text-sm opacity-80 mb-1">Points Balance</p>
              <p class="text-4xl font-bold">{{ rewards.pointsBalance | number }}</p>
              <p class="text-sm opacity-70 mt-1">Lifetime Earned: {{ rewards.totalEarned | number }} pts</p>
            </div>
            <div class="flex flex-col items-start sm:items-end gap-3">
              <span class="px-4 py-1.5 rounded-full text-sm font-bold" [class]="tierBadgeClass">
                {{ rewards.tier }} Tier
              </span>
              <p class="text-sm opacity-80 font-semibold">Keep earning points to move up tiers.</p>
            </div>
          </div>

          <div class="mt-5">
            <div class="flex justify-between text-xs opacity-70 mb-1">
              <span>{{ rewards.tier }}</span>
              <span>{{ rewards.pointsBalance | number }} pts</span>
            </div>
            <div class="w-full bg-black/20 rounded-full h-2.5">
              <div
                class="bg-white/80 h-2.5 rounded-full transition-all duration-500"
                [style.width.%]="tierProgress"
              ></div>
            </div>
          </div>
        }
      </div>

      <div class="bg-zinc-900/80 border border-zinc-800 rounded-xl shadow p-6">
        <h2 class="text-lg font-semibold text-zinc-100 mb-4">Points History</h2>
        <app-loader [show]="loadingHistory" />
        @if (!loadingHistory) {
          @if (history.length === 0) {
            <div class="text-center py-12 text-zinc-500">
              <div class="text-4xl mb-2">Points</div>
              <p>No points earned yet. Make a transfer to earn points.</p>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-zinc-700 text-zinc-400 text-left">
                    <th class="pb-3 font-medium">Date</th>
                    <th class="pb-3 font-medium">Points</th>
                    <th class="pb-3 font-medium">Reason</th>
                    <th class="pb-3 font-medium">Reference</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-zinc-800">
                  @for (tx of history; track tx.id) {
                    <tr class="hover:bg-zinc-800/50">
                      <td class="py-3 text-zinc-400 text-xs">{{ tx.createdAt | date:'dd MMM, HH:mm' }}</td>
                      <td
                        class="py-3 font-semibold"
                        [class.text-emerald-400]="tx.points >= 0"
                        [class.text-rose-400]="tx.points < 0"
                      >
                        {{ tx.points > 0 ? '+' : '' }}{{ tx.points | number }}
                      </td>
                      <td class="py-3 text-zinc-300">{{ tx.reason }}</td>
                      <td class="py-3 text-zinc-500 text-xs font-mono">{{ tx.reference }}</td>
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
      bronze: 'bg-gradient-to-br from-amber-700 to-amber-500',
      silver: 'bg-gradient-to-br from-slate-500 to-slate-400',
      gold: 'bg-gradient-to-br from-yellow-600 to-yellow-400',
      platinum: 'bg-gradient-to-br from-slate-400 to-slate-300',
    };
    return map[tier] ?? 'bg-gradient-to-br from-zinc-800 to-zinc-700';
  }

  get tierBadgeClass(): string {
    const tier = this.rewards?.tier?.toLowerCase() ?? '';
    const map: Record<string, string> = {
      bronze: 'bg-amber-900/30 text-amber-100',
      silver: 'bg-slate-700/30 text-slate-100',
      gold: 'bg-yellow-800/30 text-yellow-100',
      platinum: 'bg-slate-600/30 text-slate-100',
    };
    return map[tier] ?? 'bg-zinc-700 text-zinc-100';
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
