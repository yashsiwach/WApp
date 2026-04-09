export interface NotificationDto {
  id: string;
  channel: string;
  type: string;
  subject: string;
  status: string;
  createdAt: string;
  sentAt: string | null;
}

export interface NotificationListResponse {
  items: NotificationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
