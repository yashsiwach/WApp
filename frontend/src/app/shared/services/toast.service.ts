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

  success(message: string): void {
    this.add({ type: 'success', message });
  }

  error(message: string): void {
    this.add({ type: 'error', message });
  }

  info(message: string): void {
    this.add({ type: 'info', message });
  }

  private add(toast: Omit<Toast, 'id'>): void {
    const id = ++this.nextId;
    this.toastsSubject.next([...this.toastsSubject.value, { id, ...toast }]);
    setTimeout(() => this.remove(id), 3000);
  }

  remove(id: number): void {
    this.toastsSubject.next(this.toastsSubject.value.filter((t) => t.id !== id));
  }
}
