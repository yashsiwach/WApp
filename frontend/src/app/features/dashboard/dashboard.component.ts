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
  template: `
    <div class="mx-auto max-w-7xl space-y-6 p-6 text-slate-900">

      <!-- ── Greeting ── -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-1">
        <div>
          <h1 class="text-2xl font-bold text-slate-900">{{ greeting }}, {{ firstName }}</h1>
          <p class="mt-0.5 text-sm text-slate-500">{{ today | date:'EEEE, MMMM d, y' }}</p>
        </div>
        <div class="flex gap-2 flex-wrap">
          <a routerLink="/dashboard/wallet"
             class="px-4 py-2 bg-accent hover:bg-accent-hover text-white text-sm font-semibold rounded-lg transition-colors">
            Top Up
          </a>
          <a routerLink="/dashboard/wallet"
             class="px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 text-sm font-semibold rounded-lg border border-slate-200 transition-colors">
            Transfer
          </a>
        </div>
      </div>

      <!-- ── Stat Cards ── -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">

        <!-- Balance -->
        <div class="rounded-xl border border-accent/20 bg-gradient-to-br from-white via-white to-teal-50/70 p-5 shadow-sm space-y-3">
          <div class="flex items-center justify-between">
            <p class="text-xs font-medium text-slate-500 uppercase tracking-wider">Wallet Balance</p>
            <span class="text-xl">&#128179;</span>
          </div>
          <app-loader [show]="loading" />
          @if (!loading) {
            <p class="text-2xl font-bold text-slate-900">{{ wallet?.balance | inrCurrency }}</p>
            <div class="flex items-center gap-2">
              @if (wallet?.isLocked) {
                <span class="px-2 py-0.5 rounded-full text-xs bg-rose-500/20 text-rose-700">Locked</span>
              } @else {
                <span class="px-2 py-0.5 rounded-full text-xs bg-emerald-500/20 text-emerald-700">Active</span>
              }
              @if (wallet?.kycVerified) {
                <span class="px-2 py-0.5 rounded-full text-xs bg-sky-500/20 text-sky-700">KYC ✓</span>
              }
            </div>
          }
        </div>

        <!-- Reward Points -->
        <div class="rounded-xl border border-blue-200 bg-gradient-to-br from-white via-white to-blue-50 p-5 shadow-sm space-y-3">
          <div class="flex items-center justify-between">
            <p class="text-xs font-medium text-slate-500 uppercase tracking-wider">Reward Points</p>
            <span class="text-xl">&#127873;</span>
          </div>
          <app-loader [show]="loading" />
          @if (!loading) {
            <p class="text-2xl font-bold text-slate-900">{{ rewards?.pointsBalance | number }}</p>
            <div class="flex items-center gap-2">
              <span class="px-2 py-0.5 rounded-full text-xs font-semibold"
                    [ngClass]="tierBadgeClass(rewards?.tier)">
                {{ rewards?.tier ?? '—' }}
              </span>
              <span class="text-slate-500 text-xs">{{ rewards?.totalEarned | number }} lifetime</span>
            </div>
          }
        </div>

        <!-- Notifications -->
        <a routerLink="/dashboard/notifications"
           class="group rounded-xl border border-slate-200 bg-white/95 p-5 shadow-sm transition-colors hover:border-slate-300">
          <div class="flex items-center justify-between">
            <p class="text-xs font-medium text-slate-500 uppercase tracking-wider">Notifications</p>
            <span class="text-xl">&#128276;</span>
          </div>
          <app-loader [show]="loading" />
          @if (!loading) {
            <p class="text-2xl font-bold text-slate-900">{{ unreadCount }}</p>
            <p class="text-slate-500 text-xs">recent alerts</p>
          }
        </a>

        <!-- Support Tickets -->
        <a routerLink="/dashboard/support"
           class="group rounded-xl border border-slate-200 bg-white/95 p-5 shadow-sm transition-colors hover:border-slate-300">
          <div class="flex items-center justify-between">
            <p class="text-xs font-medium text-slate-500 uppercase tracking-wider">Open Tickets</p>
            <span class="text-xl">&#127915;</span>
          </div>
          <app-loader [show]="loading" />
          @if (!loading) {
            <p class="text-2xl font-bold text-slate-900">{{ openTickets }}</p>
            <p class="text-slate-500 text-xs">{{ openTickets === 1 ? 'ticket needs attention' : 'tickets active' }}</p>
          }
        </a>
      </div>

      <!-- ── Main Grid ── -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

        <!-- Recent Transactions (2/3 width) -->
        <div class="lg:col-span-2 rounded-xl border border-slate-200 bg-white/95 p-6 shadow-sm">
          <div class="flex items-center justify-between mb-5">
            <h2 class="text-base font-semibold text-slate-900">Recent Transactions</h2>
            <a routerLink="/dashboard/wallet"
               class="text-xs text-accent hover:text-accent-hover transition-colors">View all →</a>
          </div>

          <app-loader [show]="loading" />

          @if (!loading) {
            @if (recentTx.length === 0) {
              <div class="py-10 text-center text-slate-500">
                <p class="text-4xl mb-3">&#128203;</p>
                <p class="text-sm">No transactions yet</p>
              </div>
            } @else {
              <div class="space-y-1">
                @for (tx of recentTx; track tx.id) {
                  <div class="flex items-center justify-between rounded-lg px-3 py-3 transition-colors hover:bg-slate-100/80">
                    <div class="flex items-center gap-3 min-w-0">
                      <div class="w-8 h-8 rounded-full flex items-center justify-center shrink-0"
                           [class.bg-emerald-500/15]="tx.type === 'CREDIT'"
                           [class.bg-rose-500/15]="tx.type === 'DEBIT'"
                           [class.bg-sky-500/15]="tx.type !== 'CREDIT' && tx.type !== 'DEBIT'">
                        <span class="text-sm">
                          {{ tx.type === 'CREDIT' ? '&#8679;' : '&#8681;' }}
                        </span>
                      </div>
                      <div class="min-w-0">
                        <p class="truncate text-sm font-medium text-slate-800">{{ tx.description || tx.referenceType }}</p>
                        <p class="text-xs text-slate-500">{{ tx.createdAt | date:'dd MMM, HH:mm' }}</p>
                      </div>
                    </div>
                    <div class="text-right shrink-0 ml-4">
                      <p class="text-sm font-semibold"
                         [class.text-emerald-400]="tx.type === 'CREDIT'"
                         [class.text-rose-400]="tx.type === 'DEBIT'"
                        [class.text-slate-700]="tx.type !== 'CREDIT' && tx.type !== 'DEBIT'">
                        {{ tx.type === 'DEBIT' ? '−' : '+' }}{{ tx.amount | inrCurrency }}
                      </p>
                      <p class="text-xs text-slate-500 capitalize">{{ tx.referenceType }}</p>
                    </div>
                  </div>
                }
              </div>
            }
          }
        </div>

        <!-- Right Column (1/3 width) -->
        <div class="flex flex-col gap-6">

          <!-- Rewards Tier Card -->
          <div class="bg-gradient-to-br rounded-xl p-6 border border-slate-200"
               [ngClass]="tierBg">
            <div class="flex items-center justify-between mb-4">
              <h2 class="text-base font-semibold text-slate-900">Rewards</h2>
              <a routerLink="/dashboard/rewards"
                  class="text-xs text-slate-500 hover:text-slate-700 transition-colors">View →</a>
            </div>

            <app-loader [show]="loading" />

            @if (!loading && rewards) {
              <div class="space-y-4">
                <div class="flex items-end gap-2">
                  <span class="text-3xl font-bold text-slate-900">{{ rewards.pointsBalance | number }}</span>
                  <span class="mb-1 text-sm text-slate-500">pts</span>
                </div>

                <!-- Tier Badge -->
                <div class="flex items-center gap-2">
                  <span class="text-lg font-bold" [ngClass]="tierTextColor">{{ rewards.tier }}</span>
                  <span class="text-slate-600 text-xs">tier</span>
                </div>

                <!-- Progress to next tier -->
                @if (rewards.tier !== 'Platinum') {
                  <div class="space-y-1.5">
                    <div class="flex justify-between text-xs text-slate-600">
                      <span>{{ tierProgress | number:'1.0-0' }}% to {{ nextTier }}</span>
                      <span>{{ ptsToNextTier | number }} pts needed</span>
                    </div>
                    <div class="w-full bg-slate-200 rounded-full h-2">
                      <div class="h-2 rounded-full transition-all duration-500"
                           [ngClass]="tierBarColor"
                           [style.width.%]="tierProgress">
                      </div>
                    </div>
                  </div>
                } @else {
                  <p class="text-xs text-cyan-400">&#10024; You've reached the highest tier!</p>
                }

                <p class="text-xs text-slate-600">{{ rewards.totalEarned | number }} lifetime points earned</p>
              </div>
            }
          </div>

          <!-- KYC Status -->
          <div class="rounded-xl border border-slate-200 bg-white/95 p-5 shadow-sm">
            <div class="flex items-center justify-between mb-3">
              <h2 class="text-base font-semibold text-slate-900">KYC Status</h2>
              <a routerLink="/dashboard/kyc"
                 class="text-xs text-slate-500 hover:text-slate-700 transition-colors">Manage →</a>
            </div>
            <app-loader [show]="loading" />
            @if (!loading) {
              @if (kycInfo) {
                <div class="flex items-center gap-3">
                  <span class="text-2xl">
                    {{ kycInfo.status === 'Approved' ? '&#9989;' : kycInfo.status === 'Pending' ? '&#9203;' : '&#10060;' }}
                  </span>
                  <div>
                    <p class="text-sm font-semibold"
                       [class.text-emerald-700]="kycInfo.status === 'Approved'"
                       [class.text-blue-700]="kycInfo.status === 'Pending'"
                       [class.text-rose-700]="kycInfo.status === 'Rejected'">
                      {{ kycInfo.status }}
                    </p>
                    <p class="text-xs text-slate-600">{{ kycInfo.docType }}</p>
                  </div>
                </div>
              } @else {
                <div class="flex items-center gap-3">
                  <span class="text-2xl">&#128203;</span>
                  <div>
                    <p class="text-sm text-slate-600">Not submitted</p>
                    <a routerLink="/dashboard/kyc"
                       class="text-xs text-accent hover:underline">Submit KYC →</a>
                  </div>
                </div>
              }
            }
          </div>

        </div>
      </div>

      <!-- ── Recent Notifications ── -->
      <div class="rounded-xl border border-slate-200 bg-white/95 p-6 shadow-sm">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-base font-semibold text-slate-900">Recent Notifications</h2>
          <a routerLink="/dashboard/notifications"
             class="text-xs text-accent hover:text-accent-hover transition-colors">View all →</a>
        </div>
        <app-loader [show]="loading" />
        @if (!loading) {
          @if (recentNotifs.length === 0) {
            <p class="py-4 text-center text-sm text-slate-500">No notifications yet</p>
          } @else {
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
              @for (n of recentNotifs; track n.id) {
                <div class="flex items-start gap-3 rounded-lg border border-slate-200 bg-slate-50/80 p-3">
                  <span class="text-lg mt-0.5 shrink-0">
                    {{ n.type === 'Transfer' ? '&#128258;' : n.type === 'TopUp' ? '&#128176;' : n.type === 'Reward' ? '&#127873;' : '&#128276;' }}
                  </span>
                  <div class="min-w-0">
                    <p class="text-sm font-medium text-slate-800 truncate">{{ n.subject }}</p>
                    <div class="flex items-center gap-2 mt-0.5">
                      <span class="text-xs px-1.5 py-0.5 rounded"
                            [class.bg-emerald-500/15]="n.status === 'Sent'"
                            [class.text-emerald-700]="n.status === 'Sent'"
                            [class.bg-blue-500/15]="n.status !== 'Sent'"
                            [class.text-blue-700]="n.status !== 'Sent'">
                        {{ n.status }}
                      </span>
                      <span class="text-xs text-slate-500">{{ n.createdAt | date:'dd MMM' }}</span>
                    </div>
                  </div>
                </div>
              }
            </div>
          }
        }
      </div>

      <!-- ── Recent Support Tickets ── -->
      @if (recentTickets.length > 0) {
        <div class="rounded-xl border border-slate-200 bg-white/95 p-6 shadow-sm">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-base font-semibold text-slate-900">Recent Support Tickets</h2>
            <a routerLink="/dashboard/support"
               class="text-xs text-accent hover:text-accent-hover transition-colors">View all →</a>
          </div>
          <div class="space-y-2">
            @for (ticket of recentTickets; track ticket.id) {
              <div class="flex items-center justify-between py-2.5 px-3 rounded-lg hover:bg-slate-100/80 transition-colors">
                <div class="flex items-center gap-3 min-w-0">
                  <span class="text-xs font-mono text-slate-500 shrink-0">{{ ticket.ticketNumber }}</span>
                  <p class="text-sm text-slate-800 truncate">{{ ticket.subject }}</p>
                </div>
                <div class="flex items-center gap-2 shrink-0 ml-3">
                  <span class="text-xs px-2 py-0.5 rounded-full font-medium"
                        [class.bg-blue-500/20]="ticket.status === 'Open'"
                        [class.text-blue-700]="ticket.status === 'Open'"
                        [class.bg-emerald-500/20]="ticket.status === 'Resolved' || ticket.status === 'Closed'"
                        [class.text-emerald-700]="ticket.status === 'Resolved' || ticket.status === 'Closed'"
                        [class.bg-sky-500/20]="ticket.status === 'InProgress'"
                        [class.text-sky-700]="ticket.status === 'InProgress'"
                        [class.bg-zinc-500/20]="ticket.status !== 'Open' && ticket.status !== 'Resolved' && ticket.status !== 'Closed' && ticket.status !== 'InProgress'"
                        [class.text-slate-700]="ticket.status !== 'Open' && ticket.status !== 'Resolved' && ticket.status !== 'Closed' && ticket.status !== 'InProgress'">
                    {{ ticket.status }}
                  </span>
                  <span class="text-xs text-slate-500">{{ ticket.createdAt | date:'dd MMM' }}</span>
                </div>
              </div>
            }
          </div>
        </div>
      }

    </div>
  `,
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

  get greeting(): string {
    const h = new Date().getHours();
    if (h < 12) return 'Good morning';
    if (h < 17) return 'Good afternoon';
    return 'Good evening';
  }

  get firstName(): string {
    const email = this.tokenService.getUser()?.email ?? '';
    return email.split('@')[0];
  }

  get unreadCount(): number {
    return this.recentNotifs.length;
  }

  get openTickets(): number {
    return this.recentTickets.filter((t) => t.status === 'Open' || t.status === 'InProgress').length;
  }

  get tierBg(): string {
    return TIER_CONFIG[this.rewards?.tier ?? 'Bronze']?.bg ?? TIER_CONFIG['Bronze'].bg;
  }

  get tierTextColor(): string {
    return TIER_CONFIG[this.rewards?.tier ?? 'Bronze']?.color ?? '';
  }

  get nextTier(): string {
    return TIER_CONFIG[this.rewards?.tier ?? 'Bronze']?.next ?? '';
  }

  get tierProgress(): number {
    const tier = this.rewards?.tier ?? 'Bronze';
    const cfg = TIER_CONFIG[tier];
    if (!cfg || tier === 'Platinum') return 100;
    const pts = this.rewards?.totalEarned ?? 0;
    return Math.min(100, Math.max(0, ((pts - cfg.min) / (cfg.max - cfg.min)) * 100));
  }

  get ptsToNextTier(): number {
    const tier = this.rewards?.tier ?? 'Bronze';
    const cfg = TIER_CONFIG[tier];
    if (!cfg || tier === 'Platinum') return 0;
    return Math.max(0, cfg.max - (this.rewards?.totalEarned ?? 0));
  }

  get tierBarColor(): string {
    const map: Record<string, string> = {
      Bronze: 'bg-blue-500',
      Silver: 'bg-slate-500',
      Gold: 'bg-cyan-500',
      Platinum: 'bg-cyan-400',
    };
    return map[this.rewards?.tier ?? 'Bronze'] ?? 'bg-accent';
  }

  tierBadgeClass(tier?: string): string {
    const map: Record<string, string> = {
      Bronze:   'bg-blue-500/20 text-blue-700',
      Silver:   'bg-slate-300/60 text-slate-700',
      Gold:     'bg-cyan-500/20 text-cyan-700',
      Platinum: 'bg-cyan-500/20 text-cyan-700',
    };
    return map[tier ?? 'Bronze'] ?? 'bg-slate-200 text-slate-700';
  }

  ngOnInit(): void {
    forkJoin({
      wallet:        this.walletService.getWallet().pipe(catchError(() => of(null))),
      transactions:  this.walletService.getTransactions(1, 5).pipe(catchError(() => of(null))),
      rewards:       this.rewardsService.getRewards().pipe(catchError(() => of(null))),
      notifications: this.notificationsService.getNotifications(1, 5).pipe(catchError(() => of(null))),
      tickets:       this.supportService.getTickets().pipe(catchError(() => of([] as SupportTicketDto[]))),
      kyc:           this.authService.getKycStatus().pipe(catchError(() => of([] as KycInfo[]))),
    }).subscribe(({ wallet, transactions, rewards, notifications, tickets, kyc }) => {
      const latestKyc =
        kyc?.length
          ? [...kyc].sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime())[0]
          : null;

      this.wallet       = wallet;
      this.recentTx     = transactions?.items?.slice(0, 5) ?? [];
      this.rewards      = rewards;
      this.recentNotifs = notifications?.items?.slice(0, 6) ?? [];
      this.recentTickets = (tickets ?? []).slice(0, 3);
      this.kycInfo      =
        latestKyc && latestKyc.status === 'Pending' && wallet?.kycVerified
          ? { ...latestKyc, status: 'Approved' }
          : latestKyc;
      this.loading      = false;
    });
  }
}
