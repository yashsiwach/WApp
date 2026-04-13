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
  template: `
    <div class="space-y-4">

      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-xl font-display font-bold text-zinc-100">KYC Reviews</h2>
          <p class="text-zinc-500 text-sm mt-0.5">{{ kycList.length }} pending document{{ kycList.length === 1 ? '' : 's' }}</p>
        </div>
        <button
          type="button"
          (click)="loadKyc()"
          [disabled]="loadingKyc"
          class="px-4 py-2 rounded-xl border border-zinc-700 text-sm text-zinc-300 hover:border-amber-400/40 hover:text-amber-300 transition-colors disabled:opacity-50"
        >
          ↻ Refresh
        </button>
      </div>

      <app-loader [show]="loadingKyc" />

      @if (!loadingKyc) {
        @if (kycList.length === 0) {
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl p-12 text-center">
            <p class="text-4xl mb-3">✅</p>
            <p class="text-zinc-400 text-sm font-medium">All caught up!</p>
            <p class="text-zinc-600 text-xs mt-1">No pending KYC reviews at this time.</p>
          </div>
        } @else {
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl overflow-hidden">
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-zinc-700 text-zinc-400 text-left">
                    <th class="px-4 py-3 font-medium">User</th>
                    <th class="px-4 py-3 font-medium">Document</th>
                    <th class="px-4 py-3 font-medium">Status</th>
                    <th class="px-4 py-3 font-medium">Submitted</th>
                    <th class="px-4 py-3 font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-zinc-800">
                  @for (doc of kycList; track doc.id) {
                    <tr class="hover:bg-zinc-800/60 transition-colors">
                      <td class="px-4 py-3">
                        <p class="text-zinc-200 text-sm font-medium">{{ doc.userFullName }}</p>
                        <p class="text-zinc-500 text-xs mt-0.5">{{ doc.userEmail }}</p>
                      </td>
                      <td class="px-4 py-3">
                        <span class="px-2 py-0.5 bg-amber-500/20 text-amber-300 rounded-full text-xs font-medium">
                          {{ doc.documentType }}
                        </span>
                        <p class="text-zinc-500 text-xs mt-0.5 font-mono">{{ doc.documentNumber }}</p>
                      </td>
                      <td class="px-4 py-3">
                        <span
                          class="px-2 py-0.5 rounded-full text-xs font-semibold"
                          [class.bg-yellow-500/20]="doc.status === 'Pending'"
                          [class.text-yellow-300]="doc.status === 'Pending'"
                          [class.bg-emerald-500/20]="doc.status === 'Approved'"
                          [class.text-emerald-300]="doc.status === 'Approved'"
                          [class.bg-rose-500/20]="doc.status === 'Rejected'"
                          [class.text-rose-300]="doc.status === 'Rejected'"
                        >{{ doc.status }}</span>
                      </td>
                      <td class="px-4 py-3 text-zinc-400 text-xs">
                        {{ doc.submittedAt | date:'dd MMM yyyy, HH:mm' }}
                      </td>
                      <td class="px-4 py-3">
                        @if (doc.status === 'Pending') {
                          <div class="flex gap-2">
                            <button
                              type="button"
                              (click)="openApprove(doc)"
                              class="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white text-xs rounded-lg font-semibold transition-colors"
                            >Approve</button>
                            <button
                              type="button"
                              (click)="openReject(doc)"
                              class="px-3 py-1.5 bg-rose-600 hover:bg-rose-700 text-white text-xs rounded-lg font-semibold transition-colors"
                            >Reject</button>
                          </div>
                        } @else {
                          <span class="text-zinc-600 text-xs italic">{{ doc.adminNote || 'Reviewed' }}</span>
                        }
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        }
      }
    </div>

    <!-- ── Approve KYC Modal ── -->
    @if (approveDoc) {
      <div class="fixed inset-0 bg-black/60 z-40 flex items-center justify-center p-4">
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-2xl w-full max-w-sm p-6">
          <h3 class="text-lg font-bold text-zinc-100 mb-1">Approve KYC</h3>
          <p class="text-zinc-400 text-sm mb-5">
            <span class="font-semibold text-amber-300">{{ approveDoc.documentType }}</span>
            — {{ approveDoc.userFullName }}
          </p>
          <form [formGroup]="approveForm" (ngSubmit)="submitApprove()" novalidate class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1.5">Admin Note <span class="text-zinc-500 font-normal">(optional)</span></label>
              <textarea
                formControlName="adminNote"
                rows="3"
                placeholder="Add approval notes…"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-xl text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-emerald-500/40 resize-none"
              ></textarea>
            </div>
            <div class="flex gap-3">
              <button
                type="button"
                (click)="approveDoc = null"
                class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-3 rounded-xl hover:bg-zinc-800 transition-colors"
              >Cancel</button>
              <button
                type="submit"
                [disabled]="actionLoading"
                class="flex-1 bg-emerald-600 hover:bg-emerald-700 text-white font-semibold py-3 rounded-xl disabled:opacity-60 transition-colors"
              >{{ actionLoading ? 'Approving…' : 'Approve' }}</button>
            </div>
          </form>
        </div>
      </div>
    }

    <!-- ── Reject KYC Modal ── -->
    @if (rejectDoc) {
      <div class="fixed inset-0 bg-black/60 z-40 flex items-center justify-center p-4">
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-2xl w-full max-w-sm p-6">
          <h3 class="text-lg font-bold text-zinc-100 mb-1">Reject KYC</h3>
          <p class="text-zinc-400 text-sm mb-5">
            <span class="font-semibold text-amber-300">{{ rejectDoc.documentType }}</span>
            — {{ rejectDoc.userFullName }}
          </p>
          <form [formGroup]="rejectForm" (ngSubmit)="submitReject()" novalidate class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1.5">
                Rejection Reason <span class="text-rose-400">*</span>
              </label>
              <textarea
                formControlName="adminNote"
                rows="3"
                placeholder="Explain why this document is rejected…"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-xl text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-rose-500/40 resize-none"
                [class.border-rose-500]="rejectSubmitted && rejectForm.controls.adminNote.invalid"
              ></textarea>
              @if (rejectSubmitted && rejectForm.controls.adminNote.invalid) {
                <p class="text-rose-400 text-xs mt-1">A rejection reason is required.</p>
              }
            </div>
            <div class="flex gap-3">
              <button
                type="button"
                (click)="rejectDoc = null"
                class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-3 rounded-xl hover:bg-zinc-800 transition-colors"
              >Cancel</button>
              <button
                type="submit"
                [disabled]="actionLoading"
                class="flex-1 bg-rose-600 hover:bg-rose-700 text-white font-semibold py-3 rounded-xl disabled:opacity-60 transition-colors"
              >{{ actionLoading ? 'Rejecting…' : 'Reject' }}</button>
            </div>
          </form>
        </div>
      </div>
    }
  `,
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

  openApprove(doc: KycReviewDto): void {
    this.approveDoc = doc;
    this.approveForm.reset();
  }

  openReject(doc: KycReviewDto): void {
    this.rejectDoc = doc;
    this.rejectSubmitted = false;
    this.rejectForm.reset();
  }

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
