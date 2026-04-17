import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import { PaginatedResult } from '../../shared/models/paginated-result.model';
import {
  RewardResponse,
  RewardTransactionApiResponse,
  RewardTransactionResponse,
} from '../../shared/models/rewards.model';

@Injectable({ providedIn: 'root' })
export class RewardsService {
  constructor(private readonly api: ApiService) {}

  // Load the rewards account and backfill missing lifetime totals used by the UI.
  getRewards(): Observable<RewardResponse> {
    return this.api.get<RewardResponse>('/api/rewards/account').pipe(
      map((account) => ({
        ...account,
        totalEarned: account.totalEarned ?? account.lifetimePoints ?? 0,
      })),
    );
  }

  // Load reward activity and normalize field names coming from older API payloads.
  getHistory(): Observable<RewardTransactionResponse[]> {
    return this.api
      .get<PaginatedResult<RewardTransactionApiResponse>>('/api/rewards/transactions')
      .pipe(
        map((result) =>
          (result.items ?? []).map((tx) => ({
            id: tx.id,
            userId: tx.userId,
            points: tx.points ?? tx.pointsDelta ?? 0,
            reason: tx.reason ?? tx.description ?? '',
            reference: tx.reference ?? tx.referenceType ?? '',
            createdAt: tx.createdAt,
          })),
        ),
      );
  }

}
