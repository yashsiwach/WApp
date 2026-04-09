import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import { NotificationListResponse } from '../../shared/models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  constructor(private readonly api: ApiService) {}

  getNotifications(page = 1, pageSize = 20): Observable<NotificationListResponse> {
    return this.api.get<NotificationListResponse>('/api/notifications', { page, size: pageSize });
  }
}
