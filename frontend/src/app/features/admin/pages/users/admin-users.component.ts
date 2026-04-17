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
  templateUrl: './admin-users.component.html',
})
export class AdminUsersComponent implements OnInit {
  loading = false;
  users: UserDto[] = [];
  userStatusLoadingId: string | null = null;

  searchTerm = '';
  filterRole: '' | 'Admin' | 'User' = '';
  filterStatus: '' | 'active' | 'inactive' = '';

  // Filter the loaded users in-memory using the current search, role, and status controls.
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

  // Build the active filter badge labels shown above the user table.
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

  // Load all users for the admin management table.
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

  // Toggle a user's active state and patch the updated status into the existing row.
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

  // Reset every user filter back to its default empty state.
  clearFilters(): void {
    this.searchTerm = '';
    this.filterRole = '';
    this.filterStatus = '';
  }
}
