import { Component } from '@angular/core';
import { AsyncPipe, NgClass } from '@angular/common';
import { Toast, ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [AsyncPipe, NgClass],
  templateUrl: './toast.component.html',
  
})
export class ToastComponent {
  constructor(readonly toastService: ToastService) {}
}
