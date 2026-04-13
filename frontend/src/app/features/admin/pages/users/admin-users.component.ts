import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { AdminService } from '../../admin.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { LoaderComponent } from '../../../../shared/components/loader/loader.component';
import { UserDto } from '../../../../shared/models/auth.model';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [DatePipe, FormsModule, LoaderComponent],
  template: `
    <div class="space-y-4">

      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h2 class="text-xl font-display font-bold text-zinc-100">User Management</h2>
          <p class="text-zinc-500 text-sm mt-0.5">
            @if (loading) {
              Loading users...
            } @else {
              Showing {{ filteredUsers.length }} of {{ users.length }} users
            }
          </p>
        </div>
        <button
          type="button"
          (click)="loadUsers()"
          [disabled]="loading"
          class="shrink-0 px-4 py-2 rounded-xl border border-zinc-700 text-sm text-zinc-300 hover:border-amber-400/40 hover:text-amber-300 transition-colors disabled:opacity-50"
        >
          ↻ Refresh
        </button>
      </div>

      <!-- Filters -->
      <div class="flex flex-wrap gap-3">
        <!-- Search -->
        <input
          type="text"
          [(ngModel)]="searchTerm"
          placeholder="Search by email, phone, or user ID…"
          class="flex-1 min-w-52 px-3 py-2 border border-zinc-700 bg-zinc-950 rounded-xl text-sm text-zinc-100 placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-amber-400/30"
        />
        <!-- Role filter -->
        <select
          [(ngModel)]="filterRole"
          class="px-3 py-2 border border-zinc-700 bg-zinc-950 rounded-xl text-sm text-zinc-300 focus:outline-none focus:ring-2 focus:ring-amber-400/30"
        >
          <option value="">All Roles</option>
          <option value="User">User</option>
          <option value="Admin">Admin</option>
        </select>
        <!-- Status filter -->
        <select
          [(ngModel)]="filterStatus"
          class="px-3 py-2 border border-zinc-700 bg-zinc-950 rounded-xl text-sm text-zinc-300 focus:outline-none focus:ring-2 focus:ring-amber-400/30"
        >
          <option value="">All Statuses</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive / Blocked</option>
        </select>

        <!-- Active filter badges -->
        @if (activeBadges.length > 0) {
          <button
            type="button"
            (click)="clearFilters()"
            class="px-3 py-2 rounded-xl border border-zinc-700 text-xs text-zinc-400 hover:text-rose-400 hover:border-rose-400/40 transition-colors"
          >
            ✕ Clear filters ({{ activeBadges.length }})
          </button>
        }
      </div>

      <app-loader [show]="loading" />

      @if (!loading) {
        @if (users.length === 0) {
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl p-12 text-center">
            <p class="text-zinc-500 text-sm">No users found.</p>
          </div>
        } @else if (filteredUsers.length === 0) {
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl p-12 text-center">
            <p class="text-zinc-500 text-sm">No users match your current filters.</p>
            <button (click)="clearFilters()" class="mt-2 text-amber-400 text-xs hover:underline">Clear filters</button>
          </div>
        } @else {
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl overflow-hidden">
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-zinc-700 text-zinc-400 text-left">
                    <th class="px-4 py-3 font-medium">User</th>
                    <th class="px-4 py-3 font-medium">Phone</th>
                    <th class="px-4 py-3 font-medium">Role</th>
                    <th class="px-4 py-3 font-medium">Status</th>
                    <th class="px-4 py-3 font-medium">Joined</th>
                    <th class="px-4 py-3 font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-zinc-800">
                  @for (user of filteredUsers; track user.id) {
                    <tr class="hover:bg-zinc-800/60 transition-colors">
                      <td class="px-4 py-3">
                        <p class="text-zinc-100 text-sm font-medium">{{ user.email }}</p>
                        <p class="text-zinc-500 text-xs mt-0.5 font-mono">{{ user.id.slice(0, 12) }}…</p>
                      </td>
                      <td class="px-4 py-3 text-zinc-400 text-sm">{{ user.phone || '—' }}</td>
                      <td class="px-4 py-3">
                        <span
                          class="px-2 py-0.5 rounded-full text-xs font-semibold"
                          [class.bg-fuchsia-500/20]="user.role === 'Admin'"
                          [class.text-fuchsia-300]="user.role === 'Admin'"
                          [class.bg-zinc-700]="user.role !== 'Admin'"
                          [class.text-zinc-300]="user.role !== 'Admin'"
                        >{{ user.role }}</span>
                      </td>
                      <td class="px-4 py-3">
                        <span
                          class="px-2 py-0.5 rounded-full text-xs font-semibold"
                          [class.bg-emerald-500/20]="user.isActive"
                          [class.text-emerald-300]="user.isActive"
                          [class.bg-rose-500/20]="!user.isActive"
                          [class.text-rose-300]="!user.isActive"
                        >{{ user.isActive ? 'Active' : 'Blocked' }}</span>
                      </td>
                      <td class="px-4 py-3 text-zinc-500 text-xs">{{ user.createdAt | date:'dd MMM yyyy' }}</td>
                      <td class="px-4 py-3">
                        <button
                          type="button"
                          (click)="toggleUserStatus(user)"
                          [disabled]="userStatusLoadingId === user.id"
                          class="px-3 py-1.5 rounded-lg text-xs font-semibold border text-white transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                          [class]="user.isActive
                            ? 'bg-rose-600 hover:bg-rose-700 border-rose-500'
                            : 'bg-emerald-600 hover:bg-emerald-700 border-emerald-500'"
                        >
                          {{ userStatusLoadingId === user.id
                            ? 'Updating…'
                            : user.isActive ? 'Block User' : 'Unblock'
                          }}
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        }
      }
    </div>
  `,
})
export class AdminUsersComponent implements OnInit {
  loading = false;
  users: UserDto[] = [];
  userStatusLoadingId: string | null = null;

