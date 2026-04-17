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
  templateUrl: './admin-notifications.component.html',
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

  // Load all notification templates that can be edited from the admin screen.
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

  // Open the edit modal and populate the form with the selected template.
  openEdit(template: NotificationTemplateDto): void {
    this.editingTemplate = template;
    this.editSubmitted = false;
    this.editForm.setValue({
      subject:      template.subject,
      bodyTemplate: template.bodyTemplate,
      isActive:     template.isActive,
    });
  }

  // Close the edit modal and clear the submit state.
  closeEdit(): void {
    this.editingTemplate = null;
    this.editSubmitted = false;
  }

  // Persist template changes and replace the updated template in the local list.
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
