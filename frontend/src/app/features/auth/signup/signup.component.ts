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
  template: `
    <div class="relative min-h-screen overflow-hidden bg-surface-950 text-slate-900">
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_top_right,_rgba(59,130,246,0.2),_transparent_30%),radial-gradient(circle_at_bottom_left,_rgba(14,165,233,0.14),_transparent_32%),linear-gradient(180deg,_rgba(248,250,255,0.98),_rgba(238,244,255,1))]"></div>
      <div class="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-blue-400/70 to-transparent"></div>

      <div class="relative mx-auto flex min-h-screen max-w-7xl items-center px-4 py-8 sm:px-6 lg:px-8 lg:py-10">
        <div class="grid w-full gap-8 lg:grid-cols-[1fr_1.02fr] lg:items-start lg:gap-12">
          <section class="hidden lg:block lg:pt-8">
            <div class="max-w-xl">
              <span class="inline-flex items-center rounded-full border border-accent/20 bg-accent/10 px-4 py-1 text-xs font-semibold uppercase tracking-[0.24em] text-accent">
                New Account
              </span>
              <h1 class="mt-6 font-display text-5xl font-bold leading-tight text-slate-900">
                Create your account with a clean, guided onboarding flow.
              </h1>
              <p class="mt-5 max-w-lg text-base leading-7 text-slate-600">
                Register once, verify your email with OTP, and move straight into wallet features, rewards, KYC, and support.
              </p>

              <div class="mt-10 space-y-4">
                <div class="rounded-2xl border border-slate-200 bg-white/85 p-5 shadow-[0_18px_40px_rgba(37,99,235,0.12)] backdrop-blur-sm">
                  <p class="text-sm font-semibold uppercase tracking-[0.2em] text-accent">Step 1</p>
                  <p class="mt-2 text-lg font-semibold text-slate-900">Enter email, phone, and password</p>
                  <p class="mt-2 text-sm text-slate-600">Your existing validation and registration rules stay exactly as they are.</p>
                </div>
                <div class="rounded-2xl border border-slate-200 bg-white/85 p-5 shadow-[0_18px_40px_rgba(37,99,235,0.12)] backdrop-blur-sm">
                  <p class="text-sm font-semibold uppercase tracking-[0.2em] text-accent">Step 2</p>
                  <p class="mt-2 text-lg font-semibold text-slate-900">Verify email OTP</p>
                  <p class="mt-2 text-sm text-slate-600">The current OTP send and verify flow remains untouched, just surfaced more clearly.</p>
                </div>
              </div>
            </div>
          </section>

          <section class="mx-auto w-full max-w-md self-start lg:max-w-lg">
            <div class="overflow-hidden rounded-[28px] border border-slate-200 bg-white/95 shadow-[0_28px_70px_rgba(37,99,235,0.18)] backdrop-blur-xl">
              <div class="border-b border-slate-200 bg-slate-50/90 px-6 py-6 sm:px-8">
                <div class="flex items-start justify-between gap-4">
                  <div class="min-w-0">
                    <span class="inline-flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-600 via-sky-500 to-cyan-500 text-xl font-bold text-white shadow-lg shadow-blue-500/30">
                      DW
                    </span>
                    <h2 class="mt-4 font-display text-3xl font-bold text-slate-900">Create account</h2>
                    <p class="mt-2 text-sm leading-6 text-slate-600">
                      Finish a simple, secure setup and start using DigitalWallet right away.
                    </p>
                  </div>
                  <a
                    routerLink="/login"
                    class="hidden shrink-0 whitespace-nowrap rounded-full border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition-all duration-200 hover:border-accent/40 hover:text-accent sm:inline-flex"
                  >
                    Sign in
                  </a>
                </div>
              </div>

              <div class="px-6 py-6 sm:px-8 sm:py-7">
                <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate class="space-y-5">
                  <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                    <div class="flex items-start justify-between gap-3">
                      <div>
                        <p class="text-sm font-semibold text-slate-900">Email verification</p>
                        <p class="mt-1 text-xs leading-5 text-slate-600">
                          {{ otpVerified ? 'Email verified. You can finish creating your account.' : otpSent ? 'Enter the 6-digit code sent to your email to continue.' : 'We will send a one-time password before registration completes.' }}
                        </p>
                      </div>
                      <span
                        class="inline-flex shrink-0 rounded-full px-3 py-1 text-xs font-semibold"
                        [class.bg-emerald-500/15]="otpVerified"
                        [class.text-emerald-700]="otpVerified"
                        [class.bg-blue-500/15]="!otpVerified"
                        [class.text-blue-700]="!otpVerified"
                      >
                        {{ otpVerified ? 'Verified' : otpSent ? 'OTP Sent' : 'Pending' }}
                      </span>
                    </div>
                  </div>

                  <div class="space-y-2">
                    <label for="signup-email" class="block text-sm font-medium text-slate-700">Email address</label>
                    <div class="group relative">
                      <span class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4 text-slate-400 transition-colors group-focus-within:text-accent">
                        <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                          <path d="M2.94 5.5A2.5 2.5 0 0 1 5.1 4.25h9.8a2.5 2.5 0 0 1 2.16 1.25L10 9.91 2.94 5.5Zm-.44 1.8v7.6a2.5 2.5 0 0 0 2.5 2.5h10a2.5 2.5 0 0 0 2.5-2.5V7.3l-6.8 4.1a1.35 1.35 0 0 1-1.4 0L2.5 7.3Z"/>
                        </svg>
                      </span>
                      <input
                        id="signup-email"
                        type="email"
                        formControlName="email"
                        autocomplete="email"
                        placeholder="you@example.com"
                        [readonly]="otpSent"
                        class="w-full rounded-2xl border border-slate-300 bg-white py-3.5 pl-12 pr-4 text-sm text-slate-900 placeholder-slate-400 transition-all duration-200 focus:border-accent/50 focus:outline-none focus:ring-4 focus:ring-accent/15"
                        [class.cursor-not-allowed]="otpSent"
                        [class.bg-slate-100]="otpSent"
                        [class.text-slate-500]="otpSent"
                        [class.border-rose-500]="submitted && form.controls.email.invalid"
                        [attr.aria-invalid]="submitted && form.controls.email.invalid"
                        (input)="onIdentityChanged()"
                      />
                    </div>
                    @if (submitted && form.controls.email.invalid) {
                      <p class="text-sm text-rose-400">Valid email is required.</p>
                    }
                  </div>

                  <div class="space-y-2">
                    <label for="signup-phone" class="block text-sm font-medium text-slate-700">Phone number</label>
                    <div class="group relative">
                      <span class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4 text-slate-400 transition-colors group-focus-within:text-accent">
                        <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                          <path d="M2.5 4.75A2.25 2.25 0 0 1 4.75 2.5h1.46c.43 0 .81.29.92.7l.66 2.46a1 1 0 0 1-.26.98l-1.1 1.1a11.04 11.04 0 0 0 4.83 4.83l1.1-1.1a1 1 0 0 1 .98-.26l2.46.66c.41.11.7.49.7.92v1.46a2.25 2.25 0 0 1-2.25 2.25h-.5C7.82 17.5 2.5 12.18 2.5 5.25v-.5Z"/>
                        </svg>
                      </span>
                      <input
                        id="signup-phone"
                        type="tel"
                        formControlName="phone"
                        autocomplete="tel"
                        placeholder="9876543210"
                        maxlength="10"
                        [readonly]="otpSent"
                        class="w-full rounded-2xl border border-slate-300 bg-white py-3.5 pl-12 pr-4 text-sm text-slate-900 placeholder-slate-400 transition-all duration-200 focus:border-accent/50 focus:outline-none focus:ring-4 focus:ring-accent/15"
                        [class.cursor-not-allowed]="otpSent"
                        [class.bg-slate-100]="otpSent"
                        [class.text-slate-500]="otpSent"
                        [class.border-rose-500]="submitted && form.controls.phone.invalid"
                        [attr.aria-invalid]="submitted && form.controls.phone.invalid"
                        (input)="onIdentityChanged()"
                      />
                    </div>
                    @if (submitted && form.controls.phone.invalid) {
                      <p class="text-sm text-rose-400">Enter a valid 10-digit phone number.</p>
                    }
                  </div>

                  <div class="space-y-2">
                    <label for="signup-password" class="block text-sm font-medium text-slate-700">Password</label>
                    <div class="group relative">
                      <span class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4 text-slate-400 transition-colors group-focus-within:text-accent">
                        <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                          <path fill-rule="evenodd" d="M10 2.75a4.25 4.25 0 0 0-4.25 4.25v1H5A2.25 2.25 0 0 0 2.75 10.25v5A2.25 2.25 0 0 0 5 17.5h10a2.25 2.25 0 0 0 2.25-2.25v-5A2.25 2.25 0 0 0 15 8h-.75V7A4.25 4.25 0 0 0 10 2.75ZM12.75 8V7a2.75 2.75 0 1 0-5.5 0v1h5.5Z" clip-rule="evenodd"/>
                        </svg>
                      </span>
                      <input
                        id="signup-password"
                        [type]="showPassword ? 'text' : 'password'"
                        formControlName="password"
                        autocomplete="new-password"
                        placeholder="At least 8 characters"
                        [readonly]="otpSent"
                        class="w-full rounded-2xl border border-slate-300 bg-white py-3.5 pl-12 pr-14 text-sm text-slate-900 placeholder-slate-400 transition-all duration-200 focus:border-accent/50 focus:outline-none focus:ring-4 focus:ring-accent/15"
                        [class.cursor-not-allowed]="otpSent"
                        [class.bg-slate-100]="otpSent"
                        [class.text-slate-500]="otpSent"
                        [class.border-rose-500]="submitted && form.controls.password.invalid"
                        [attr.aria-invalid]="submitted && form.controls.password.invalid"
                        (input)="onIdentityChanged()"
                      />
                      <button
                        type="button"
                        (click)="togglePasswordVisibility()"
                        class="absolute inset-y-0 right-0 flex items-center pr-4 text-sm font-medium text-slate-500 transition-colors hover:text-slate-800 focus:outline-none"
                        [attr.aria-label]="showPassword ? 'Hide password' : 'Show password'"
                      >
                        {{ showPassword ? 'Hide' : 'Show' }}
                      </button>
                    </div>
                    @if (submitted && form.controls.password.invalid) {
                      <p class="text-sm text-rose-400">Password must be at least 8 characters.</p>
                    }
                  </div>

                  @if (otpSent) {
                    <div class="space-y-2">
                      <label for="signup-otp" class="block text-sm font-medium text-slate-700">Email OTP</label>
                      <input
                        id="signup-otp"
                        type="text"
                        formControlName="otpCode"
                        inputmode="numeric"
                        autocomplete="one-time-code"
                        placeholder="Enter 6-digit OTP"
                        maxlength="6"
                        class="w-full rounded-2xl border border-slate-300 bg-white px-4 py-3.5 text-center text-sm tracking-[0.3em] text-slate-900 placeholder-slate-400 transition-all duration-200 focus:border-accent/50 focus:outline-none focus:ring-4 focus:ring-accent/15"
                        [class.border-rose-500]="submitted && otpSent && !otpVerified && form.controls.otpCode.invalid"
                        [attr.aria-invalid]="submitted && otpSent && !otpVerified && form.controls.otpCode.invalid"
                      />
                      @if (submitted && otpSent && !otpVerified && form.controls.otpCode.invalid) {
                        <p class="text-sm text-rose-400">Enter the 6-digit OTP sent to your email.</p>
                      }
                    </div>
                  }

                  @if (otpSent && !otpVerified) {
                    <div class="flex items-center justify-between gap-3 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3">
                      <button
                        type="button"
                        class="text-sm font-medium text-slate-600 transition-colors hover:text-slate-900"
                        (click)="resetOtpFlow()"
                      >
                        Edit details
                      </button>
                      <button
                        type="button"
                        class="text-sm font-semibold text-accent transition-colors hover:text-accent-hover disabled:opacity-60"
                        [disabled]="loading"
                        (click)="resendOtp()"
                      >
                        Resend OTP
                      </button>
                    </div>
                  }

                  <button
                    type="submit"
                    [disabled]="loading"
                    class="inline-flex w-full items-center justify-center rounded-2xl bg-accent px-4 py-3.5 text-sm font-semibold text-white shadow-lg shadow-blue-500/20 transition-all duration-200 hover:-translate-y-0.5 hover:bg-accent-hover hover:shadow-xl hover:shadow-blue-500/20 disabled:translate-y-0 disabled:opacity-60"
                  >
                    {{
                      loading
                        ? otpVerified
                          ? 'Creating account...'
                          : otpSent
                            ? 'Verifying OTP...'
                            : 'Sending OTP...'
                        : otpVerified
                          ? 'Create Account'
                          : otpSent
                            ? 'Verify OTP'
                            : 'Send OTP'
                    }}
                  </button>
                </form>

                <div class="mt-6 rounded-2xl border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
                  <p class="font-medium text-slate-800">Already registered?</p>
                  <p class="mt-1">
                    Sign in with your existing credentials and continue from your dashboard.
                    <a routerLink="/login" class="ml-1 font-semibold text-accent transition-colors hover:text-accent-hover">Sign in</a>
                  </p>
                </div>
              </div>
            </div>
          </section>
        </div>
      </div>
    </div>
  `,
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

  onSubmit(): void {
    this.submitted = true;
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

  resendOtp(): void {
    this.submitted = true;
    this.sendOtp();
  }

  resetOtpFlow(): void {
    this.otpSent = false;
    this.otpVerified = false;
    this.form.controls.otpCode.setValue('');
    this.form.controls.otpCode.markAsPristine();
    this.form.controls.otpCode.markAsUntouched();
  }

  onIdentityChanged(): void {
    if (this.otpSent) {
      this.resetOtpFlow();
    }
  }

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

  private verifyOtp(): void {
    if (this.form.controls.otpCode.invalid || !this.form.controls.otpCode.getRawValue()) {
      return;
    }

    this.loading = true;
    this.authService
      .verifyOtp({
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
        email: this.form.controls.email.getRawValue(),
        phone: this.form.controls.phone.getRawValue(),
        password: this.form.controls.password.getRawValue(),
      })
      .subscribe({
        next: (res) => {
          this.tokenService.setTokens(res);
          this.toastService.success('Account created! Welcome aboard.');
          this.loading = false;
          this.router.navigate(['/dashboard/overview']);
        },
        error: (err: { message?: string }) => {
          this.toastService.error(err.message ?? 'Registration failed');
          this.loading = false;
        },
      });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
