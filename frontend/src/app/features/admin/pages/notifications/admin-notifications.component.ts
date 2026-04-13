import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, Validators } from '@angular/forms';

import { AdminService } from '../../admin.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { LoaderComponent } from '../../../../shared/components/loader/loader.component';
import {
  NotificationTemplateDto,
  UpdateNotificationTemplateRequest,
} from '../../../../shared/models/admin.model';

@Component({
  selector: 'app-admin-notifications',
  standalone: true,
  imports: [ReactiveFormsModule, LoaderComponent],
  template: `
    <div class="space-y-4">

      <!-- Header -->
      <div>
        <h2 class="text-xl font-display font-bold text-zinc-100">Notification Templates</h2>
        <p class="text-zinc-500 text-sm mt-0.5">
          Manage email and SMS templates sent to users for system events.
        </p>
      </div>

      <app-loader [show]="loading" />

      @if (!loading) {
        @if (templates.length === 0) {
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl p-12 text-center">
            <p class="text-4xl mb-3">🔔</p>
            <p class="text-zinc-400 text-sm">No notification templates found.</p>
          </div>
        } @else {
          <div class="bg-zinc-900 border border-zinc-800 rounded-xl overflow-hidden">
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-zinc-700 text-zinc-400 text-left">
                    <th class="px-4 py-3 font-medium">Type</th>
                    <th class="px-4 py-3 font-medium">Channel</th>
                    <th class="px-4 py-3 font-medium">Subject</th>
                    <th class="px-4 py-3 font-medium">Status</th>
                    <th class="px-4 py-3 font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-zinc-800">
                  @for (tpl of templates; track tpl.id) {
                    <tr class="hover:bg-zinc-800/60 transition-colors">
                      <td class="px-4 py-3">
                        <span class="px-2 py-0.5 bg-zinc-700 text-zinc-300 rounded-full text-xs font-medium">
                          {{ tpl.type }}
                        </span>
                      </td>
                      <td class="px-4 py-3 text-zinc-400 text-xs uppercase tracking-wider">{{ tpl.channel }}</td>
                      <td class="px-4 py-3 text-zinc-200 max-w-xs truncate">{{ tpl.subject }}</td>
                      <td class="px-4 py-3">
                        <span
                          class="px-2 py-0.5 rounded-full text-xs font-semibold"
                          [class.bg-emerald-500/20]="tpl.isActive"
                          [class.text-emerald-300]="tpl.isActive"
                          [class.bg-zinc-700]="!tpl.isActive"
                          [class.text-zinc-500]="!tpl.isActive"
                        >{{ tpl.isActive ? 'Active' : 'Inactive' }}</span>
                      </td>
                      <td class="px-4 py-3">
                        <button
                          type="button"
                          (click)="openEdit(tpl)"
                          class="px-3 py-1.5 bg-zinc-800 hover:bg-zinc-700 border border-zinc-700 text-zinc-200 text-xs rounded-lg font-medium transition-colors"
                        >Edit</button>
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

    <!-- ── Edit Template Modal ── -->
    @if (editingTemplate) {
      <div class="fixed inset-0 bg-black/60 z-40 flex items-center justify-center p-4">
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-2xl w-full max-w-lg p-6 max-h-[90vh] overflow-y-auto">
          <div class="flex justify-between items-start mb-5">
            <div>
              <h3 class="text-lg font-bold text-zinc-100">Edit Template</h3>
              <p class="text-zinc-500 text-xs mt-0.5">
                <span class="px-1.5 py-0.5 bg-zinc-700 text-zinc-300 rounded text-[10px] uppercase tracking-wider">{{ editingTemplate.type }}</span>
                <span class="ml-1.5">{{ editingTemplate.channel }}</span>
              </p>
            </div>
            <button
              type="button"
              (click)="closeEdit()"
              class="text-zinc-500 hover:text-zinc-300 text-2xl leading-none ml-4"
            >×</button>
          </div>

          <form [formGroup]="editForm" (ngSubmit)="submitEdit()" novalidate class="space-y-4">
            <!-- Subject -->
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1.5">
                Subject <span class="text-rose-400">*</span>
              </label>
              <input
                type="text"
                formControlName="subject"
                placeholder="Email / notification subject…"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-xl text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-amber-400/30"
                [class.border-rose-500]="editSubmitted && editForm.controls.subject.invalid"
              />
              @if (editSubmitted && editForm.controls.subject.invalid) {
                <p class="text-rose-400 text-xs mt-1">Subject is required.</p>
              }
            </div>

            <!-- Body Template -->
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1.5">
                Body Template <span class="text-rose-400">*</span>
              </label>
              <textarea
                formControlName="bodyTemplate"
                rows="8"
                [placeholder]="bodyPlaceholder"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-xl text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-amber-400/30 resize-y font-mono"
                [class.border-rose-500]="editSubmitted && editForm.controls.bodyTemplate.invalid"
              ></textarea>
              @if (editSubmitted && editForm.controls.bodyTemplate.invalid) {
                <p class="text-rose-400 text-xs mt-1">Body template is required.</p>
              }
              <p class="text-zinc-600 text-xs mt-1">Supports dynamic placeholders wrapped in double curly braces, e.g. Name, Amount, Balance.</p>
            </div>

            <!-- Active Toggle -->
            <div class="flex items-center gap-3">
              <input
                type="checkbox"
                id="template-active"
                formControlName="isActive"
                class="h-4 w-4 rounded border-zinc-600 bg-zinc-800 text-amber-400 focus:ring-amber-400/30"
              />
              <label for="template-active" class="text-sm font-medium text-zinc-300 cursor-pointer">
                Template Active
              </label>
              <span class="text-xs text-zinc-500">(Inactive templates will not be sent)</span>
            </div>

            <!-- Actions -->
            <div class="flex gap-3 pt-2">
              <button
                type="button"
                (click)="closeEdit()"
                class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-3 rounded-xl hover:bg-zinc-800 transition-colors"
              >Cancel</button>
              <button
                type="submit"
                [disabled]="saving"
                class="flex-1 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold py-3 rounded-xl disabled:opacity-60 transition-colors"
              >{{ saving ? 'Saving…' : 'Save Changes' }}</button>
            </div>
          </form>
        </div>
      </div>
    }
  `,
})
export class AdminNotificationsComponent implements OnInit {
  loading = false;
  saving = false;
  editSubmitted = false;
  readonly bodyPlaceholder = 'Template body with placeholders like {{Name}}, {{Amount}}, {{Balance}}…';

