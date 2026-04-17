import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import { CatalogItemDto } from '../../shared/models/catalog.model';

export interface RedeemResponseDto {
  redemptionId: string;
  pointsSpent: number;
  remainingBalance: number;
  status: string;
  fulfillmentCode?: string | null;
}

@Injectable({ providedIn: 'root' })
export class CatalogService {
  constructor(private readonly api: ApiService) {}

  // Load the available rewards catalog for the redemption screen.
  getCatalog(): Observable<CatalogItemDto[]> {
    return this.api.get<CatalogItemDto[]>('/api/rewards/catalog');
  }

  // Redeem a selected catalog item and return the fulfillment details.
  redeem(catalogItemId: string): Observable<RedeemResponseDto> {
    return this.api.post<RedeemResponseDto>('/api/rewards/redeem', {
      catalogItemId,
    });
  }
}
