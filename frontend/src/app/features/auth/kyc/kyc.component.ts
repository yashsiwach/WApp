import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';

import { AuthService } from '../auth.service';
import { WalletService } from '../../wallet/wallet.service';
import { ToastService } from '../../../shared/services/toast.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import { KycInfo } from '../../../shared/models/auth.model';

@Component({
  selector: 'app-kyc',
  standalone: true,
  imports: [ReactiveFormsModule, LoaderComponent, DatePipe],
  template: `
    <div class="mx-auto max-w-2xl space-y-6 p-6 text-slate-900">
      <h1 class="text-2xl font-display font-bold text-slate-900">KYC Verification</h1>
      <p class="text-sm text-slate-500">Submit a document type and hosted file URL so the backend review flow can process your verification.</p>

      <div class="rounded-xl border border-slate-200 bg-white/95 p-6 shadow-sm">
        <h2 class="mb-4 text-lg font-semibold text-slate-900">Latest Submission</h2>
        <app-loader [show]="loadingStatus" />

        @if (!loadingStatus) {
          @if (kycInfo) {
            <div class="space-y-3">
              <div class="flex items-center gap-3">
                <span
                  class="px-3 py-1 rounded-full text-sm font-semibold"
                  [class.bg-emerald-500/20]="kycInfo.status === 'Approved'"
                  [class.text-emerald-700]="kycInfo.status === 'Approved'"
                  [class.bg-blue-500/20]="kycInfo.status === 'Pending'"
                  [class.text-blue-700]="kycInfo.status === 'Pending'"
                  [class.bg-rose-500/20]="kycInfo.status === 'Rejected'"
                  [class.text-rose-700]="kycInfo.status === 'Rejected'"
                >{{ kycInfo.status }}</span>
                @if (kycInfo.status === 'Approved') {
                  <span class="text-emerald-700 text-sm">Your identity is verified.</span>
                } @else if (kycInfo.status === 'Pending') {
                  <span class="text-sm text-slate-500">Under review, usually within 24 hours.</span>
                } @else if (kycInfo.status === 'Rejected') {
                  <span class="text-rose-700 text-sm">Rejected. You can submit a fresh document below.</span>
                }
              </div>

              <div class="grid grid-cols-2 gap-3 text-sm mt-2">
                <div>
                  <p class="mb-0.5 text-xs text-slate-500">Document Type</p>
                  <p class="text-slate-700">{{ kycInfo.docType }}</p>
                </div>
                <div>
                  <p class="mb-0.5 text-xs text-slate-500">Submitted</p>
                  <p class="text-slate-700">{{ kycInfo.submittedAt | date:'dd MMM yyyy, HH:mm' }}</p>
                </div>
              </div>

              @if (kycInfo.reviewNotes) {
                <div class="mt-3 rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm">
                  <p class="mb-1 text-xs text-slate-500">{{ kycInfo.status === 'Rejected' ? 'Rejection Reason' : 'Admin Note' }}</p>
                  <p class="whitespace-pre-wrap text-slate-700">{{ kycInfo.reviewNotes }}</p>
                </div>
              }
            </div>
          } @else {
            <p class="text-sm text-slate-500">No KYC submission on file. Submit your document below.</p>
          }
        }
      </div>

      @if (!kycInfo || kycInfo.status === 'Rejected') {
        <div class="rounded-xl border border-slate-200 bg-white/95 p-6 shadow-sm">
          <h2 class="mb-4 text-lg font-semibold text-slate-900">Submit KYC Document</h2>
          <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate class="space-y-4">
            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Document Type</label>
              <select
                formControlName="docType"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
                [class.border-red-500]="submitted && form.controls.docType.invalid"
              >
                <option value="">Select document type</option>
                <option value="Aadhaar">Aadhaar</option>
                <option value="PAN">PAN</option>
                <option value="Passport">Passport</option>
              </select>
              @if (submitted && form.controls.docType.invalid) {
                <p class="text-red-500 text-xs mt-1">Document type is required</p>
              }
            </div>

            <div>
              <label class="mb-1 block text-sm font-medium text-slate-700">Hosted File URL</label>
              <input
                type="url"
                formControlName="fileUrl"
                placeholder="https://example.com/my-document.pdf"
                class="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-accent/30"
                [class.border-red-500]="submitted && form.controls.fileUrl.invalid"
              />
              @if (submitted && form.controls.fileUrl.invalid) {
                <p class="text-red-500 text-xs mt-1">A valid file URL is required</p>
              }
            </div>

            <div class="rounded-lg border border-slate-200 bg-slate-50 p-3 text-xs text-slate-500">
              The current backend contract accepts a document type and file URL for KYC submissions.
            </div>

            <button
              type="submit"
              [disabled]="loading"
              class="w-full bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-lg transition-colors disabled:opacity-60"
            >
              {{ loading ? 'Submitting...' : 'Submit for Verification' }}
            </button>
          </form>
        </div>
      }
    </div>
  `,
})
export class KycComponent implements OnInit {
  readonly form;
  loading = false;
  loadingStatus = false;
  submitted = false;
  kycInfo: KycInfo | null = null;

  constructor(
    fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly walletService: WalletService,
    private readonly toastService: ToastService,
  ) {
    this.form = fb.nonNullable.group({
      docType: ['', Validators.required],
      fileUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/i)]],
    });
  }

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus(): void {
    this.loadingStatus = true;
    forkJoin({
      documents: this.authService.getKycStatus(),
      wallet: this.walletService.getWallet(),
    }).subscribe({
      next: ({ documents, wallet }) => {
        const latestSubmission =
          [...documents].sort(
            (a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime(),
          )[0] ?? null;

        this.kycInfo =
          latestSubmission && latestSubmission.status === 'Pending' && wallet.kycVerified
            ? { ...latestSubmission, status: 'Approved' }
            : latestSubmission;

        this.loadingStatus = false;
      },
      error: () => {
        this.loadingStatus = false;
      },
    });
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) return;

    this.loading = true;
    this.authService.kycSubmit(this.form.getRawValue()).subscribe({
      next: () => {
        this.toastService.success('KYC submitted! Our team will review it soon.');
        this.loading = false;
        this.submitted = false;
        this.form.reset();
        this.loadStatus();
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Submission failed');
        this.loading = false;
      },
    });
  }
}
