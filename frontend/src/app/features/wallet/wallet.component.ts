import { Component, OnInit } from '@angular/core';
import { AsyncPipe, DatePipe, SlicePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { WalletService } from './wallet.service';
import { TransactionPdfService } from './transaction-pdf.service';
import { TransactionCsvService } from './transaction-csv.service';
import { ToastService } from '../../shared/services/toast.service';
import { LoaderComponent } from '../../shared/components/loader/loader.component';
import { InrCurrencyPipe } from '../../shared/pipes/inr-currency.pipe';
import {
  PaymentResponse,
  TopUpResult,
  TransactionResponse,
  TransferResult,
  WalletResponse,
} from '../../shared/models/wallet.model';

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [AsyncPipe, DatePipe, SlicePipe, ReactiveFormsModule, LoaderComponent, InrCurrencyPipe],
  template: `
    <div class="mx-auto max-w-5xl space-y-6 p-6 text-slate-900">
      <h1 class="text-2xl font-display font-bold text-slate-900">My Wallet</h1>

      <div class="rounded-xl border border-slate-200 bg-gradient-to-br from-white via-white to-teal-50/60 p-6 text-slate-900 shadow-sm">
        @if (wallet$ | async; as wallet) {
          <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <p class="mb-1 text-sm text-slate-500">Available Balance</p>
              <p class="text-4xl font-bold">{{ wallet.balance | inrCurrency }}</p>
              <p class="mt-1 text-sm text-slate-500">{{ wallet.currency }} | Wallet ID: {{ wallet.walletId | slice:0:8 }}...</p>
            </div>
            <div class="flex gap-2 flex-wrap">
              @if (wallet.isLocked) {
                <span class="rounded-full bg-blue-100 px-3 py-1 text-xs font-semibold text-blue-700">Locked</span>
              } @else {
                <span class="rounded-full bg-emerald-100 px-3 py-1 text-xs font-semibold text-emerald-700">Active</span>
              }
              @if (wallet.kycVerified) {
                <span class="rounded-full bg-teal-100 px-3 py-1 text-xs font-semibold text-teal-700">KYC Verified</span>
              }
            </div>
          </div>

          <div class="flex gap-3 mt-6">
            <button
              (click)="openTopUp(wallet)"
              [disabled]="wallet.isLocked"
              class="flex-1 bg-accent hover:bg-accent-hover text-white font-semibold py-2.5 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Top Up
            </button>
            <button
              (click)="showTransfer = true"
              [disabled]="wallet.isLocked"
              class="flex-1 rounded-lg border border-slate-300 bg-slate-100 py-2.5 font-semibold text-slate-700 transition-colors hover:bg-slate-200 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Transfer
            </button>
          </div>
        } @else {
          <div class="animate-pulse space-y-3">
            <div class="h-4 bg-zinc-700 rounded w-1/3"></div>
            <div class="h-10 bg-zinc-700 rounded w-1/2"></div>
          </div>
        }
      </div>

      <div class="rounded-xl border border-slate-200 bg-white/95 p-6 shadow-sm">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-semibold text-slate-900">Transaction History</h2>
          <div class="flex items-center gap-2">
            <button
              (click)="downloadCsv()"
              [disabled]="csvLoading"
              class="flex items-center gap-2 rounded-lg border border-slate-300 bg-slate-100 px-3 py-1.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-200 hover:text-slate-900 disabled:cursor-not-allowed disabled:opacity-50"
            >
              @if (csvLoading) {
                <span class="w-3.5 h-3.5 border-2 border-zinc-500 border-t-zinc-200 rounded-full animate-spin"></span>
                <span>Exporting...</span>
              } @else {
                <span>Download CSV</span>
              }
            </button>
            <button
              (click)="downloadPdf()"
              [disabled]="pdfLoading"
              class="flex items-center gap-2 rounded-lg border border-slate-300 bg-slate-100 px-3 py-1.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-200 hover:text-slate-900 disabled:cursor-not-allowed disabled:opacity-50"
            >
              @if (pdfLoading) {
                <span class="w-3.5 h-3.5 border-2 border-zinc-500 border-t-zinc-200 rounded-full animate-spin"></span>
                <span>Generating...</span>
              } @else {
                <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v2a2 2 0 002 2h12a2 2 0 002-2v-2M7 10l5 5 5-5M12 15V3" />
                </svg>
                <span>Download PDF</span>
              }
            </button>
          </div>
        </div>

        <app-loader [show]="loadingTx" />

        @if (!loadingTx) {
          @if (transactions.length === 0) {
            <div class="py-12 text-center text-slate-500">
              <div class="text-5xl mb-3">Ledger</div>
              <p>No transactions yet</p>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-slate-200 text-left text-slate-500">
                    <th class="pb-3 font-medium">Date</th>
                    <th class="pb-3 font-medium">Type</th>
                    <th class="pb-3 font-medium">Amount</th>
                    <th class="pb-3 font-medium">Reference</th>
                    <th class="pb-3 font-medium">Description</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100">
                  @for (tx of transactions; track tx.id) {
                    <tr
                      (click)="openTransactionDetails(tx)"
                      class="cursor-pointer transition-colors hover:bg-slate-100/80"
                    >
                      <td class="py-3 text-xs text-slate-500">{{ tx.createdAt | date:'dd MMM, HH:mm' }}</td>
                      <td class="py-3">
                        <span
                          class="px-2 py-0.5 rounded-full text-xs font-semibold"
                          [class.bg-emerald-500/20]="tx.type === 'CREDIT'"
                          [class.text-emerald-300]="tx.type === 'CREDIT'"
                          [class.bg-rose-500/20]="tx.type === 'DEBIT'"
                          [class.text-rose-300]="tx.type === 'DEBIT'"
                          [class.bg-sky-500/20]="tx.type !== 'CREDIT' && tx.type !== 'DEBIT'"
                          [class.text-sky-300]="tx.type !== 'CREDIT' && tx.type !== 'DEBIT'"
                        >{{ tx.referenceType }}</span>
                      </td>
                      <td class="py-3 font-semibold">{{ tx.amount | inrCurrency }}</td>
                      <td class="py-3 text-xs text-slate-500">{{ tx.referenceType }}</td>
                      <td class="max-w-48 truncate py-3 text-xs text-slate-700">{{ tx.description || '-' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            <div class="flex items-center justify-between mt-4 text-sm">
              <span class="text-slate-500">Page {{ txPage }} of {{ txTotalPages }}</span>
              <div class="flex gap-2">
                <button
                  (click)="prevPage()"
                  [disabled]="txPage === 1"
                  class="rounded-lg border border-slate-300 px-3 py-1.5 hover:bg-slate-100 disabled:opacity-40"
                >Prev</button>
                <button
                  (click)="nextPage()"
                  [disabled]="txPage >= txTotalPages"
                  class="rounded-lg border border-slate-300 px-3 py-1.5 hover:bg-slate-100 disabled:opacity-40"
                >Next</button>
              </div>
            </div>
          }
        }
      </div>

      <div class="rounded-xl border border-slate-200 bg-white/95 p-6 shadow-sm">
        <h2 class="mb-4 text-lg font-semibold text-slate-900">Top-Up Payment History</h2>
        <app-loader [show]="loadingPayments" />

        @if (!loadingPayments) {
          @if (payments.length === 0) {
            <div class="py-8 text-center text-slate-500">
              <div class="text-4xl mb-2">Top Ups</div>
              <p>No top-up records yet</p>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-slate-200 text-left text-slate-500">
                    <th class="pb-3 font-medium">Date</th>
                    <th class="pb-3 font-medium">Amount</th>
                    <th class="pb-3 font-medium">Status</th>
                    <th class="pb-3 font-medium">Reference Type</th>
                    <th class="pb-3 font-medium">Description</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100">
                  @for (p of payments; track p.id) {
                    <tr class="hover:bg-slate-100/80">
                      <td class="py-3 text-xs text-slate-500">{{ p.createdAt | date:'dd MMM, HH:mm' }}</td>
                      <td class="py-3 font-semibold text-green-400">{{ p.amount | inrCurrency }}</td>
                      <td class="py-3"><span class="px-2 py-0.5 rounded-full text-xs text-emerald-300">{{ p.status }}</span></td>
                      <td class="py-3 text-xs font-mono text-slate-500">{{ p.referenceType }}</td>
                      <td class="py-3 text-xs text-slate-700">{{ p.description || '-' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
      </div>
    </div>

    @if (selectedTransaction) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="w-full max-w-lg rounded-2xl border border-slate-200 bg-white p-6 shadow-xl">
          <div class="flex justify-between items-center mb-5">
            <div>
              <h3 class="text-lg font-bold text-slate-900">Transaction Details</h3>
              <p class="text-sm text-slate-500">Full record for the selected wallet entry.</p>
            </div>
            <button (click)="closeTransactionDetails()" class="text-2xl leading-none text-slate-400 hover:text-slate-600">&times;</button>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-sm">
            <div class="rounded-xl border border-slate-200 bg-slate-50 p-4 sm:col-span-2">
              <p class="mb-1 text-xs text-slate-500">Transaction ID</p>
              <p class="font-mono break-all text-slate-900">{{ selectedTransaction.id }}</p>
            </div>

            <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <p class="mb-1 text-xs text-slate-500">Date & Time</p>
              <p class="text-slate-900">{{ selectedTransaction.createdAt | date:'dd MMM yyyy, HH:mm:ss' }}</p>
            </div>

            <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <p class="mb-1 text-xs text-slate-500">Status</p>
              <span
                class="inline-flex px-2.5 py-1 rounded-full text-xs font-semibold"
                [class.bg-emerald-500/20]="selectedTransaction.status === 'Completed' || selectedTransaction.status === 'Success'"
                [class.text-emerald-700]="selectedTransaction.status === 'Completed' || selectedTransaction.status === 'Success'"
                [class.bg-blue-500/20]="selectedTransaction.status === 'Pending'"
                [class.text-blue-700]="selectedTransaction.status === 'Pending'"
                [class.bg-rose-500/20]="selectedTransaction.status === 'Failed'"
                [class.text-rose-700]="selectedTransaction.status === 'Failed'"
              >{{ selectedTransaction.status }}</span>
            </div>

            <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <p class="mb-1 text-xs text-slate-500">Type</p>
              <p class="text-slate-900">{{ selectedTransaction.type }}</p>
            </div>

            <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <p class="mb-1 text-xs text-slate-500">Amount</p>
              <p class="font-semibold text-slate-900">{{ selectedTransaction.amount | inrCurrency }}</p>
            </div>

            <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <p class="mb-1 text-xs text-slate-500">Reference Type</p>
              <p class="text-slate-900">{{ selectedTransaction.referenceType }}</p>
            </div>

            <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <p class="mb-1 text-xs text-slate-500">Reference ID</p>
              <p class="font-mono break-all text-slate-900">{{ selectedTransaction.referenceId }}</p>
            </div>

            <div class="rounded-xl border border-slate-200 bg-slate-50 p-4 sm:col-span-2">
              <p class="mb-1 text-xs text-slate-500">Description</p>
              <p class="text-slate-900">{{ selectedTransaction.description || '-' }}</p>
            </div>
          </div>

          <div class="flex justify-end pt-5">
            <button
              type="button"
              (click)="closeTransactionDetails()"
              class="rounded-lg border border-slate-300 px-4 py-2 font-semibold text-slate-700 hover:bg-slate-100"
            >Close</button>
          </div>
        </div>
      </div>
    }

    @if (showTopUp) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-xl">
          <div class="flex justify-between items-center mb-5">
            <h3 class="text-lg font-bold text-slate-900">Top Up Wallet</h3>
            <button (click)="showTopUp = false" class="text-2xl leading-none text-slate-400 hover:text-slate-600">&times;</button>
          </div>

          <form [formGroup]="topUpForm" (ngSubmit)="submitTopUp()" novalidate class="space-y-4">
            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Amount (INR)</label>
              <input
                type="number"
                formControlName="amount"
                min="1"
                max="100000"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
                [class.border-red-500]="topUpSubmitted && topUpForm.controls.amount.invalid"
              />
              @if (topUpSubmitted && topUpForm.controls.amount.invalid) {
                <p class="text-red-500 text-xs mt-1">Enter a valid amount (1 to 100000)</p>
              }
            </div>
            <div class="flex gap-3 pt-2">
              <button
                type="button"
                (click)="showTopUp = false"
                class="flex-1 rounded-lg border border-slate-300 py-3 font-semibold text-slate-700 hover:bg-slate-100"
              >Cancel</button>
              <button
                type="submit"
                [disabled]="topUpLoading"
                class="flex-1 bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-lg disabled:opacity-60"
              >{{ topUpLoading ? 'Processing...' : 'Top Up' }}</button>
            </div>
          </form>
        </div>
      </div>
    }

    @if (showTransfer) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-xl">
          <div class="flex justify-between items-center mb-5">
            <h3 class="text-lg font-bold text-slate-900">Transfer Money</h3>
            <button (click)="showTransfer = false" class="text-2xl leading-none text-slate-400 hover:text-slate-600">&times;</button>
          </div>

          <form [formGroup]="transferForm" (ngSubmit)="submitTransfer()" novalidate class="space-y-4">
            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Recipient Email</label>
              <input
                type="email"
                formControlName="toEmail"
                placeholder="recipient@example.com"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
                [class.border-red-500]="txSubmitted && transferForm.controls.toEmail.invalid"
              />
              @if (txSubmitted && transferForm.controls.toEmail.errors?.['required']) {
                <p class="text-red-500 text-xs mt-1">Recipient email is required</p>
              }
              @if (txSubmitted && transferForm.controls.toEmail.errors?.['email']) {
                <p class="text-red-500 text-xs mt-1">Enter a valid email address</p>
              }
            </div>
            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Amount (INR)</label>
              <input
                type="number"
                formControlName="amount"
                min="1"
                max="100000"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
                [class.border-red-500]="txSubmitted && transferForm.controls.amount.invalid"
              />
              @if (txSubmitted && transferForm.controls.amount.invalid) {
                <p class="text-red-500 text-xs mt-1">Enter a valid amount (1 to 100000)</p>
              }
            </div>
            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Description (optional)</label>
              <input
                type="text"
                formControlName="note"
                placeholder="e.g. Lunch split"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
              />
            </div>
            <div class="flex gap-3 pt-2">
              <button
                type="button"
                (click)="showTransfer = false"
                class="flex-1 rounded-lg border border-slate-300 py-3 font-semibold text-slate-700 hover:bg-slate-100"
              >Cancel</button>
              <button
                type="submit"
                [disabled]="txLoading"
                class="flex-1 rounded-lg bg-accent py-3 font-semibold text-white hover:bg-accent-hover disabled:opacity-60"
              >{{ txLoading ? 'Sending...' : 'Send Money' }}</button>
            </div>
          </form>
        </div>
      </div>
    }
  `,
})
export class WalletComponent implements OnInit {
  readonly wallet$;

