import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import { PaginatedResult } from '../../shared/models/paginated-result.model';
import { CreateTicketRequest, SupportTicketDto } from '../../shared/models/support.model';

interface TicketReplyApiDto {
  id: string;
  authorId: string;
  authorRole: string;
  message: string;
  createdAt: string;
}

interface TicketApiDto {
  id: string;
  ticketNumber: string;
  subject: string;
  description: string;
  category: string;
  priority: string;
  status: string;
  createdAt: string;
  replies?: TicketReplyApiDto[];
}

interface TicketSummaryApiDto {
  id: string;
  ticketNumber: string;
  subject: string;
  status: string;
  priority: string;
  category: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class SupportService {
  constructor(private readonly api: ApiService) {}

  // Load a lightweight ticket list for the main support inbox view.
  getTickets(): Observable<SupportTicketDto[]> {
    return this.api
      .get<PaginatedResult<TicketSummaryApiDto>>('/api/support/tickets', { page: 1, size: 50 })
      .pipe(map((res) => (res.items ?? []).map((t) => this.mapSummaryToDto(t))));
  }

  // Create a new support ticket and map the API response into the UI model.
  createTicket(payload: CreateTicketRequest): Observable<SupportTicketDto> {
    return this.api.post<TicketApiDto>('/api/support/tickets', payload).pipe(map((t) => this.mapTicketToDto(t)));
  }

  // Load one ticket with its reply history for the detail panel.
  getTicket(id: string): Observable<SupportTicketDto> {
    return this.api.get<TicketApiDto>(`/api/support/tickets/${id}`).pipe(map((t) => this.mapTicketToDto(t)));
  }

  // Convert summary rows into the ticket shape expected by the support screen.
  private mapSummaryToDto(t: TicketSummaryApiDto): SupportTicketDto {
    return {
      id: t.id,
      ticketNumber: t.ticketNumber,
      subject: t.subject,
      description: '',
      category: t.category,
      priority: t.priority,
      status: t.status,
      adminReply: null,
      createdAt: t.createdAt,
      respondedAt: null,
    };
  }

  // Sort replies newest-first and surface the latest non-user reply as the admin response.
  private mapTicketToDto(t: TicketApiDto): SupportTicketDto {
    const replies = [...(t.replies ?? [])].sort((a, b) =>
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );
    const adminReply = replies.find((r) => r.authorRole.toLowerCase() !== 'user') ?? null;

    return {
      id: t.id,
      ticketNumber: t.ticketNumber,
      subject: t.subject,
      description: t.description,
      category: t.category,
      priority: t.priority,
      status: t.status,
      adminReply: adminReply?.message ?? null,
      createdAt: t.createdAt,
      respondedAt: adminReply?.createdAt ?? null,
    };
  }
}