  templates: NotificationTemplateDto[] = [];
  editingTemplate: NotificationTemplateDto | null = null;

  readonly editForm;

  constructor(
    fb: FormBuilder,
    private readonly adminService: AdminService,
    private readonly toastService: ToastService,
  ) {
    this.editForm = fb.nonNullable.group({
      subject:      ['', Validators.required],
      bodyTemplate: ['', Validators.required],
      isActive:     [true],
    });
  }

  ngOnInit(): void {
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.loading = true;
    this.adminService.getNotificationTemplates().subscribe({
      next: (templates) => {
        this.templates = templates ?? [];
        this.loading = false;
      },
      error: (err: { message?: string }) => {
        this.loading = false;
        this.toastService.error(err.message ?? 'Failed to load notification templates.');
      },
    });
  }

  openEdit(template: NotificationTemplateDto): void {
    this.editingTemplate = template;
    this.editSubmitted = false;
    this.editForm.setValue({
      subject:      template.subject,
      bodyTemplate: template.bodyTemplate,
      isActive:     template.isActive,
    });
  }

  closeEdit(): void {
    this.editingTemplate = null;
    this.editSubmitted = false;
  }

  submitEdit(): void {
    this.editSubmitted = true;
    if (this.editForm.invalid || !this.editingTemplate) return;

    this.saving = true;
    const payload: UpdateNotificationTemplateRequest = this.editForm.getRawValue();

    this.adminService.updateNotificationTemplate(this.editingTemplate.id, payload).subscribe({
      next: (updated) => {
        const idx = this.templates.findIndex((t) => t.id === updated.id);
        if (idx !== -1) {
          this.templates = [
            ...this.templates.slice(0, idx),
            updated,
            ...this.templates.slice(idx + 1),
          ];
        }
        this.saving = false;
        this.editingTemplate = null;
        this.editSubmitted = false;
        this.toastService.success('Template updated successfully.');
      },
      error: (err: { message?: string }) => {
        this.saving = false;
        this.toastService.error(err.message ?? 'Failed to update template.');
      },
    });
  }
}