  showTopUp = false;
  topUpLoading = false;
  topUpSubmitted = false;
  readonly topUpForm;
  private currentWalletId = '';

  showTransfer = false;
  txLoading = false;
  txSubmitted = false;
  readonly transferForm;

  loadingTx = false;
  transactions: TransactionResponse[] = [];
  selectedTransaction: TransactionResponse | null = null;
  txPage = 1;
  txTotalPages = 1;

  loadingPayments = false;
  payments: PaymentResponse[] = [];
  pdfLoading = false;
  csvLoading = false;

  constructor(
    fb: FormBuilder,
    private readonly walletService: WalletService,
    private readonly pdfService: TransactionPdfService,
    private readonly csvService: TransactionCsvService,
    private readonly toastService: ToastService,
  ) {
    this.wallet$ = this.walletService.wallet$;

    this.topUpForm = fb.nonNullable.group({
      amount: [100, [Validators.required, Validators.min(1), Validators.max(100000)]],
    });

    this.transferForm = fb.nonNullable.group({
      toEmail: ['', [Validators.required, Validators.email]],
      amount: [1, [Validators.required, Validators.min(1), Validators.max(100000)]],
      note: [''],
    });
  }

  ngOnInit(): void {
    this.walletService.loadWallet();
    this.loadTransactions();
    this.loadPayments();
  }

