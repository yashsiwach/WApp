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
  templateUrl: './rewards.component.html',
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

  // Load the account summary that powers the balance and tier widgets.
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

  // Load the reward transaction feed shown below the account summary.
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

  // Pick the gradient background that matches the user's current tier.
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

  // Pick the badge colors that match the user's current tier.
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

  // Calculate progress within the current tier toward the next threshold.
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
