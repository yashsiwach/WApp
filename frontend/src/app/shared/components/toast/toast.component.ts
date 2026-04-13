import { Component } from '@angular/core';
import { AsyncPipe, NgClass } from '@angular/common';
import { Toast, ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [AsyncPipe, NgClass],
  template: `
    <div class="fixed top-4 right-4 z-50 flex flex-col gap-2 w-80">
      @for (toast of toastService.toasts$ | async; track toast.id) {
        <div
          class="flex items-center justify-between px-4 py-3 rounded-lg shadow-lg text-white text-sm font-medium transition-all border"
          [ngClass]="{
            'bg-emerald-600/95 border-emerald-500': toast.type === 'success',
            'bg-rose-600/95 border-rose-500': toast.type === 'error',
            'bg-blue-600/95 border-blue-500': toast.type === 'info'
          }"
        >
          <span>{{ toast.message }}</span>
          <button
            class="ml-3 text-white opacity-80 hover:opacity-100"
            (click)="toastService.remove(toast.id)"
          >
            &times;
          </button>
        </div>
      }
    </div>
  `,
})
export class ToastComponent {
  constructor(readonly toastService: ToastService) {}
}
