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
    <div class="p-6 max-w-4xl mx-auto space-y-6">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-display font-bold text-zinc-100">Support Tickets</h1>
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
            <div class="bg-zinc-900 border border-zinc-800 rounded-xl shadow p-5 animate-pulse">
              <div class="flex items-start justify-between gap-3">
                <div class="flex-1 space-y-2">
                  <div class="h-4 bg-zinc-700 rounded w-2/3"></div>
                  <div class="h-3 bg-zinc-800 rounded w-1/3"></div>
                </div>
                <div class="h-5 w-20 bg-zinc-700 rounded-full shrink-0"></div>
              </div>
            </div>
          }
        </div>
      }

      @if (!loading) {
        <!-- ── Ticket detail view ── -->
        @if (selectedTicket) {
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl shadow p-6">
            <div class="flex items-center gap-3 mb-5">
              <button
                (click)="selectedTicket = null"
                class="text-zinc-500 hover:text-zinc-300 text-lg font-bold"
              >← Back</button>
              <h2 class="font-semibold text-zinc-100 flex-1">{{ selectedTicket.subject }}</h2>
              <span
                class="px-2 py-0.5 rounded-full text-xs font-semibold shrink-0"
                [class]="statusClass(selectedTicket.status)"
              >{{ selectedTicket.status }}</span>
            </div>

            <!-- Ticket number + metadata -->
            <p class="text-xs text-zinc-500 mb-4">
              #{{ selectedTicket.ticketNumber }} &bull;
              {{ selectedTicket.category }} &bull;
              Priority: {{ selectedTicket.priority }} &bull;
              {{ selectedTicket.createdAt | date:'dd MMM yyyy, HH:mm' }}
            </p>

            <!-- User's original message -->
            <div class="bg-zinc-800 rounded-lg p-4 mb-3">
              <p class="text-xs font-medium text-zinc-400 mb-1">You</p>
              <p class="text-zinc-200 text-sm">{{ selectedTicket.description }}</p>
            </div>

            <!-- Admin reply (if any) -->
            @if (selectedTicket.adminReply) {
              <div class="bg-amber-500/10 rounded-lg p-4 mb-3 border border-amber-500/20">
                <p class="text-xs font-medium text-amber-300 mb-1">
                  Support Agent
                  @if (selectedTicket.respondedAt) {
                    — {{ selectedTicket.respondedAt | date:'dd MMM, HH:mm' }}
                  }
                </p>
                <p class="text-zinc-200 text-sm">{{ selectedTicket.adminReply }}</p>
              </div>
            } @else {
              <p class="text-xs text-zinc-500 italic">Awaiting reply from our support team...</p>
            }
          </div>
        }

        <!-- ── Ticket list ── -->
        @if (!selectedTicket) {
          @if (tickets.length === 0) {
            <!-- Empty state -->
            <div class="text-center py-16 text-zinc-500">
              <div class="text-5xl mb-3">🎫</div>
              <p class="font-medium text-zinc-400">No tickets yet</p>
              <p class="text-sm mt-1">Create one if you need help!</p>
            </div>
          } @else {
            <div class="space-y-3">
              @for (ticket of tickets; track ticket.id) {
                <div
                  class="bg-zinc-900 border border-zinc-800 rounded-xl shadow p-5 cursor-pointer hover:border-zinc-700 transition-colors"
                  (click)="openTicket(ticket)"
                >
                  <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                      <p class="font-medium text-zinc-100 truncate">{{ ticket.subject }}</p>
                      <p class="text-xs text-zinc-500 mt-0.5">
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
                        <span class="text-xs text-zinc-500">1 reply</span>
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
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-xl w-full max-w-md p-6">
          <div class="flex justify-between items-center mb-5">
            <h3 class="text-lg font-bold text-zinc-100">New Support Ticket</h3>
            <button (click)="closeCreate()" class="text-zinc-500 hover:text-zinc-300 text-2xl leading-none">&times;</button>
          </div>

          <form [formGroup]="createForm" (ngSubmit)="submitCreate()" novalidate class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Subject</label>
              <input
                type="text"
                formControlName="subject"
                placeholder="Briefly describe your issue"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent"
                [class.border-red-500]="submitted && createForm.controls.subject.invalid"
              />
              @if (submitted && createForm.controls.subject.invalid) {
                <p class="text-red-500 text-xs mt-1">Subject is required</p>
              }
            </div>

            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Category</label>
              <select
                formControlName="category"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent"
              >
                <option value="Payment">Payment</option>
                <option value="Account">Account</option>
                <option value="KYC">KYC</option>
                <option value="Rewards">Rewards</option>
                <option value="Other">Other</option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Priority</label>
              <select
                formControlName="priority"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent"
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Urgent">Urgent</option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Description</label>
              <textarea
                formControlName="description"
                rows="4"
                placeholder="Describe your issue in detail..."
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent resize-none"
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
                class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-3 rounded-lg hover:bg-zinc-800"
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
      case 'Open':       return 'bg-yellow-500/20 text-yellow-300';
      case 'InProgress': return 'bg-amber-500/20 text-amber-300';
      case 'Responded':  return 'bg-fuchsia-500/20 text-fuchsia-300';
      case 'Resolved':   return 'bg-emerald-500/20 text-emerald-300';
      case 'Closed':     return 'bg-zinc-700 text-zinc-300';
      default:           return 'bg-zinc-700 text-zinc-300';
    }
  }
}
