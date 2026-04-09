import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import {
  CatalogAdminItemDto,
  CreateCatalogItemRequest,
  KycActionRequest,
  KycReviewDto,
  TicketDetailDto,
  TicketSummaryDto,
} from '../../shared/models/admin.model';
import { UserDto } from '../../shared/models/auth.model';
import { CatalogItemDto } from '../../shared/models/catalog.model';
import { PaginatedResult } from '../../shared/models/paginated-result.model';

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

  getPendingKyc(): Observable<KycReviewDto[]> {
    return this.api
      .get<PaginatedResult<AdminKycApiDto>>('/api/admin/kyc/pending')
      .pipe(map((result) => (result.items ?? []).map((item) => this.mapKycReview(item))));
  }

  approveKyc(reviewId: string, payload: KycActionRequest): Observable<unknown> {
    return this.api.post<unknown>(`/api/admin/kyc/${reviewId}/approve`, {
      notes: payload.adminNote ?? '',
    });
  }

  rejectKyc(reviewId: string, payload: KycActionRequest): Observable<unknown> {
    return this.api.post<unknown>(`/api/admin/kyc/${reviewId}/reject`, {
      reason: payload.adminNote ?? '',
    });
  }

  getTickets(status?: string): Observable<TicketSummaryDto[]> {
    return this.api
      .get<PaginatedResult<TicketSummaryDto>>('/api/support/admin/tickets', {
        ...(status ? { status } : {}),
      })
      .pipe(map((result) => result.items ?? []));
  }

  getTicket(ticketId: string): Observable<TicketDetailDto> {
    return this.api
      .get<SupportTicketApiDto>(`/api/support/admin/tickets/${ticketId}`)
      .pipe(map((ticket) => this.mapTicketDetail(ticket)));
  }

  replyToTicket(ticketId: string, reply: string): Observable<unknown> {
    return this.api.post<unknown>(`/api/support/admin/tickets/${ticketId}/replies`, { message: reply });
  }

  closeTicket(ticketId: string): Observable<unknown> {
    return this.api.patch<unknown>(`/api/support/admin/tickets/${ticketId}/status`, { status: 'Closed' });
  }

  getUsers(): Observable<UserDto[]> {
    return this.api.get<UserDto[]>('/api/auth/admin/users');
  }

  updateUserStatus(userId: string, isActive: boolean): Observable<UserDto> {
    return this.api.patch<UserDto>(`/api/auth/admin/users/${userId}/status`, { isActive });
  }

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
