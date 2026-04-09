export interface CreateTicketRequest {
  subject: string;
  description: string;
  priority: 'Low' | 'Medium' | 'High' | 'Urgent';
  category: 'Payment' | 'Account' | 'KYC' | 'Rewards' | 'Other';
}

export interface SupportTicketDto {
  id: string;
  ticketNumber: string;
  subject: string;
  description: string;
  category: string;
  priority: string;
  status: string;
  adminReply: string | null;
  createdAt: string;
  respondedAt: string | null;
}

