import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: number;
  type: 'success' | 'error' | 'info';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  private readonly toastsSubject = new BehaviorSubject<Toast[]>([]);
  readonly toasts$ = this.toastsSubject.asObservable();

  // Push a short-lived success toast into the shared toast stream.
  success(message: string): void {
    this.add({ type: 'success', message });
  }

  // Push a short-lived error toast into the shared toast stream.
  error(message: string): void {
    this.add({ type: 'error', message });
  }

  // Push an informational toast into the shared toast stream.
  info(message: string): void {
    this.add({ type: 'info', message });
  }

  // Assign an id, publish the toast, and schedule its automatic dismissal.
  private add(toast: Omit<Toast, 'id'>): void {
    const id = ++this.nextId;
    this.toastsSubject.next([...this.toastsSubject.value, { id, ...toast }]);
    setTimeout(() => this.remove(id), 3000);
  }

  // Remove a toast after timeout expiry or explicit dismissal.
  remove(id: number): void {
    this.toastsSubject.next(this.toastsSubject.value.filter((t) => t.id !== id));
  }
}
