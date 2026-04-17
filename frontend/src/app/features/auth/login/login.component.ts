import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../auth.service';
import { TokenService } from '../../../core/services/token.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  readonly form;
  loading = false;
  submitted = false;
  showPassword = false;

  constructor(
    fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly tokenService: TokenService,
    private readonly toastService: ToastService,
    private readonly router: Router,
  ) {
    this.form = fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  // Validate the form, save the returned session, and route users to their role-specific landing page.
  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) return;

    this.loading = true;
    this.authService.login(this.form.getRawValue()).subscribe({
      next: (res) => {
        this.tokenService.setTokens(res);
        this.loading = false;
        const destination = this.tokenService.isAdmin() ? '/admin' : '/dashboard/overview';
        void this.router.navigate([destination]);
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Login failed');
        this.loading = false;
      },
    });
  }

  // Toggle the password field between masked and plain text states.
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
