import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../auth.service';
import { TokenService } from '../../../core/services/token.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './signup.component.html',

})
export class SignupComponent {
  readonly form;
  loading = false;
  submitted = false;
  otpSent = false;
  otpVerified = false;
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
      phone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      otpCode: ['', [Validators.pattern(/^\d{6}$/)]],
    });
  }

  // Advance the signup flow one step at a time: send OTP, verify OTP, then register the account.
  onSubmit(): void {
    this.submitted = true;
    // The same submit button drives the whole staged signup flow.
    if (!this.otpSent) {
      this.sendOtp();
      return;
    }

    if (!this.otpVerified) {
      this.verifyOtp();
      return;
    }

    this.register();
  }

  // Restart OTP delivery without clearing the already entered identity fields.
  resendOtp(): void {
    this.submitted = true;
    this.sendOtp();
  }

  // Reset the verification step whenever the user needs to request a fresh OTP.
  resetOtpFlow(): void {
    this.otpSent = false;
    this.otpVerified = false;
    this.form.controls.otpCode.setValue('');
    this.form.controls.otpCode.markAsPristine();
    this.form.controls.otpCode.markAsUntouched();
  }

  // Invalidate the current OTP step when the email or phone identity changes.
  onIdentityChanged(): void {
    if (this.otpSent) {
      this.resetOtpFlow();
    }
  }

  // Request an OTP once the identity and password fields are valid.
  private sendOtp(): void {
    if (
      this.form.controls.email.invalid ||
      this.form.controls.phone.invalid ||
      this.form.controls.password.invalid
    ) {
      return;
    }

    this.loading = true;
    this.authService.sendOtp({ email: this.form.controls.email.getRawValue() }).subscribe({
      next: (res) => {
        // Clear any previous code so the user always enters a fresh OTP.
        this.otpSent = true;
        this.otpVerified = false;
        this.form.controls.otpCode.setValue('');
        this.toastService.success(res.message || 'OTP sent to your email.');
        this.loading = false;
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Unable to send OTP');
        this.loading = false;
      },
    });
  }

  // Confirm the entered OTP before enabling final account creation.
  private verifyOtp(): void {
    if (this.form.controls.otpCode.invalid || !this.form.controls.otpCode.getRawValue()) {
      return;
    }

    this.loading = true;
    this.authService
      .verifyOtp({
        // Verification is tied to the email used to request the OTP.
        email: this.form.controls.email.getRawValue(),
        code: this.form.controls.otpCode.getRawValue(),
      })
      .subscribe({
        next: () => {
          this.otpVerified = true;
          this.toastService.success('Email verified. Finish creating your account.');
          this.loading = false;
        },
        error: (err: { message?: string }) => {
          this.toastService.error(err.message ?? 'OTP verification failed');
          this.loading = false;
        },
      });
  }

  // Create the user account and immediately store the authenticated session.
  private register(): void {
    if (
      this.form.controls.email.invalid ||
      this.form.controls.phone.invalid ||
      this.form.controls.password.invalid
    ) {
      return;
    }

    this.loading = true;
    this.authService
      .register({
        // Reuse the validated form values from the earlier OTP steps for final account creation.
        email: this.form.controls.email.getRawValue(),
        phone: this.form.controls.phone.getRawValue(),
        password: this.form.controls.password.getRawValue(),
      })
      .subscribe({
        next: (res) => {
          this.tokenService.setTokens(res);
          this.toastService.success('Account created! Welcome aboard.');
          this.loading = false;
          // New users land directly on the dashboard with an active session.
          this.router.navigate(['/dashboard/overview']);
        },
        error: (err: { message?: string }) => {
          this.toastService.error(err.message ?? 'Registration failed');
          this.loading = false;
        },
      });
  }

  // Toggle the password field between masked and plain text states.
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
