import { Component, OnInit } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { AdminService } from '../../admin.service';
import { LoaderComponent } from '../../../../shared/components/loader/loader.component';
import { DecimalPipe } from '@angular/common';
import {
  AdminDashboardStatsDto,
  TicketSummaryDto,
  WalletAdminStatsDto,
} from '../../../../shared/models/admin.model';
import { UserDto } from '../../../../shared/models/auth.model';
import { PaginatedResult } from '../../../../shared/models/paginated-result.model';

interface TrendBar {
  label: string;
  count: number;
  barPct: number;
  barPx: number;
}

interface StatCard {
  label: string;
  value: string | number;
  color: string;
  available: boolean;
  tooltip?: string;
}

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [DecimalPipe, LoaderComponent],
  template: `
    <div class="space-y-6">

      <!-- Header -->
      <div>
        <h2 class="text-xl font-display font-bold text-zinc-100">System Overview</h2>
        <p class="text-zinc-500 text-sm mt-1">Real-time stats from all services</p>
      </div>

      <app-loader [show]="loading" />

      @if (!loading) {

        <!-- ── Stat Cards ── -->
        <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          @for (card of statCards; track card.label) {
            <div
              class="rounded-xl p-4 border transition-all"
              [class.bg-zinc-900]="card.available"
              [class.border-zinc-800]="card.available"
              [class.bg-zinc-900/40]="!card.available"
              [class.border-zinc-800/50]="!card.available"
              [attr.title]="card.tooltip ?? null"
            >
              <p class="text-xs uppercase tracking-wider truncate"
                 [class.text-zinc-500]="card.available"
                 [class.text-zinc-600]="!card.available">
                {{ card.label }}
              </p>
              <div class="flex items-end justify-between mt-2 gap-1">
                <p class="text-2xl font-bold leading-none"
                   [class]="card.available ? card.color : 'text-zinc-600'">
                  {{ card.value }}
                </p>
                @if (!card.available) {
                  <span class="text-[10px] text-zinc-600 border border-zinc-700 rounded px-1 cursor-help shrink-0">N/A</span>
                }
              </div>
            </div>
          }
        </div>

        <!-- ── Analytics Row ── -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">

          <!-- User Registration Trend -->
          <div class="lg:col-span-2 bg-zinc-900 border border-zinc-800 rounded-xl p-5">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-sm font-semibold text-zinc-200">User Registrations</h3>
              <span class="text-xs text-zinc-500">Last 6 months</span>
            </div>
            <!-- Bar area: fixed 80px height, bars grow from bottom -->
            <div class="flex items-end gap-2" style="height:80px">
              @for (bar of registrationTrend; track bar.label) {
                <div class="flex-1 flex flex-col items-center justify-end gap-1">
                  <span class="text-[10px] text-zinc-500 font-medium leading-none">{{ bar.count }}</span>
                  <div
                    class="w-full bg-gradient-to-t from-amber-500 to-amber-300 rounded-t-sm transition-all duration-700"
                    [style.height.px]="bar.barPx"
                  ></div>
                </div>
              }
            </div>
            <!-- Label row below -->
            <div class="flex gap-2 mt-2">
              @for (bar of registrationTrend; track bar.label) {
                <span class="flex-1 text-center text-[10px] text-zinc-500">{{ bar.label }}</span>
              }
            </div>
          </div>

          <!-- KYC Stats -->
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl p-5 space-y-5">
            <h3 class="text-sm font-semibold text-zinc-200">KYC Today</h3>

            <div class="space-y-3">
              <!-- Approval Rate -->
              <div>
                <div class="flex justify-between text-xs text-zinc-400 mb-1.5">
                  <span>Approval Rate</span>
                  <span class="font-semibold text-emerald-400">{{ kycApprovalRate }}%</span>
                </div>
                <div class="h-2 bg-zinc-800 rounded-full overflow-hidden">
                  <div
                    class="h-full bg-gradient-to-r from-emerald-500 to-emerald-400 rounded-full transition-all duration-700"
                    [style.width.%]="kycApprovalRate"
                  ></div>
                </div>
              </div>

              <!-- Stats breakdown -->
              <div class="grid grid-cols-3 gap-2 pt-2">
                <div class="text-center">
                  <p class="text-lg font-bold text-amber-300">{{ stats?.pendingKYCCount ?? 0 }}</p>
                  <p class="text-[10px] text-zinc-500 mt-0.5">Pending</p>
                </div>
                <div class="text-center">
                  <p class="text-lg font-bold text-emerald-400">{{ stats?.approvedKYCToday ?? 0 }}</p>
                  <p class="text-[10px] text-zinc-500 mt-0.5">Approved</p>
                </div>
                <div class="text-center">
                  <p class="text-lg font-bold text-rose-400">{{ stats?.rejectedKYCToday ?? 0 }}</p>
                  <p class="text-[10px] text-zinc-500 mt-0.5">Rejected</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- ── User Breakdown ── -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

          <!-- User Activity Bar -->
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl p-5">
            <h3 class="text-sm font-semibold text-zinc-200 mb-4">User Activity Breakdown</h3>
            <div class="space-y-3">
              <div>
                <div class="flex justify-between text-xs text-zinc-400 mb-1.5">
                  <span>Active Users</span>
                  <span class="font-semibold text-emerald-400">{{ activeUsers }} / {{ totalUsers }}</span>
                </div>
                <div class="h-2 bg-zinc-800 rounded-full overflow-hidden">
                  <div
                    class="h-full bg-emerald-500 rounded-full transition-all duration-700"
                    [style.width.%]="totalUsers > 0 ? (activeUsers / totalUsers * 100) : 0"
                  ></div>
                </div>
              </div>
              <div>
                <div class="flex justify-between text-xs text-zinc-400 mb-1.5">
                  <span>Blocked Users</span>
                  <span class="font-semibold text-rose-400">{{ blockedUsers }} / {{ totalUsers }}</span>
                </div>
                <div class="h-2 bg-zinc-800 rounded-full overflow-hidden">
                  <div
                    class="h-full bg-rose-500 rounded-full transition-all duration-700"
                    [style.width.%]="totalUsers > 0 ? (blockedUsers / totalUsers * 100) : 0"
                  ></div>
                </div>
              </div>
            </div>
          </div>

          <!-- Financial Insights (real data) -->
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl p-5">
            <h3 class="text-sm font-semibold text-zinc-200 mb-4">Financial Insights</h3>
            <div class="space-y-3">

              <div class="flex items-center justify-between py-2 border-b border-zinc-800 last:border-0">
                <span class="text-sm text-zinc-400">Total Transaction Volume</span>
                <span class="text-sm font-semibold text-emerald-300">
                  ₹{{ (walletStats?.totalVolume ?? 0) | number:'1.2-2' }}
                </span>
              </div>

              <div class="flex items-center justify-between py-2 border-b border-zinc-800 last:border-0">
                <span class="text-sm text-zinc-400">Today's Volume</span>
                <span class="text-sm font-semibold text-amber-300">
                  ₹{{ (walletStats?.todaysVolume ?? 0) | number:'1.2-2' }}
                </span>
              </div>

              <div class="flex items-center justify-between py-2 border-b border-zinc-800 last:border-0">
                <span class="text-sm text-zinc-400">Total Transactions</span>
                <span class="text-sm font-semibold text-sky-300">
                  {{ (walletStats?.totalTransactionCount ?? 0) | number }}
                </span>
              </div>

              <div class="flex items-center justify-between py-2 border-b border-zinc-800 last:border-0">
                <span class="text-sm text-zinc-400">Today's Transactions</span>
                <span class="text-sm font-semibold text-zinc-200">
                  {{ (walletStats?.todaysTransactionCount ?? 0) | number }}
                </span>
              </div>

              <div class="flex items-center justify-between py-2 border-b border-zinc-800 last:border-0">
                <span class="text-sm text-zinc-400">Avg. Transaction Value</span>
                <span class="text-sm font-semibold text-zinc-200">
                  ₹{{ (walletStats?.averageTransactionValue ?? 0) | number:'1.2-2' }}
                </span>
              </div>

              <div class="flex items-center justify-between py-2">
                <span class="text-sm text-zinc-400">Failed Transactions</span>
                <span class="text-sm font-semibold"
                  [class.text-rose-400]="(walletStats?.failedTransactions ?? 0) > 0"
                  [class.text-zinc-500]="(walletStats?.failedTransactions ?? 0) === 0">
                  {{ (walletStats?.failedTransactions ?? 0) | number }}
                </span>
              </div>

            </div>
          </div>
        </div>

      }
    </div>
  `,
})
export class AdminOverviewComponent implements OnInit {
  loading = true;

