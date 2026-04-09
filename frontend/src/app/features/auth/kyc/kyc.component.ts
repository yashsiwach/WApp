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
    <div class="max-w-2xl mx-auto p-6 space-y-6">
      <h1 class="text-2xl font-display font-bold text-zinc-100">KYC Verification</h1>
      <p class="text-zinc-400 text-sm">Submit a document type and hosted file URL so the backend review flow can process your verification.</p>

      <div class="bg-zinc-900/80 border border-zinc-800 rounded-xl shadow p-6">
        <h2 class="text-lg font-semibold text-zinc-100 mb-4">Latest Submission</h2>
        <app-loader [show]="loadingStatus" />

        @if (!loadingStatus) {
          @if (kycInfo) {
            <div class="space-y-3">
              <div class="flex items-center gap-3">
                <span
                  class="px-3 py-1 rounded-full text-sm font-semibold"
                  [class.bg-emerald-500/20]="kycInfo.status === 'Approved'"
                  [class.text-emerald-300]="kycInfo.status === 'Approved'"
                  [class.bg-yellow-500/20]="kycInfo.status === 'Pending'"
                  [class.text-yellow-300]="kycInfo.status === 'Pending'"
                  [class.bg-rose-500/20]="kycInfo.status === 'Rejected'"
                  [class.text-rose-300]="kycInfo.status === 'Rejected'"
                >{{ kycInfo.status }}</span>
                @if (kycInfo.status === 'Approved') {
                  <span class="text-emerald-300 text-sm">Your identity is verified.</span>
                } @else if (kycInfo.status === 'Pending') {
                  <span class="text-zinc-400 text-sm">Under review, usually within 24 hours.</span>
                } @else if (kycInfo.status === 'Rejected') {
                  <span class="text-rose-300 text-sm">Rejected. You can submit a fresh document below.</span>
                }
              </div>

              <div class="grid grid-cols-2 gap-3 text-sm mt-2">
                <div>
                  <p class="text-zinc-500 text-xs mb-0.5">Document Type</p>
                  <p class="text-zinc-200">{{ kycInfo.docType }}</p>
                </div>
                <div>
                  <p class="text-zinc-500 text-xs mb-0.5">Submitted</p>
                  <p class="text-zinc-200">{{ kycInfo.submittedAt | date:'dd MMM yyyy, HH:mm' }}</p>
                </div>
              </div>

              @if (kycInfo.reviewNotes) {
                <div class="mt-3 rounded-lg border border-zinc-700 bg-zinc-800/50 p-3 text-sm">
                  <p class="text-zinc-400 text-xs mb-1">{{ kycInfo.status === 'Rejected' ? 'Rejection Reason' : 'Admin Note' }}</p>
                  <p class="text-zinc-200 whitespace-pre-wrap">{{ kycInfo.reviewNotes }}</p>
                </div>
              }
            </div>
          } @else {
            <p class="text-zinc-500 text-sm">No KYC submission on file. Submit your document below.</p>
          }
        }
      </div>

      @if (!kycInfo || kycInfo.status === 'Rejected') {
        <div class="bg-zinc-900/80 border border-zinc-800 rounded-xl shadow p-6">
          <h2 class="text-lg font-semibold text-zinc-100 mb-4">Submit KYC Document</h2>
          <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Document Type</label>
              <select
                formControlName="docType"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 text-zinc-100 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-accent"
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
              <label class="block text-sm font-medium text-zinc-300 mb-1">Hosted File URL</label>
              <input
                type="url"
                formControlName="fileUrl"
                placeholder="https://example.com/my-document.pdf"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 text-zinc-100 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-accent"
                [class.border-red-500]="submitted && form.controls.fileUrl.invalid"
              />
              @if (submitted && form.controls.fileUrl.invalid) {
                <p class="text-red-500 text-xs mt-1">A valid file URL is required</p>
              }
            </div>

            <div class="bg-zinc-800/50 rounded-lg p-3 text-xs text-zinc-400">
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
