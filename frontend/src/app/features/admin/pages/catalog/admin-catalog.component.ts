import { Component, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, Validators } from '@angular/forms';

import { AdminService } from '../../admin.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { LoaderComponent } from '../../../../shared/components/loader/loader.component';
import { CatalogAdminItemDto, CreateCatalogItemRequest, UpdateCatalogItemRequest } from '../../../../shared/models/admin.model';

@Component({
  selector: 'app-admin-catalog',
  standalone: true,
  imports: [DecimalPipe, ReactiveFormsModule, LoaderComponent],
  templateUrl: './admin-catalog.component.html',
})
export class AdminCatalogComponent implements OnInit {
  loadingCatalog = false;
  catalogLoading = false;
  catalogSubmitted = false;
  editSubmitted = false;
  showCreateCatalog = false;

  catalogItems: CatalogAdminItemDto[] = [];
  editingItem: CatalogAdminItemDto | null = null;
  deleteItem: CatalogAdminItemDto | null = null;

  readonly catalogForm;
  readonly editForm;

  constructor(
    fb: FormBuilder,
    private readonly adminService: AdminService,
    private readonly toastService: ToastService,
  ) {
    this.catalogForm = fb.nonNullable.group({
      name:        ['', Validators.required],
      description: [''],
      category:    ['GiftCard'],
      pointsCost:  [100, [Validators.required, Validators.min(1)]],
    });
    this.editForm = fb.nonNullable.group({
      name:        ['', Validators.required],
      description: [''],
      category:    ['GiftCard'],
      pointsCost:  [100, [Validators.required, Validators.min(1)]],
      isActive:    [true],
    });
  }

  ngOnInit(): void {
    this.loadCatalog();
  }

  // Load the current catalog items shown in the admin management table.
  loadCatalog(): void {
    this.loadingCatalog = true;
    this.adminService.getCatalogItems().subscribe({
      next: (items) => {
        this.catalogItems = items ?? [];
        this.loadingCatalog = false;
      },
      error: () => {
        this.loadingCatalog = false;
      },
    });
  }

  // Validate and submit the create form, then prepend the new item to the local table state.
  submitCatalogItem(): void {
    this.catalogSubmitted = true;
    if (this.catalogForm.invalid) return;
    this.catalogLoading = true;
    const payload: CreateCatalogItemRequest = this.catalogForm.getRawValue();
    this.adminService.createCatalogItem(payload).subscribe({
      next: (newItem) => {
        this.catalogItems = [newItem, ...this.catalogItems];
        this.showCreateCatalog = false;
        this.catalogSubmitted = false;
        this.catalogLoading = false;
        this.catalogForm.reset({
          name: '', description: '', category: 'GiftCard', pointsCost: 100,
        });
        this.toastService.success('Catalog item added successfully.');
      },
      error: (err: { message?: string }) => {
        this.catalogLoading = false;
        this.toastService.error(err.message ?? 'Failed to add catalog item.');
      },
    });
  }

  // Open the edit modal and seed the form with the selected catalog item values.
  openEditModal(item: CatalogAdminItemDto): void {
    this.editingItem = item;
    this.editForm.reset({
      name: item.name,
      description: item.description,
      category: item.category,
      pointsCost: item.pointsCost,
      isActive: item.isActive,
    });
    this.editSubmitted = false;
  }

  // Close the edit modal and clear its validation state.
  closeEditModal(): void {
    this.editingItem = null;
    this.editSubmitted = false;
  }

  // Persist edits for the selected catalog item and replace the matching row in the local list.
  submitEditCatalogItem(): void {
    this.editSubmitted = true;
    if (this.editForm.invalid || !this.editingItem) return;
    this.catalogLoading = true;
    const itemIdToUpdate = this.editingItem.id;
    const payload: UpdateCatalogItemRequest = this.editForm.getRawValue();
    this.adminService.updateCatalogItem(itemIdToUpdate, payload).subscribe({
      next: (updated) => {
        const idx = this.catalogItems.findIndex((i) => i.id === itemIdToUpdate);
        if (idx >= 0) {
          this.catalogItems = [
            ...this.catalogItems.slice(0, idx),
            updated,
            ...this.catalogItems.slice(idx + 1),
          ];
        }
        this.closeEditModal();
        this.catalogLoading = false;
        this.toastService.success('Catalog item updated successfully.');
      },
      error: (err: { message?: string } | any) => {
        this.catalogLoading = false;
        console.error('Update error:', err);
        this.toastService.error(err?.message ?? 'Failed to update catalog item.');
      },
    });
  }

  // Open the delete confirmation modal for the selected catalog item.
  openDeleteModal(item: CatalogAdminItemDto): void {
    this.deleteItem = item;
  }

  // Close the delete confirmation modal without changing the table.
  closeDeleteModal(): void {
    this.deleteItem = null;
  }

  // Remove the selected catalog item and drop its row from the local table on success.
  confirmDelete(): void {
    if (!this.deleteItem) return;
    this.catalogLoading = true;
    const itemIdToDelete = this.deleteItem.id;
    this.adminService.deleteCatalogItem(itemIdToDelete).subscribe({
      next: () => {
        this.catalogItems = this.catalogItems.filter((item) => item.id !== itemIdToDelete);
        this.deleteItem = null;
        this.catalogLoading = false;
        this.toastService.success('Catalog item removed from active catalog.');
      },
      error: (err: { message?: string } | any) => {
        this.catalogLoading = false;
        console.error('Delete error:', err);
        this.toastService.error(err?.message ?? 'Failed to delete catalog item.');
      },
    });
  }
}