  openTopUp(wallet: WalletResponse): void {
    this.currentWalletId = wallet.walletId;
    this.showTopUp = true;
  }

  openTransactionDetails(tx: TransactionResponse): void {
    this.selectedTransaction = tx;
  }

  closeTransactionDetails(): void {
    this.selectedTransaction = null;
  }

  loadTransactions(): void {
    this.loadingTx = true;
    this.walletService.getTransactions(this.txPage).subscribe({
      next: (res) => {
        this.transactions = res.items;
        this.txTotalPages = res.totalPages || 1;
        this.loadingTx = false;
      },
      error: () => (this.loadingTx = false),
    });
  }

  loadPayments(): void {
    this.loadingPayments = true;
    this.walletService.getPaymentHistory().subscribe({
      next: (res) => {
        this.payments = res;
        this.loadingPayments = false;
      },
      error: () => (this.loadingPayments = false),
    });
  }

  prevPage(): void {
    if (this.txPage > 1) {
      this.txPage--;
      this.loadTransactions();
    }
  }

  nextPage(): void {
    if (this.txPage < this.txTotalPages) {
      this.txPage++;
      this.loadTransactions();
    }
  }

  downloadPdf(): void {
    this.pdfLoading = true;
    forkJoin([
      this.walletService.getWalletForExport(),
      this.walletService.getTransactionsForExport(),
    ]).subscribe({
      next: ([wallet, res]) => {
        this.pdfService.download(wallet, res);
        this.pdfLoading = false;
      },
      error: (err: { message?: string; status?: number }) => {
        this.pdfLoading = false;
        if (err.status === 0) {
          this.toastService.error('Cannot reach wallet service. Please ensure API Gateway is running on http://localhost:5000.');
          return;
        }
        this.toastService.error(err.message ?? 'Failed to generate PDF.');
      },
    });
  }

