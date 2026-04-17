import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, Validators } from '@angular/forms';

import { AdminService } from '../../admin.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { LoaderComponent } from '../../../../shared/components/loader/loader.component';
import { KycActionRequest, KycReviewDto } from '../../../../shared/models/admin.model';

@Component({
  selector: 'app-admin-kyc',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule, LoaderComponent],
  templateUrl: './admin-kyc.component.html',
})
export class AdminKycComponent implements OnInit {
  loadingKyc = false;
  actionLoading = false;
  rejectSubmitted = false;

  kycList: KycReviewDto[] = [];
  approveDoc: KycReviewDto | null = null;
  rejectDoc: KycReviewDto | null = null;

  readonly approveForm;
  readonly rejectForm;

  constructor(
    fb: FormBuilder,
    private readonly adminService: AdminService,
    private readonly toastService: ToastService,
  ) {
    this.approveForm = fb.nonNullable.group({ adminNote: [''] });
    this.rejectForm = fb.nonNullable.group({ adminNote: ['', Validators.required] });
  }

  ngOnInit(): void {
    this.loadKyc();
  }

  // Load the pending KYC review queue for the admin review table.
  loadKyc(): void {
    this.loadingKyc = true;
    this.adminService.getPendingKyc().subscribe({
      next: (list) => {
        this.kycList = list;
        this.loadingKyc = false;
      },
      error: (err: { message?: string }) => {
        this.loadingKyc = false;
        this.toastService.error(err.message ?? 'Failed to load KYC reviews.');
      },
    });
  }

  // Open the approval modal and reset the optional note field.
  openApprove(doc: KycReviewDto): void {
    this.approveDoc = doc;
    this.approveForm.reset();
  }

  // Open the rejection modal and clear any previous validation state.
  openReject(doc: KycReviewDto): void {
    this.rejectDoc = doc;
    this.rejectSubmitted = false;
    this.rejectForm.reset();
  }

  // Submit an approval decision, then remove the reviewed document from the pending queue.
  submitApprove(): void {
    if (!this.approveDoc) return;
    this.actionLoading = true;
    const payload: KycActionRequest = { adminNote: this.approveForm.getRawValue().adminNote };
    this.adminService.approveKyc(this.approveDoc.id, payload).subscribe({
      next: () => {
        this.kycList = this.kycList.filter((k) => k.id !== this.approveDoc!.id);
        this.approveDoc = null;
        this.actionLoading = false;
        this.toastService.success('KYC document approved.');
      },
      error: (err: { message?: string }) => {
        this.actionLoading = false;
        this.toastService.error(err.message ?? 'Failed to approve KYC.');
      },
    });
  }

  // Submit a rejection decision with a required reason, then remove the reviewed document from the queue.
  submitReject(): void {
    this.rejectSubmitted = true;
    if (this.rejectForm.invalid || !this.rejectDoc) return;
    this.actionLoading = true;
    const payload: KycActionRequest = { adminNote: this.rejectForm.getRawValue().adminNote };
    this.adminService.rejectKyc(this.rejectDoc.id, payload).subscribe({
      next: () => {
        this.kycList = this.kycList.filter((k) => k.id !== this.rejectDoc!.id);
        this.rejectDoc = null;
        this.rejectSubmitted = false;
        this.actionLoading = false;
        this.toastService.success('KYC document rejected.');
      },
      error: (err: { message?: string }) => {
        this.actionLoading = false;
        this.toastService.error(err.message ?? 'Failed to reject KYC.');
      },
    });
  }
}