  searchTerm = '';
  filterRole: '' | 'Admin' | 'User' = '';
  filterStatus: '' | 'active' | 'inactive' = '';

  get filteredUsers(): UserDto[] {
    const q = this.searchTerm.toLowerCase();
    return this.users.filter((u) => {
      const matchSearch =
        !q ||
        u.email.toLowerCase().includes(q) ||
        (u.phone ?? '').toLowerCase().includes(q) ||
        u.id.toLowerCase().includes(q);
      const matchRole = !this.filterRole || u.role === this.filterRole;
      const matchStatus =
        !this.filterStatus ||
        (this.filterStatus === 'active' ? u.isActive : !u.isActive);
      return matchSearch && matchRole && matchStatus;
    });
  }

  get activeBadges(): string[] {
    const badges: string[] = [];
    if (this.filterRole) badges.push(`Role: ${this.filterRole}`);
    if (this.filterStatus) badges.push(`Status: ${this.filterStatus}`);
    if (this.searchTerm) badges.push(`Search`);
    return badges;
  }

  constructor(
    private readonly adminService: AdminService,
    private readonly toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.adminService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.loading = false;
      },
      error: (err: { message?: string }) => {
        this.loading = false;
        this.toastService.error(err.message ?? 'Failed to load users.');
      },
    });
  }

  toggleUserStatus(user: UserDto): void {
    this.userStatusLoadingId = user.id;
    this.adminService.updateUserStatus(user.id, !user.isActive).subscribe({
      next: (updated) => {
        user.isActive = updated.isActive;
        this.userStatusLoadingId = null;
        this.toastService.success(`User ${updated.isActive ? 'unblocked' : 'blocked'} successfully.`);
      },
      error: (err: { message?: string }) => {
        this.userStatusLoadingId = null;
        this.toastService.error(err.message ?? 'Failed to update user status.');
      },
    });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.filterRole = '';
    this.filterStatus = '';
  }
}