  stats: AdminDashboardStatsDto | null = null;
  walletStats: WalletAdminStatsDto | null = null;
  totalUsers = 0;
  activeUsers = 0;
  blockedUsers = 0;
  totalTickets = 0;

  registrationTrend: TrendBar[] = [];
  kycApprovalRate = 0;

  statCards: StatCard[] = [];

  constructor(private readonly adminService: AdminService) {}

  ngOnInit(): void {
    forkJoin({
      stats:       this.adminService.getDashboardStats().pipe(catchError(() => of(null))),
      users:       this.adminService.getUsers().pipe(catchError(() => of([]))),
      tickets:     this.adminService.getTicketsPaginated({ page: 1, size: 1 }).pipe(catchError(() => of(null))),
      walletStats: this.adminService.getWalletStats().pipe(catchError(() => of(null))),
    }).subscribe(({ stats, users, tickets, walletStats }) => {
      this.loading = false;

      this.stats       = stats as AdminDashboardStatsDto | null;
      this.walletStats = walletStats as WalletAdminStatsDto | null;

      const userList = users as UserDto[];
      this.totalUsers  = userList.length;
      this.activeUsers = userList.filter((u) => u.isActive).length;
      this.blockedUsers = userList.filter((u) => !u.isActive).length;
      this.totalTickets =
        (tickets as PaginatedResult<TicketSummaryDto> | null)?.totalCount ?? 0;

      this.buildRegistrationTrend(userList);
      this.buildKycApprovalRate();
      this.buildStatCards();
    });
  }

