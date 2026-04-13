import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

import { TransactionResponse, WalletResponse } from '../../shared/models/wallet.model';

function formatInr(amount: number): string {
  return 'Rs. ' + new Intl.NumberFormat('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(amount);
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleString('en-IN', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit', hour12: true,
  });
}

@Injectable({ providedIn: 'root' })
export class TransactionPdfService {
  download(wallet: WalletResponse, transactions: TransactionResponse[]): void {
    const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
    const pageW = doc.internal.pageSize.getWidth();
    const now = new Date().toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' });

    // ── Header band ──────────────────────────────────────────────────────────
    doc.setFillColor(24, 24, 27);           // zinc-900
    doc.rect(0, 0, pageW, 38, 'F');

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(18);
    doc.setTextColor(37, 99, 235);          // blue-600
    doc.text('Digital Wallet', 14, 15);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.setTextColor(161, 161, 170);        // zinc-400
    doc.text('Transaction Statement', 14, 22);

    doc.setFontSize(8);
    doc.text(`Generated: ${now}`, 14, 29);
    doc.text(`Wallet ID: ${wallet.walletId}`, 14, 34);

    // ── Balance summary card ──────────────────────────────────────────────────
    const totalCredit  = transactions.filter(t => t.type === 'CREDIT').reduce((s, t) => s + t.amount, 0);
    const totalDebit   = transactions.filter(t => t.type === 'DEBIT').reduce((s, t) => s + t.amount, 0);

    doc.setFillColor(39, 39, 42);           // zinc-800
    doc.roundedRect(14, 44, pageW - 28, 26, 3, 3, 'F');

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(9);
    doc.setTextColor(161, 161, 170);

    // Balance
    doc.text('Current Balance', 20, 53);
    doc.setFontSize(14);
    doc.setTextColor(250, 250, 250);
    doc.text(formatInr(wallet.balance), 20, 61);

    // Credits
    const col2 = pageW / 2 - 10;
    doc.setFontSize(9);
    doc.setTextColor(161, 161, 170);
    doc.text('Total Credits', col2, 53);
    doc.setFontSize(12);
    doc.setTextColor(52, 211, 153);         // emerald-400
    doc.text(formatInr(totalCredit), col2, 61);

    // Debits
    const col3 = pageW - 55;
    doc.setFontSize(9);
    doc.setTextColor(161, 161, 170);
    doc.text('Total Debits', col3, 53);
    doc.setFontSize(12);
    doc.setTextColor(251, 113, 133);        // rose-400
    doc.text(formatInr(totalDebit), col3, 61);

    // ── Section title ─────────────────────────────────────────────────────────
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(10);
    doc.setTextColor(250, 250, 250);
    doc.text(`Transaction History  (${transactions.length} records)`, 14, 80);

    // ── Table ─────────────────────────────────────────────────────────────────
    const rows = transactions.map(tx => [
      formatDate(tx.createdAt),
      tx.type,
      tx.referenceType || '-',
      formatInr(tx.amount),
      tx.description || '-',
    ]);

    autoTable(doc, {
      startY: 84,
      head: [['Date & Time', 'Type', 'Reference', 'Amount', 'Description']],
      body: rows,
      theme: 'grid',
      styles: {
        fontSize: 8,
        cellPadding: 3,
        textColor: [228, 228, 231],         // zinc-200
        lineColor: [63, 63, 70],            // zinc-700
        lineWidth: 0.2,
        font: 'helvetica',
        fillColor: [24, 24, 27],            // zinc-900
      },
      headStyles: {
        fillColor: [39, 39, 42],            // zinc-800
        textColor: [161, 161, 170],         // zinc-400
        fontStyle: 'bold',
        fontSize: 8,
      },
      alternateRowStyles: {
        fillColor: [28, 28, 31],
      },
      columnStyles: {
        0: { cellWidth: 38 },
        1: { cellWidth: 20, halign: 'center' },
        2: { cellWidth: 26 },
        3: { cellWidth: 26, halign: 'right', fontStyle: 'bold' },
        4: { cellWidth: 'auto' },
      },
      didParseCell: (data) => {
        if (data.section === 'body' && data.column.index === 1) {
          if (data.cell.raw === 'CREDIT') {
            data.cell.styles.textColor = [52, 211, 153];  // emerald-400
          } else if (data.cell.raw === 'DEBIT') {
            data.cell.styles.textColor = [251, 113, 133]; // rose-400
          }
        }
        if (data.section === 'body' && data.column.index === 3) {
          const row = transactions[data.row.index];
          if (row?.type === 'CREDIT') {
            data.cell.styles.textColor = [52, 211, 153];
          } else if (row?.type === 'DEBIT') {
            data.cell.styles.textColor = [251, 113, 133];
          }
        }
      },
    });

    // ── Footer on each page ───────────────────────────────────────────────────
    const pageCount = (doc as jsPDF & { internal: { getNumberOfPages(): number } }).internal.getNumberOfPages();
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      const pageH = doc.internal.pageSize.getHeight();
      doc.setFillColor(24, 24, 27);
      doc.rect(0, pageH - 10, pageW, 10, 'F');
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(7);
      doc.setTextColor(113, 113, 122);      // zinc-500
      doc.text('Digital Wallet — Confidential', 14, pageH - 4);
      doc.text(`Page ${i} of ${pageCount}`, pageW - 14, pageH - 4, { align: 'right' });
    }

    // ── Save ──────────────────────────────────────────────────────────────────
    const filename = `transactions_${wallet.walletId.slice(0, 8)}_${new Date().toISOString().slice(0, 10)}.pdf`;
    doc.save(filename);
  }
}
