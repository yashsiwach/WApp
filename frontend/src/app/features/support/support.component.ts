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
  template: `
    <div class="mx-auto max-w-4xl space-y-6 p-6 text-slate-900">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-display font-bold text-slate-900">Support Tickets</h1>
        <button
          (click)="showCreate = true"
          class="px-4 py-2 bg-accent hover:bg-accent-hover text-white rounded-lg font-medium text-sm transition-colors"
        >
          + New Ticket
        </button>
      </div>

      <!-- ── Shimmer skeletons while loading ── -->
      @if (loading && !selectedTicket) {
        <div class="space-y-3">
          @for (i of [1, 2, 3]; track i) {
            <div class="animate-pulse rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <div class="flex items-start justify-between gap-3">
                <div class="flex-1 space-y-2">
                  <div class="h-4 w-2/3 rounded bg-slate-200"></div>
                  <div class="h-3 w-1/3 rounded bg-slate-100"></div>
                </div>
                <div class="h-5 w-20 shrink-0 rounded-full bg-slate-200"></div>
              </div>
            </div>
          }
        </div>
      }

      @if (!loading) {
        <!-- ── Ticket detail view ── -->
        @if (selectedTicket) {
          <div class="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <div class="flex items-center gap-3 mb-5">
              <button
                (click)="selectedTicket = null"
                class="text-lg font-bold text-slate-400 hover:text-slate-700"
              >← Back</button>
              <h2 class="flex-1 font-semibold text-slate-900">{{ selectedTicket.subject }}</h2>
              <span
                class="px-2 py-0.5 rounded-full text-xs font-semibold shrink-0"
                [class]="statusClass(selectedTicket.status)"
              >{{ selectedTicket.status }}</span>
            </div>

            <!-- Ticket number + metadata -->
            <p class="mb-4 text-xs text-slate-500">
              #{{ selectedTicket.ticketNumber }} &bull;
              {{ selectedTicket.category }} &bull;
              Priority: {{ selectedTicket.priority }} &bull;
              {{ selectedTicket.createdAt | date:'dd MMM yyyy, HH:mm' }}
            </p>

            <!-- User's original message -->
            <div class="mb-3 rounded-lg border border-slate-200 bg-slate-50 p-4">
              <p class="mb-1 text-xs font-medium text-slate-500">You</p>
              <p class="text-sm text-slate-700">{{ selectedTicket.description }}</p>
            </div>

            <!-- Admin reply (if any) -->
            @if (selectedTicket.adminReply) {
              <div class="bg-blue-500/10 rounded-lg p-4 mb-3 border border-blue-500/20">
                <p class="text-xs font-medium text-blue-700 mb-1">
                  Support Agent
                  @if (selectedTicket.respondedAt) {
                    — {{ selectedTicket.respondedAt | date:'dd MMM, HH:mm' }}
                  }
                </p>
                <p class="text-sm text-slate-700">{{ selectedTicket.adminReply }}</p>
              </div>
            } @else {
              <p class="text-xs italic text-slate-500">Awaiting reply from our support team...</p>
            }
          </div>
        }

        <!-- ── Ticket list ── -->
        @if (!selectedTicket) {
          @if (tickets.length === 0) {
            <!-- Empty state -->
            <div class="py-16 text-center text-slate-500">
              <div class="text-5xl mb-3">🎫</div>
              <p class="font-medium text-slate-600">No tickets yet</p>
              <p class="text-sm mt-1">Create one if you need help!</p>
            </div>
          } @else {
            <div class="space-y-3">
              @for (ticket of tickets; track ticket.id) {
                <div
                  class="cursor-pointer rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition-colors hover:border-slate-300"
                  (click)="openTicket(ticket)"
                >
                  <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                      <p class="truncate font-medium text-slate-900">{{ ticket.subject }}</p>
                      <p class="mt-0.5 text-xs text-slate-500">
                        #{{ ticket.ticketNumber }} &bull; {{ ticket.category }} &bull;
                        {{ ticket.createdAt | date:'dd MMM yyyy' }}
                      </p>
                    </div>
                    <div class="flex gap-2 items-center shrink-0">
                      <span
                        class="px-2 py-0.5 rounded-full text-xs font-semibold"
                        [class]="statusClass(ticket.status)"
                      >{{ ticket.status }}</span>
                      @if (ticket.adminReply) {
                        <span class="text-xs text-slate-500">1 reply</span>
                      }
                    </div>
                  </div>
                </div>
              }
            </div>
          }
        }
      }
    </div>

    <!-- ── Create Ticket Modal ── -->
    @if (showCreate) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-xl">
          <div class="flex justify-between items-center mb-5">
            <h3 class="text-lg font-bold text-slate-900">New Support Ticket</h3>
            <button (click)="closeCreate()" class="text-2xl leading-none text-slate-400 hover:text-slate-600">&times;</button>
          </div>

          <form [formGroup]="createForm" (ngSubmit)="submitCreate()" novalidate class="space-y-4">
            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Subject</label>
              <input
                type="text"
                formControlName="subject"
                placeholder="Briefly describe your issue"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
                [class.border-red-500]="submitted && createForm.controls.subject.invalid"
              />
              @if (submitted && createForm.controls.subject.invalid) {
                <p class="text-red-500 text-xs mt-1">Subject is required</p>
              }
            </div>

            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Category</label>
              <select
                formControlName="category"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
              >
                <option value="Payment">Payment</option>
                <option value="Account">Account</option>
                <option value="KYC">KYC</option>
                <option value="Rewards">Rewards</option>
                <option value="Other">Other</option>
              </select>
            </div>

            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Priority</label>
              <select
                formControlName="priority"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Urgent">Urgent</option>
              </select>
            </div>

            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Description</label>
              <textarea
                formControlName="description"
                rows="4"
                placeholder="Describe your issue in detail..."
                class="w-full resize-none rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
                [class.border-red-500]="submitted && createForm.controls.description.invalid"
              ></textarea>
              @if (submitted && createForm.controls.description.invalid) {
                <p class="text-red-500 text-xs mt-1">Description is required</p>
              }
            </div>

            <div class="flex gap-3 pt-2">
              <button
                type="button"
                (click)="closeCreate()"
                class="flex-1 rounded-lg border border-slate-300 py-3 font-semibold text-slate-700 hover:bg-slate-100"
              >Cancel</button>
              <button
                type="submit"
                [disabled]="createLoading"
                class="flex-1 bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-lg disabled:opacity-60"
              >{{ createLoading ? 'Creating...' : 'Create Ticket' }}</button>
            </div>
          </form>
        </div>
      </div>
    }

    <!-- ── Detail shimmer overlay ── -->
    @if (loading && selectedTicket === null && tickets.length > 0) {
      <div class="fixed inset-0 bg-black/60 z-30 flex items-center justify-center">
        <div class="w-8 h-8 border-4 border-accent border-t-transparent rounded-full animate-spin"></div>
      </div>
    }
  `,
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

  closeCreate(): void {
    this.showCreate = false;
    this.createLoading = false;
    this.submitted = false;
    this.createForm.reset({ subject: '', description: '', category: 'Payment', priority: 'Medium' });
  }

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