  private buildRegistrationTrend(users: UserDto[]): void {
    const now = new Date();
    const months: TrendBar[] = [];

    for (let i = 5; i >= 0; i--) {
      const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
      months.push({
        label: d.toLocaleString('default', { month: 'short' }),
        count: 0,
        barPct: 0,
        barPx: 4,
      });
    }

    users.forEach((u) => {
      const created = new Date(u.createdAt);
      const monthsAgo =
        (now.getFullYear() - created.getFullYear()) * 12 +
        (now.getMonth() - created.getMonth());
      if (monthsAgo >= 0 && monthsAgo <= 5) {
        months[5 - monthsAgo].count++;
      }
    });

    const maxCount = Math.max(...months.map((m) => m.count), 1);
    this.registrationTrend = months.map((m) => ({
      ...m,
      barPct: Math.round((m.count / maxCount) * 100),
      barPx: Math.max(4, Math.round((m.count / maxCount) * 64)),
    }));
  }

  private buildKycApprovalRate(): void {
    if (!this.stats) return;
    const total = this.stats.approvedKYCToday + this.stats.rejectedKYCToday;
    this.kycApprovalRate =
      total === 0 ? 0 : Math.round((this.stats.approvedKYCToday / total) * 100);
  }

  private buildStatCards(): void {
    const s = this.stats;
    const w = this.walletStats;
    this.statCards = [
      { label: 'Total Users',         value: this.totalUsers,                color: 'text-zinc-100',    available: true },
      { label: 'Active Users',        value: this.activeUsers,               color: 'text-emerald-400', available: true },
      { label: 'Blocked Users',       value: this.blockedUsers,              color: 'text-rose-400',    available: true },
      { label: 'Total Tickets',       value: this.totalTickets,              color: 'text-sky-400',     available: true },
      { label: 'Pending KYC',         value: s?.pendingKYCCount ?? 0,        color: 'text-amber-300',   available: true },
      { label: 'KYC Approved Today',  value: s?.approvedKYCToday ?? 0,       color: 'text-emerald-400', available: true },
      { label: 'KYC Rejected Today',  value: s?.rejectedKYCToday ?? 0,       color: 'text-rose-400',    available: true },
      { label: 'Admin Actions Today', value: s?.adminActionsToday ?? 0,      color: 'text-fuchsia-400', available: true },
      { label: 'Total Transactions',  value: w?.totalTransactionCount ?? 0, color: 'text-sky-300',     available: true },
      { label: 'Total Volume (₹)',    value: `₹${(w?.totalVolume ?? 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`, color: 'text-emerald-300', available: true },
      { label: "Today's Volume (₹)",  value: `₹${(w?.todaysVolume ?? 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`, color: 'text-amber-300',   available: true },
    ];
  }
}
