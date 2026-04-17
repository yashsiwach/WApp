import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { SupportService } from './support.service';
import { ToastService } from '../../shared/services/toast.service';
import { SupportTicketDto } from '../../shared/models/support.model';

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './support.component.html',
  
})
export class SupportComponent implements OnInit {
  loading = false;
  tickets: SupportTicketDto[] = [];
  selectedTicket: SupportTicketDto | null = null;

  showCreate = false;
  createLoading = false;
  submitted = false;
  readonly createForm;

  constructor(
    fb: FormBuilder,
    private readonly supportService: SupportService,
    private readonly toastService: ToastService,
  ) {
    this.createForm = fb.nonNullable.group({
      subject:     ['', Validators.required],
      category:    ['Payment' as 'Payment' | 'Account' | 'KYC' | 'Rewards' | 'Other'],
      priority:    ['Medium' as 'Low' | 'Medium' | 'High' | 'Urgent'],
      description: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadTickets();
  }

  // Load the user's ticket list for the support inbox.
  loadTickets(): void {
    this.loading = true;
    this.supportService.getTickets().subscribe({
      next: (t) => {
        this.tickets = t;
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  // Load a single ticket with reply history before showing it in the detail panel.
  openTicket(ticket: SupportTicketDto): void {
    this.loading = true;
    this.supportService.getTicket(ticket.id).subscribe({
      next: (t) => {
        this.selectedTicket = t;
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  // Submit the new ticket form and refresh the ticket list after creation.
  submitCreate(): void {
    this.submitted = true;
    if (this.createForm.invalid) return;

    this.createLoading = true;
    this.supportService.createTicket(this.createForm.getRawValue()).subscribe({
      next: () => {
        this.toastService.success('Ticket created successfully!');
        this.closeCreate();
        this.loadTickets();
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Failed to create ticket');
        this.createLoading = false;
      },
    });
  }

  // Close the create modal and restore the form to its default values.
  closeCreate(): void {
    this.showCreate = false;
    this.createLoading = false;
    this.submitted = false;
    this.createForm.reset({ subject: '', description: '', category: 'Payment', priority: 'Medium' });
  }

  // Convert support statuses into the badge styles used in the template.
  statusClass(status: string): string {
    switch (status) {
      case 'Open':       return 'bg-blue-500/20 text-blue-700';
      case 'InProgress': return 'bg-sky-500/20 text-sky-700';
      case 'Responded':  return 'bg-fuchsia-500/20 text-fuchsia-300';
      case 'Resolved':   return 'bg-emerald-500/20 text-emerald-700';
      case 'Closed':     return 'bg-slate-200 text-slate-700';
      default:           return 'bg-slate-200 text-slate-700';
    }
  }
}
