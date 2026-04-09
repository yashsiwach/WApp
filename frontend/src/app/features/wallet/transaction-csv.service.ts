import { Injectable } from '@angular/core';

import { TransactionResponse, WalletResponse } from '../../shared/models/wallet.model';

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleString('en-IN', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: true,
  });
}

function escapeCsv(value: string | number): string {
  const text = String(value ?? '');
  return /[",\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

@Injectable({ providedIn: 'root' })
export class TransactionCsvService {
  download(wallet: WalletResponse, transactions: TransactionResponse[]): void {
    const headers = [
      'Transaction ID',
      'Date & Time',
      'Type',
      'Amount',
      'Status',
      'Reference Type',
      'Reference ID',
      'Description',
      'Wallet ID',
    ];

    const rows = transactions.map((tx) => [
      tx.id,
      formatDate(tx.createdAt),
      tx.type,
      tx.amount,
      tx.status,
      tx.referenceType || '-',
      tx.referenceId || '-',
      tx.description || '-',
      wallet.walletId,
    ]);

    const csv = [headers, ...rows]
      .map((row) => row.map((cell) => escapeCsv(cell)).join(','))
      .join('\n');

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `transactions_${wallet.walletId.slice(0, 8)}_${new Date().toISOString().slice(0, 10)}.csv`;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
