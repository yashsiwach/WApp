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
  templateUrl: './kyc.component.html',
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

  // Load both KYC submissions and wallet status so the latest document can reflect approval state correctly.
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

  // Submit the current KYC form and refresh the displayed review status after a successful upload.
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
