import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, forkJoin, map, of, switchMap, take } from 'rxjs';
import { tap } from 'rxjs/operators';

import { ApiService } from '../../core/services/api.service';
import { PaginatedResult } from '../../shared/models/paginated-result.model';
import {
  PaymentResponse,
  TopUpRequest,
  TopUpResult,
  TransactionHistory,
  TransactionResponse,
  TransferRequest,
  TransferResult,
  WalletResponse,
} from '../../shared/models/wallet.model';

@Injectable({ providedIn: 'root' })
export class WalletService {
  private readonly walletSubject = new BehaviorSubject<WalletResponse | null>(null);
  readonly wallet$ = this.walletSubject.asObservable();

  constructor(private readonly api: ApiService) {}

  // Fetch the latest wallet summary from the backend.
  getWallet(): Observable<WalletResponse> {
    return this.api.get<WalletResponse>('/api/wallet/balance');
  }

  // Refresh the shared wallet stream used by wallet-related screens.
  loadWallet(): void {
    this.getWallet().subscribe((wallet) => this.walletSubject.next(wallet));
  }

  // Start a top-up request and refresh the wallet once the backend records it.
  topUp(payload: TopUpRequest): Observable<TopUpResult> {
    return this.api
      .post<TopUpResult>('/api/wallet/topup', {
        amount: payload.amount,
        provider: 'manual',
        // Use a fresh idempotency key so retries do not accidentally double-charge a top-up.
        idempotencyKey: crypto.randomUUID(),
      })
      .pipe(tap(() => this.loadWallet()));
  }

  // Transfer funds to another user and refresh the cached wallet balance afterward.
  transfer(payload: TransferRequest): Observable<TransferResult> {
    return this.api
      .post<TransferResult>('/api/wallet/transfer', {
        toEmail: payload.toEmail,
        amount: payload.amount,
        description: payload.description ?? '',
        // Idempotency protects against duplicate transfers if the request is retried.
        idempotencyKey: crypto.randomUUID(),
      })
      .pipe(tap(() => this.loadWallet()));
  }

  // Load a single page of transaction history and normalize missing pagination arrays.
  getTransactions(page = 1, pageSize = 10): Observable<TransactionHistory> {
    return this.api
      .get<PaginatedResult<TransactionResponse>>('/api/wallet/transactions', { page, size: pageSize })
      .pipe(
        map((res) => ({
          // Normalize empty responses so the component can always render arrays safely.
          items: res.items ?? [],
          page: res.page,
          pageSize: res.pageSize,
          totalCount: res.totalCount,
          totalPages: res.totalPages,
        })),
      );
  }

  // Reuse transaction history to expose only top-up style payments for the wallet screen.
  getPaymentHistory(page = 1, pageSize = 50): Observable<PaymentResponse[]> {
    return this.getTransactions(page, pageSize).pipe(
      map((res) =>
        res.items
          // The payments panel only reflects wallet top-up activity from the full transaction history.
          .filter((item) => item.referenceType === 'TopUp')
          .map((item) => ({
            id: item.id,
            amount: item.amount,
            type: item.type,
            status: 'Success',
            referenceType: item.referenceType,
            description: item.description,
            createdAt: item.createdAt,
          })),
      ),
    );
  }

  // Export helper: pull transactions in manageable pages to avoid large single-request failures.
  getTransactionsForExport(pageSize = 100, maxPages = 20): Observable<TransactionResponse[]> {
    return this.getTransactions(1, pageSize).pipe(
      switchMap((firstPage) => {
        // Cap export pagination so very large histories do not fan out into too many requests.
        const totalPages = Math.max(1, Math.min(firstPage.totalPages || 1, maxPages));
        if (totalPages === 1) {
          return of(firstPage.items ?? []);
        }

        const remainingPageRequests = Array.from({ length: totalPages - 1 }, (_, idx) =>
          this.getTransactions(idx + 2, pageSize),
        );

        return forkJoin(remainingPageRequests).pipe(
          // Flatten the first page together with the remaining pages into one export-friendly list.
          map((restPages) => [firstPage, ...restPages].flatMap((page) => page.items ?? [])),
        );
      }),
    );
  }

  // Prefer cached wallet data but fall back to API when export starts before wallet stream is populated.
  getWalletForExport(): Observable<WalletResponse> {
    return this.wallet$.pipe(
      take(1),
      // Reuse the in-memory wallet when available to avoid an extra export-time request.
      switchMap((wallet) => (wallet ? of(wallet) : this.getWallet())),
    );
  }
}