  downloadCsv(): void {
    this.csvLoading = true;
    forkJoin([
      this.walletService.getWalletForExport(),
      this.walletService.getTransactionsForExport(),
    ]).subscribe({
      next: ([wallet, res]) => {
        this.csvService.download(wallet, res);
        this.csvLoading = false;
      },
      error: (err: { message?: string; status?: number }) => {
        this.csvLoading = false;
        if (err.status === 0) {
          this.toastService.error('Cannot reach wallet service. Please ensure API Gateway is running on http://localhost:5000.');
          return;
        }
        this.toastService.error(err.message ?? 'Failed to export CSV.');
      },
    });
  }

  submitTopUp(): void {
    this.topUpSubmitted = true;
    if (this.topUpForm.invalid) return;

    this.topUpLoading = true;
    const { amount } = this.topUpForm.getRawValue();
    this.walletService.topUp({ amount }).subscribe({
      next: (res: TopUpResult) => {
        this.toastService.success(`Top-up successful! New balance: Rs ${res.newBalance}`);
        this.showTopUp = false;
        this.topUpLoading = false;
        this.topUpSubmitted = false;
        this.topUpForm.reset({ amount: 100 });
        this.loadTransactions();
        this.loadPayments();
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Top-up failed');
        this.topUpLoading = false;
      },
    });
  }

  submitTransfer(): void {
    this.txSubmitted = true;
    if (this.transferForm.invalid) return;

    this.txLoading = true;
    const { toEmail, amount, note } = this.transferForm.getRawValue();
    this.walletService.transfer({ toEmail, amount, description: note || undefined }).subscribe({
      next: (_res: TransferResult) => {
        this.toastService.success('Transfer successful!');
        this.showTransfer = false;
        this.txLoading = false;
        this.txSubmitted = false;
        this.transferForm.reset({ toEmail: '', amount: 1, note: '' });
        this.loadTransactions();
        this.loadPayments();
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Transfer failed');
        this.txLoading = false;
      },
    });
  }
}
