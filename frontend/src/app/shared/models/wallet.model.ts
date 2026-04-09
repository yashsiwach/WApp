export interface WalletResponse {
  walletId: string;
  balance: number;
  currency: string;
  isLocked: boolean;
  kycVerified: boolean;
}

export interface TransactionResponse {
  id: string;
  type: string;
  amount: number;
  referenceId: string;
  referenceType: string;
  status: string;
  description: string;
  createdAt: string;
}

export interface TransactionHistory {
  items: TransactionResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PaymentResponse {
  id: string;
  amount: number;
  type: string;
  status: string;
  referenceType: string;
  description: string;
  createdAt: string;
}

export interface TopUpRequest {
  amount: number;
}

export interface TopUpResult {
  transactionId: string;
  amount: number;
  newBalance: number;
  status: string;
}

export interface TransferRequest {
  toEmail: string;
  amount: number;
  description?: string;
}

export interface TransferResult {
  transactionId: string;
  amount: number;
  newBalance: number;
  status: string;
}
