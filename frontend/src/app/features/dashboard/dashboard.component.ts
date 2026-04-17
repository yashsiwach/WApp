import { Component, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { WalletService } from '../wallet/wallet.service';
import { RewardsService } from '../rewards/rewards.service';
import { NotificationsService } from '../notifications/notifications.service';
import { SupportService } from '../support/support.service';
import { AuthService } from '../auth/auth.service';
import { TokenService } from '../../core/services/token.service';
import { LoaderComponent } from '../../shared/components/loader/loader.component';
import { InrCurrencyPipe } from '../../shared/pipes/inr-currency.pipe';

import { WalletResponse, TransactionResponse } from '../../shared/models/wallet.model';
import { RewardResponse } from '../../shared/models/rewards.model';
import { NotificationDto } from '../../shared/models/notification.model';
import { SupportTicketDto } from '../../shared/models/support.model';
import { KycInfo } from '../../shared/models/auth.model';

const TIER_CONFIG: Record<string, { next: string; min: number; max: number; color: string; bg: string }> = {
  Bronze:   { next: 'Silver',   min: 0,     max: 1000,  color: 'text-blue-700',   bg: 'from-blue-50 to-blue-100/80' },
  Silver:   { next: 'Gold',     min: 1000,  max: 5000,  color: 'text-slate-600',  bg: 'from-slate-50 to-slate-100' },
  Gold:     { next: 'Platinum', min: 5000,  max: 15000, color: 'text-cyan-700',   bg: 'from-cyan-50 to-cyan-100/80' },
  Platinum: { next: 'Max',      min: 15000, max: 15000, color: 'text-cyan-700',   bg: 'from-cyan-50 to-sky-100/70' },
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DatePipe, DecimalPipe, NgClass, RouterLink, LoaderComponent, InrCurrencyPipe],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  loading = true;

  wallet: WalletResponse | null = null;
  rewards: RewardResponse | null = null;
  recentTx: TransactionResponse[] = [];
  recentNotifs: NotificationDto[] = [];
  recentTickets: SupportTicketDto[] = [];
  kycInfo: KycInfo | null = null;

  today = new Date();

  constructor(
    private readonly walletService: WalletService,
    private readonly rewardsService: RewardsService,
    private readonly notificationsService: NotificationsService,
    private readonly supportService: SupportService,
    private readonly authService: AuthService,
    private readonly tokenService: TokenService,
  ) {}

  // Choose a greeting based on the local time shown on the dashboard.
  get greeting(): string {
    const h = new Date().getHours();
    if (h < 12) return 'Good morning';
    if (h < 17) return 'Good afternoon';
    return 'Good evening';
  }

  // Derive a friendly display name from the stored email address.
  get firstName(): string {
    const email = this.tokenService.getUser()?.email ?? '';
    return email.split('@')[0];
  }

  // Surface the current unread notification count.
  get unreadCount(): number {
    return this.recentNotifs.length;
  }

  // Count tickets that still need user attention.
  get openTickets(): number {
    return this.recentTickets.filter((t) => t.status === 'Open' || t.status === 'InProgress').length;
  }

  // Resolve the gradient background for the current rewards tier card.
  get tierBg(): string {
    return TIER_CONFIG[this.rewards?.tier ?? 'Bronze']?.bg ?? TIER_CONFIG['Bronze'].bg;
  }

  // Resolve the text color accent for the current rewards tier.
  get tierTextColor(): string {
    return TIER_CONFIG[this.rewards?.tier ?? 'Bronze']?.color ?? '';
  }

  // Show the next tier label in the progress widget.
  get nextTier(): string {
    return TIER_CONFIG[this.rewards?.tier ?? 'Bronze']?.next ?? '';
  }

  // Convert the user's total earned points into a 0-100 progress value for the next tier.
  get tierProgress(): number {
    const tier = this.rewards?.tier ?? 'Bronze';
    const cfg = TIER_CONFIG[tier];
    if (!cfg || tier === 'Platinum') return 100;
    const pts = this.rewards?.totalEarned ?? 0;
    // Clamp the value so template bindings never render negative widths or values above 100%.
    return Math.min(100, Math.max(0, ((pts - cfg.min) / (cfg.max - cfg.min)) * 100));
  }

  // Show how many points remain before the next tier is reached.
  get ptsToNextTier(): number {
    const tier = this.rewards?.tier ?? 'Bronze';
    const cfg = TIER_CONFIG[tier];
    if (!cfg || tier === 'Platinum') return 0;
    return Math.max(0, cfg.max - (this.rewards?.totalEarned ?? 0));
  }

  // Resolve the fill color used by the tier progress bar.
  get tierBarColor(): string {
    const map: Record<string, string> = {
      Bronze: 'bg-blue-500',
      Silver: 'bg-slate-500',
      Gold: 'bg-cyan-500',
      Platinum: 'bg-cyan-400',
    };
    return map[this.rewards?.tier ?? 'Bronze'] ?? 'bg-accent';
  }

  // Map tier names to the badge classes used throughout the dashboard.
  tierBadgeClass(tier?: string): string {
    const map: Record<string, string> = {
      Bronze:   'bg-blue-500/20 text-blue-700',
      Silver:   'bg-slate-300/60 text-slate-700',
      Gold:     'bg-cyan-500/20 text-cyan-700',
      Platinum: 'bg-cyan-500/20 text-cyan-700',
    };
    return map[tier ?? 'Bronze'] ?? 'bg-slate-200 text-slate-700';
  }

  // Load the dashboard cards in parallel and gracefully fall back when one source fails.
  ngOnInit(): void {
    forkJoin({
      // Each source falls back independently so one failing card does not blank the whole dashboard.
      wallet:        this.walletService.getWallet().pipe(catchError(() => of(null))),
      transactions:  this.walletService.getTransactions(1, 5).pipe(catchError(() => of(null))),
      rewards:       this.rewardsService.getRewards().pipe(catchError(() => of(null))),
      notifications: this.notificationsService.getNotifications(1, 5).pipe(catchError(() => of(null))),
      tickets:       this.supportService.getTickets().pipe(catchError(() => of([] as SupportTicketDto[]))),
      kyc:           this.authService.getKycStatus().pipe(catchError(() => of([] as KycInfo[]))),
    }).subscribe(({ wallet, transactions, rewards, notifications, tickets, kyc }) => {
      const latestKyc =
        kyc?.length
          // Show only the most recent submission in the status card.
          ? [...kyc].sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime())[0]
          : null;

      this.wallet       = wallet;
      this.recentTx     = transactions?.items?.slice(0, 5) ?? [];
      this.rewards      = rewards;
      this.recentNotifs = notifications?.items?.slice(0, 6) ?? [];
      this.recentTickets = (tickets ?? []).slice(0, 3);
      this.kycInfo      =
        // Mirror the wallet verification flag when the latest submission has not yet been reflected in KYC history.
        latestKyc && latestKyc.status === 'Pending' && wallet?.kycVerified
          ? { ...latestKyc, status: 'Approved' }
          : latestKyc;
      this.loading      = false;
    });
  }
}
