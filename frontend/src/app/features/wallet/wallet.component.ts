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
  templateUrl: './wallet.component.html',

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

  // Open the top-up dialog and remember which wallet should receive the funds.
  openTopUp(wallet: WalletResponse): void {
    this.currentWalletId = wallet.walletId;
    this.showTopUp = true;
  }

  // Show the selected transaction in the detail modal.
  openTransactionDetails(tx: TransactionResponse): void {
    this.selectedTransaction = tx;
  }

  // Close the transaction detail modal without touching the list state.
  closeTransactionDetails(): void {
    this.selectedTransaction = null;
  }

  // Load the current transaction page and keep pagination metadata in sync.
  loadTransactions(): void {
    this.loadingTx = true;
    this.walletService.getTransactions(this.txPage).subscribe({
      next: (res) => {
        // Keep the table rows and pager totals aligned with the latest backend response.
        this.transactions = res.items;
        this.txTotalPages = res.totalPages || 1;
        this.loadingTx = false;
      },
      error: () => (this.loadingTx = false),
    });
  }

  // Load recent top-up payments for the payment history section.
  loadPayments(): void {
    this.loadingPayments = true;
    this.walletService.getPaymentHistory().subscribe({
      next: (res) => {
        // Cache the transformed payment history for the separate payments section.
        this.payments = res;
        this.loadingPayments = false;
      },
      error: () => (this.loadingPayments = false),
    });
  }

  // Move backward through paginated transaction history.
  prevPage(): void {
    if (this.txPage > 1) {
      this.txPage--;
      this.loadTransactions();
    }
  }

  // Move forward through paginated transaction history when another page exists.
  nextPage(): void {
    if (this.txPage < this.txTotalPages) {
      this.txPage++;
      this.loadTransactions();
    }
  }

  // Gather wallet and transaction data together before generating the PDF export.
  downloadPdf(): void {
    this.pdfLoading = true;
    forkJoin([
      // Export needs both the wallet header details and the full transaction dataset.
      this.walletService.getWalletForExport(),
      this.walletService.getTransactionsForExport(),
    ]).subscribe({
      next: ([wallet, res]) => {
        this.pdfService.download(wallet, res);
        this.pdfLoading = false;
      },
      error: (err: { message?: string; status?: number }) => {
        this.pdfLoading = false;
        // Status 0 usually means the API is unreachable rather than a domain validation failure.
        if (err.status === 0) {
          this.toastService.error('Cannot reach wallet service. Please ensure API Gateway is running on http://localhost:5000.');
          return;
        }
        this.toastService.error(err.message ?? 'Failed to generate PDF.');
      },
    });
  }

  // Gather wallet and transaction data together before generating the CSV export.
  downloadCsv(): void {
    this.csvLoading = true;
    forkJoin([
      // Export needs both the wallet header details and the full transaction dataset.
      this.walletService.getWalletForExport(),
      this.walletService.getTransactionsForExport(),
    ]).subscribe({
      next: ([wallet, res]) => {
        this.csvService.download(wallet, res);
        this.csvLoading = false;
      },
      error: (err: { message?: string; status?: number }) => {
        this.csvLoading = false;
        // Status 0 usually means the API is unreachable rather than a domain validation failure.
        if (err.status === 0) {
          this.toastService.error('Cannot reach wallet service. Please ensure API Gateway is running on http://localhost:5000.');
          return;
        }
        this.toastService.error(err.message ?? 'Failed to export CSV.');
      },
    });
  }

  // Submit the top-up form, reset the modal state, and refresh wallet-related lists on success.
  submitTopUp(): void {
    this.topUpSubmitted = true;
    if (this.topUpForm.invalid) return;

    this.topUpLoading = true;
    const { amount } = this.topUpForm.getRawValue();
    this.walletService.topUp({ amount }).subscribe({
      next: (res: TopUpResult) => {
        this.toastService.success(`Top-up successful! New balance: Rs ${res.newBalance}`);
        // Reset the modal and form so the next top-up starts from a clean default state.
        this.showTopUp = false;
        this.topUpLoading = false;
        this.topUpSubmitted = false;
        this.topUpForm.reset({ amount: 100 });
        // Refresh both views that depend on the newly created top-up transaction.
        this.loadTransactions();
        this.loadPayments();
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Top-up failed');
        this.topUpLoading = false;
      },
    });
  }

  // Submit the transfer form, then refresh the transaction and payment views after completion.
  submitTransfer(): void {
    this.txSubmitted = true;
    if (this.transferForm.invalid) return;

    this.txLoading = true;
    const { toEmail, amount, note } = this.transferForm.getRawValue();
    this.walletService.transfer({ toEmail, amount, description: note || undefined }).subscribe({
      next: (_res: TransferResult) => {
        this.toastService.success('Transfer successful!');
        // Clear the transfer dialog so stale recipient data is not left in the form.
        this.showTransfer = false;
        this.txLoading = false;
        this.txSubmitted = false;
        this.transferForm.reset({ toEmail: '', amount: 1, note: '' });
        // Reload the tables because transfers affect both balance history and payment-related activity.
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
