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
