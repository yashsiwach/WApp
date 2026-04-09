export interface KycSubmitRequest {
  docType: string;
  fileUrl: string;
}

export interface KycStatusResponse {
  documentId: string;
  docType: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  reviewNotes?: string | null;
  submittedAt: string;
}
