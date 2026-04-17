import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, Validators } from '@angular/forms';

import { AdminService } from '../../admin.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { LoaderComponent } from '../../../../shared/components/loader/loader.component';
import { TicketDetailDto, TicketSummaryDto } from '../../../../shared/models/admin.model';

@Component({
  selector: 'app-admin-tickets',
  standalone: true,
  imports: [DatePipe, FormsModule, ReactiveFormsModule, LoaderComponent],
  templateUrl: './admin-tickets.component.html',
})
export class AdminTicketsComponent implements OnInit {
  loading = false;
  tickets: TicketSummaryDto[] = [];
  totalCount = 0;
  ticketDetailLoadingId: string | null = null;
  viewTicket: TicketDetailDto | null = null;
  replyLoading = false;
  closeLoading = false;
  replySubmitted = false;

  filterStatus = '';
  filterPriority = '';
  filterCategory = '';
  currentPage = 1;
  readonly pageSize = 20;

  readonly statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Open', label: 'Open' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Responded', label: 'Responded' },
    { value: 'Resolved', label: 'Resolved' },
    { value: 'Closed', label: 'Closed' },
  ];
  readonly priorityOptions = [
    { value: '', label: 'All Priorities' },
    { value: 'Urgent', label: 'Urgent' },
    { value: 'High', label: 'High' },
    { value: 'Medium', label: 'Medium' },
    { value: 'Low', label: 'Low' },
  ];
  readonly categoryOptions = [
    { value: '', label: 'All Categories' },
    { value: 'Payment', label: 'Payment' },
    { value: 'Account', label: 'Account' },
    { value: 'KYC', label: 'KYC' },
    { value: 'Rewards', label: 'Rewards' },
    { value: 'Other', label: 'Other' },
  ];

  readonly replyForm;

  constructor(
    fb: FormBuilder,
    private readonly adminService: AdminService,
    private readonly toastService: ToastService,
  ) {
    this.replyForm = fb.nonNullable.group({ reply: ['', Validators.required] });
  }

  ngOnInit(): void {
    this.loadTickets();
  }

  // Load the admin ticket table using the current filter set and pagination values.
  loadTickets(): void {
    this.loading = true;
    this.adminService.getTicketsPaginated({
      status:   this.filterStatus   || undefined,
      priority: this.filterPriority || undefined,
      category: this.filterCategory || undefined,
      page: this.currentPage,
      size: this.pageSize,
    }).subscribe({
      next: (result) => {
        this.tickets = result.items ?? [];
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: (err: { message?: string }) => {
        this.loading = false;
        this.toastService.error(err.message ?? 'Failed to load tickets.');
      },
    });
  }

  // Reset paging whenever a filter changes so results always start from the first page.
  onFilterChange(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  // Clear every ticket filter and reload the default result set.
  clearFilters(): void {
    this.filterStatus = '';
    this.filterPriority = '';
    this.filterCategory = '';
    this.onFilterChange();
  }

  // Load one ticket's full details and open the reply modal for it.
  openTicket(ticket: TicketSummaryDto): void {
    this.ticketDetailLoadingId = ticket.id;
    this.adminService.getTicket(ticket.id).subscribe({
      next: (detail) => {
        this.viewTicket = detail;
        this.ticketDetailLoadingId = null;
        this.replySubmitted = false;
        this.replyForm.reset();
      },
      error: (err: { message?: string }) => {
        this.ticketDetailLoadingId = null;
        this.toastService.error(err.message ?? 'Failed to load ticket details.');
      },
    });
  }

  // Close the ticket detail modal and clear reply validation state.
  closeView(): void {
    this.viewTicket = null;
    this.replySubmitted = false;
  }

  // Send the current admin reply and refresh the table to reflect the latest ticket state.
  submitReply(): void {
    this.replySubmitted = true;
    if (this.replyForm.invalid || !this.viewTicket) return;
    this.replyLoading = true;
    const message = this.replyForm.getRawValue().reply;
    this.adminService.replyToTicket(this.viewTicket.id, message).subscribe({
      next: () => {
        this.replyLoading = false;
        this.replyForm.reset();
        this.replySubmitted = false;
        // Update the open detail modal immediately so the new reply appears without reopening it.
        if (this.viewTicket) this.viewTicket.adminReply = message;
        this.toastService.success('Reply sent successfully.');
        this.loadTickets();
      },
      error: (err: { message?: string }) => {
        this.replyLoading = false;
        this.toastService.error(err.message ?? 'Failed to send reply.');
      },
    });
  }

  // Close the current ticket and refresh the table to reflect the new status.
  closeTicketAction(): void {
    if (!this.viewTicket) return;
    this.closeLoading = true;
    this.adminService.closeTicket(this.viewTicket.id).subscribe({
      next: () => {
        this.closeLoading = false;
        this.viewTicket = null;
        this.toastService.success('Ticket closed.');
        this.loadTickets();
      },
      error: (err: { message?: string }) => {
        this.closeLoading = false;
        this.toastService.error(err.message ?? 'Failed to close ticket.');
      },
    });
  }
}
