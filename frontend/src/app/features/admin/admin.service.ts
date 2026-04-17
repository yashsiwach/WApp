import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import {
  AdminDashboardStatsDto,
  CatalogAdminItemDto,
  CreateCatalogItemRequest,
  KycActionRequest,
  KycReviewDto,
  NotificationTemplateDto,
  TicketDetailDto,
  TicketSummaryDto,
  UpdateCatalogItemRequest,
  UpdateNotificationTemplateRequest,
  WalletAdminStatsDto,
} from '../../shared/models/admin.model';
import { UserDto } from '../../shared/models/auth.model';
import { CatalogItemDto } from '../../shared/models/catalog.model';
import { PaginatedResult } from '../../shared/models/paginated-result.model';

interface TicketQueryParams {
  status?: string;
  priority?: string;
  category?: string;
  page?: number;
  size?: number;
}

interface AdminKycApiDto {
  id: string;
  documentId: string;
  userId: string;
  docType: string;
  fileUrl: string;
  status: string;
  reviewNotes?: string | null;
  submittedAt: string;
  reviewedAt?: string | null;
}

interface SupportTicketApiDto {
  id: string;
  ticketNumber: string;
  subject: string;
  description: string;
  category: string;
  priority: string;
  status: string;
  internalNote?: string | null;
  replyCount: number;
  createdAt: string;
  updatedAt?: string;
  resolvedAt?: string | null;
  replies?: SupportTicketReplyApiDto[];
}

interface SupportTicketReplyApiDto {
  id: string;
  authorId: string;
  authorRole: string;
  message: string;
  createdAt: string;
}

