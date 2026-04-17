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
  templateUrl: './admin-overview.component.html',

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

  // Load the overview metrics in parallel and derive the secondary dashboard summaries from them.
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

      // Derive user activity and ticket totals from the fallback-safe response payloads.
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

  // Build a six-month registration trend used by the overview chart.
  private buildRegistrationTrend(users: UserDto[]): void {
    const now = new Date();
    const months: TrendBar[] = [];

    for (let i = 5; i >= 0; i--) {
      // Seed the chart with the current month plus the previous five months.
      const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
      months.push({
        label: d.toLocaleString('default', { month: 'short' }),
        count: 0,
        barPct: 0,
        barPx: 4,
      });
    }

    users.forEach((u) => {
      // Bucket each user into the matching month slot when it falls within the visible range.
      const created = new Date(u.createdAt);
      const monthsAgo =
        (now.getFullYear() - created.getFullYear()) * 12 +
        (now.getMonth() - created.getMonth());
      if (monthsAgo >= 0 && monthsAgo <= 5) {
        months[5 - monthsAgo].count++;
      }
    });

    // Convert raw monthly counts into percentages and pixel heights for the bar chart.
    const maxCount = Math.max(...months.map((m) => m.count), 1);
    this.registrationTrend = months.map((m) => ({
      ...m,
      barPct: Math.round((m.count / maxCount) * 100),
      barPx: Math.max(4, Math.round((m.count / maxCount) * 64)),
    }));
  }

  // Calculate the daily KYC approval percentage for the summary card.
  private buildKycApprovalRate(): void {
    if (!this.stats) return;
    const total = this.stats.approvedKYCToday + this.stats.rejectedKYCToday;
    this.kycApprovalRate =
      total === 0 ? 0 : Math.round((this.stats.approvedKYCToday / total) * 100);
  }

  // Assemble the admin stat cards from both dashboard and wallet metrics.
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
