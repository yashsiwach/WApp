export interface RewardResponse {
  id: string;
  userId?: string;
  pointsBalance: number;
  totalEarned?: number;
  lifetimePoints?: number;
  tier: string;
  nextTier?: string;
  pointsToNext?: number;
  createdAt: string;
}

export interface RewardTransactionResponse {
  id: string;
  userId?: string;
  points: number;
  reason: string;
  reference: string;
  createdAt: string;
}

export interface RewardTransactionApiResponse {
  id: string;
  userId?: string;
  points?: number;
  pointsDelta?: number;
  reason?: string;
  description?: string;
  reference?: string;
  referenceType?: string;
  createdAt: string;
}