interface AdminCatalogApiDto {
  id: string;
  name: string;
  description: string;
  pointsCost: number;
  category: string;
  isAvailable: boolean;
  stockQuantity: number;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly api: ApiService) {}

  // Load pending KYC reviews and map them into the admin UI model.
  getPendingKyc(): Observable<KycReviewDto[]> {
    return this.api
      .get<PaginatedResult<AdminKycApiDto>>('/api/admin/kyc/pending')
      .pipe(map((result) => (result.items ?? []).map((item) => this.mapKycReview(item))));
  }

  // Approve one KYC review and forward the optional admin note to the backend.
  approveKyc(reviewId: string, payload: KycActionRequest): Observable<unknown> {
    return this.api.post<unknown>(`/api/admin/kyc/${reviewId}/approve`, {
      notes: payload.adminNote ?? '',
    });
  }

  // Reject one KYC review and forward the admin reason to the backend.
  rejectKyc(reviewId: string, payload: KycActionRequest): Observable<unknown> {
    return this.api.post<unknown>(`/api/admin/kyc/${reviewId}/reject`, {
      reason: payload.adminNote ?? '',
    });
  }

  // Load admin ticket summaries, optionally filtered by status.
  getTickets(status?: string): Observable<TicketSummaryDto[]> {
    return this.api
      .get<PaginatedResult<TicketSummaryDto>>('/api/support/admin/tickets', {
        ...(status ? { status } : {}),
      })
      .pipe(map((result) => result.items ?? []));
  }

  // Load a single ticket with reply details for the admin detail drawer.
  getTicket(ticketId: string): Observable<TicketDetailDto> {
    return this.api
      .get<SupportTicketApiDto>(`/api/support/admin/tickets/${ticketId}`)
      .pipe(map((ticket) => this.mapTicketDetail(ticket)));
  }

  // Post an admin reply to the selected support ticket.
  replyToTicket(ticketId: string, reply: string): Observable<unknown> {
    return this.api.post<unknown>(`/api/support/admin/tickets/${ticketId}/replies`, { message: reply });
  }

  // Move a support ticket into the closed state.
  closeTicket(ticketId: string): Observable<unknown> {
    return this.api.patch<unknown>(`/api/support/admin/tickets/${ticketId}/status`, { status: 'Closed' });
  }

  // Load all users for the admin user management screen.
  getUsers(): Observable<UserDto[]> {
    return this.api.get<UserDto[]>('/api/auth/admin/users');
  }

  // Toggle a user's active state from the admin users screen.
  updateUserStatus(userId: string, isActive: boolean): Observable<UserDto> {
    return this.api.patch<UserDto>(`/api/auth/admin/users/${userId}/status`, { isActive });
  }

  // Reuse the public catalog endpoint and remap its fields for admin editing.
  getCatalogItems(): Observable<CatalogAdminItemDto[]> {
    return this.api
      .get<CatalogItemDto[]>('/api/rewards/catalog')
      .pipe(
        map((items) =>
          (items ?? []).map((item) => ({
            id: item.id,
            name: item.name,
            description: item.description,
            pointsCost: item.pointsCost,
            category: item.category,
            isActive: item.isAvailable,
          })),
        ),
      );
  }

  // Create a new catalog item using the admin catalog payload shape.
  createCatalogItem(payload: CreateCatalogItemRequest): Observable<CatalogAdminItemDto> {
    return this.api
      .post<AdminCatalogApiDto>('/api/admin/rewards/catalog', {
        name: payload.name,
        description: payload.description,
        pointsCost: payload.pointsCost,
        category: payload.category,
        isAvailable: true,
        stockQuantity: -1,
      })
      .pipe(
        map((item) => ({
          id: item.id,
          name: item.name,
          description: item.description,
          pointsCost: item.pointsCost,
          category: item.category,
          isActive: item.isAvailable,
        })),
      );
  }

  // Persist admin edits to an existing catalog item and map the response back into the table model.
  updateCatalogItem(itemId: string, payload: UpdateCatalogItemRequest): Observable<CatalogAdminItemDto> {
    return this.api
      .put<AdminCatalogApiDto>(`/api/admin/rewards/catalog/${itemId}`, {
        name: payload.name,
        description: payload.description,
        pointsCost: payload.pointsCost,
        category: payload.category,
        isAvailable: payload.isActive,
      })
      .pipe(
        map((item) => ({
          id: item.id,
          name: item.name,
          description: item.description,
          pointsCost: item.pointsCost,
          category: item.category,
          isActive: item.isAvailable,
        })),
      );
  }

  // Delete one catalog item from the admin catalog table.
  deleteCatalogItem(itemId: string): Observable<null> {
    return this.api.delete<null>(`/api/admin/rewards/catalog/${itemId}`);
  }

  // Load the high-level admin dashboard metrics cards.
  getDashboardStats(): Observable<AdminDashboardStatsDto> {
    return this.api.get<AdminDashboardStatsDto>('/api/admin/dashboard');
  }

  // Load paginated support tickets with the current admin filters applied.
  getTicketsPaginated(params: TicketQueryParams): Observable<PaginatedResult<TicketSummaryDto>> {
    return this.api.get<PaginatedResult<TicketSummaryDto>>('/api/support/admin/tickets', {
      ...(params.status   ? { status:   params.status }   : {}),
      ...(params.priority ? { priority: params.priority } : {}),
      ...(params.category ? { category: params.category } : {}),
      page: params.page ?? 1,
      size: params.size ?? 20,
    });
  }

  // Load wallet usage and balance metrics for the admin overview.
  getWalletStats(): Observable<WalletAdminStatsDto> {
    return this.api.get<WalletAdminStatsDto>('/api/admin/wallet/stats');
  }

  // Load editable notification templates for the admin notifications page.
  getNotificationTemplates(): Observable<NotificationTemplateDto[]> {
    return this.api.get<NotificationTemplateDto[]>('/api/notifications/templates');
  }

  // Persist edits made to one notification template.
  updateNotificationTemplate(
    id: string,
    payload: UpdateNotificationTemplateRequest,
  ): Observable<NotificationTemplateDto> {
    return this.api.put<NotificationTemplateDto>(`/api/notifications/templates/${id}`, payload);
  }

  // Adapt the KYC API shape into the fields expected by the review table.
  private mapKycReview(item: AdminKycApiDto): KycReviewDto {
    return {
      id: item.id,
      userId: item.userId,
      userFullName: `User ${item.userId.slice(0, 8)}`,
      userEmail: item.fileUrl,
      documentType: item.docType,
      documentNumber: item.documentId,
      status: item.status,
      adminNote: item.reviewNotes ?? null,
      submittedAt: item.submittedAt,
      reviewedAt: item.reviewedAt ?? null,
    };
  }

  // Extract the latest admin response and normalize ticket detail data for the UI.
  private mapTicketDetail(ticket: SupportTicketApiDto): TicketDetailDto {
    const adminReplies = (ticket.replies ?? []).filter((reply) => reply.authorRole === 'Admin');
    const latestAdminReply = adminReplies.at(-1) ?? null;

    return {
      id: ticket.id,
      ticketNumber: ticket.ticketNumber,
      subject: ticket.subject,
      description: ticket.description,
      category: ticket.category,
      priority: ticket.priority,
      status: ticket.status,
      userEmail: '',
      adminReply: latestAdminReply?.message ?? ticket.internalNote ?? null,
      replyCount: ticket.replyCount,
      createdAt: ticket.createdAt,
      respondedAt: latestAdminReply?.createdAt ?? null,
      closedAt: ticket.resolvedAt ?? null,
    };
  }

}
