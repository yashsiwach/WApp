import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';

import { NotificationsService } from './notifications.service';
import { LoaderComponent } from '../../shared/components/loader/loader.component';
import { NotificationDto } from '../../shared/models/notification.model';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [DatePipe, LoaderComponent],
  templateUrl: './notifications.component.html',
  
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

  // Load the current page of notifications and keep pagination totals updated.
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

  // Move to the previous notifications page when available.
  prevPage(): void {
    if (this.page > 1) {
      this.page--;
      this.load();
    }
  }

  // Move to the next notifications page when available.
  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.load();
    }
  }
}
