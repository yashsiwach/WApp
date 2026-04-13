export interface KycReviewDto {
  id: string;
  userId: string;
  userFullName: string;
  userEmail: string;
  documentType: string;
  documentNumber: string;
  status: string;
  adminNote: string | null;
  submittedAt: string;
  reviewedAt: string | null;
}

export interface KycActionRequest {
  adminNote?: string;
}

export interface TicketSummaryDto {
  id: string;
  ticketNumber: string;
  subject: string;
  status: string;
  priority: string;
  category: string;
  replyCount: number;
  createdAt: string;
}

export interface TicketDetailDto {
  id: string;
  ticketNumber: string;
  subject: string;
  description: string;
  category: string;
  priority: string;
  status: string;
  userEmail: string;
  adminReply: string | null;
  replyCount: number;
  createdAt: string;
  respondedAt: string | null;
  closedAt: string | null;
}

export interface TicketReplyRequest {
  reply: string;
}

export interface CatalogAdminItemDto {
  id: string;
  name: string;
  description: string;
  pointsCost: number;
  category: string;
  isActive: boolean;
  createdAt?: string;
}

export interface CreateCatalogItemRequest {
  name: string;
  description: string;
  pointsCost: number;
  category: string;
}

export interface UpdateCatalogItemRequest {
  name: string;
  description: string;
  pointsCost: number;
  category: string;
  isActive: boolean;
}

export interface AdminDashboardStatsDto {
  pendingKYCCount: number;
  approvedKYCToday: number;
  rejectedKYCToday: number;
  adminActionsToday: number;
}

export interface NotificationTemplateDto {
  id: string;
  type: string;
  channel: string;
  subject: string;
  bodyTemplate: string;
  isActive: boolean;
}

export interface UpdateNotificationTemplateRequest {
  subject: string;
  bodyTemplate: string;
  isActive: boolean;
}

export interface WalletAdminStatsDto {
  totalTransactionCount: number;
  totalVolume: number;
  todaysVolume: number;
  todaysTransactionCount: number;
  failedTransactions: number;
  averageTransactionValue: number;
}
