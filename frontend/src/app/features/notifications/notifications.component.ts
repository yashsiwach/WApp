import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';

import { NotificationsService } from './notifications.service';
import { LoaderComponent } from '../../shared/components/loader/loader.component';
import { NotificationDto } from '../../shared/models/notification.model';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [DatePipe, LoaderComponent],
  template: `
    <div class="mx-auto max-w-3xl space-y-6 p-6 text-slate-900">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-display font-bold text-slate-900">Notifications</h1>
      </div>

      <app-loader [show]="loading" />

      @if (!loading) {
        @if (notifications.length === 0) {
          <div class="py-16 text-center text-slate-500">
            <div class="text-5xl mb-3">Inbox</div>
            <p>No notifications yet</p>
          </div>
        } @else {
          <div class="space-y-3">
            @for (n of notifications; track n.id) {
              <div class="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white/95 p-5 shadow-sm transition-colors hover:border-slate-300">
                <div class="min-w-0">
                  <p class="text-sm font-medium text-slate-900">{{ n.subject }}</p>
                  <div class="flex items-center gap-2 mt-2 flex-wrap">
                    <span class="rounded bg-slate-100 px-1.5 py-0.5 text-xs text-slate-700">{{ n.type }}</span>
                    <span class="rounded bg-slate-100 px-1.5 py-0.5 text-xs text-slate-500">{{ n.channel }}</span>
                    <span
                      class="text-xs px-1.5 py-0.5 rounded"
                      [class.bg-emerald-500/20]="n.status === 'Sent' || n.status === 'Delivered'"
                      [class.text-emerald-700]="n.status === 'Sent' || n.status === 'Delivered'"
                      [class.bg-blue-500/20]="n.status !== 'Sent' && n.status !== 'Delivered'"
                      [class.text-blue-700]="n.status !== 'Sent' && n.status !== 'Delivered'"
                    >
                      {{ n.status }}
                    </span>
                  </div>
                  <div class="mt-2 text-xs text-slate-500">
                    <div>Created: {{ n.createdAt | date:'dd MMM, HH:mm' }}</div>
                    @if (n.sentAt) {
                      <div>Sent: {{ n.sentAt | date:'dd MMM, HH:mm' }}</div>
                    }
                  </div>
                </div>
              </div>
            }
          </div>

          <div class="flex items-center justify-between text-sm">
            <span class="text-slate-500">Page {{ page }} of {{ totalPages }}</span>
            <div class="flex gap-2">
              <button
                (click)="prevPage()"
                [disabled]="page === 1"
                class="rounded-lg border border-slate-300 px-3 py-1.5 hover:bg-slate-100 disabled:opacity-40"
              >Prev</button>
              <button
                (click)="nextPage()"
                [disabled]="page >= totalPages"
                class="rounded-lg border border-slate-300 px-3 py-1.5 hover:bg-slate-100 disabled:opacity-40"
              >Next</button>
            </div>
          </div>
        }
      }
    </div>
  `,
})
export class NotificationsComponent implements OnInit {
  loading = false;
  notifications: NotificationDto[] = [];
  page = 1;
  totalPages = 1;

  constructor(private readonly notificationsService: NotificationsService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.notificationsService.getNotifications(this.page).subscribe({
      next: (res) => {
        this.notifications = res.items ?? [];
        this.totalPages = res.totalPages || 1;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page--;
      this.load();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.load();
    }
  }
}
