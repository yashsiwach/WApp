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
    <div class="p-6 max-w-3xl mx-auto space-y-6">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-display font-bold text-zinc-100">Notifications</h1>
      </div>

      <app-loader [show]="loading" />

      @if (!loading) {
        @if (notifications.length === 0) {
          <div class="text-center py-16 text-zinc-500">
            <div class="text-5xl mb-3">Inbox</div>
            <p>No notifications yet</p>
          </div>
        } @else {
          <div class="space-y-3">
            @for (n of notifications; track n.id) {
              <div class="bg-zinc-900/80 border border-zinc-800 rounded-xl shadow p-5 flex flex-col gap-3 transition-colors">
                <div class="min-w-0">
                  <p class="font-medium text-sm text-zinc-100">{{ n.subject }}</p>
                  <div class="flex items-center gap-2 mt-2 flex-wrap">
                    <span class="text-xs px-1.5 py-0.5 bg-zinc-800 text-zinc-300 rounded">{{ n.type }}</span>
                    <span class="text-xs px-1.5 py-0.5 bg-zinc-800 text-zinc-400 rounded">{{ n.channel }}</span>
                    <span
                      class="text-xs px-1.5 py-0.5 rounded"
                      [class.bg-emerald-500/20]="n.status === 'Sent' || n.status === 'Delivered'"
                      [class.text-emerald-300]="n.status === 'Sent' || n.status === 'Delivered'"
                      [class.bg-yellow-500/20]="n.status !== 'Sent' && n.status !== 'Delivered'"
                      [class.text-yellow-300]="n.status !== 'Sent' && n.status !== 'Delivered'"
                    >
                      {{ n.status }}
                    </span>
                  </div>
                  <div class="mt-2 text-zinc-500 text-xs">
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
            <span class="text-zinc-400">Page {{ page }} of {{ totalPages }}</span>
            <div class="flex gap-2">
              <button
                (click)="prevPage()"
                [disabled]="page === 1"
                class="px-3 py-1.5 border border-zinc-700 rounded-lg hover:bg-zinc-800 disabled:opacity-40"
              >Prev</button>
              <button
                (click)="nextPage()"
                [disabled]="page >= totalPages"
                class="px-3 py-1.5 border border-zinc-700 rounded-lg hover:bg-zinc-800 disabled:opacity-40"
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
