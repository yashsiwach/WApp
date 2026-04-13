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
  template: `
    <div class="relative min-h-screen overflow-hidden bg-surface-950 text-slate-900">
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.2),_transparent_34%),radial-gradient(circle_at_bottom_right,_rgba(14,165,233,0.14),_transparent_30%),linear-gradient(180deg,_rgba(248,250,255,0.98),_rgba(238,244,255,1))]"></div>
      <div class="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-blue-400/70 to-transparent"></div>

      <div class="relative mx-auto flex min-h-screen max-w-7xl items-center px-4 py-8 sm:px-6 lg:px-8">
        <div class="grid w-full gap-8 lg:grid-cols-[1.05fr_0.95fr] lg:items-center">
          <section class="hidden lg:block">
            <div class="max-w-xl">
              <span class="inline-flex items-center rounded-full border border-accent/20 bg-accent/10 px-4 py-1 text-xs font-semibold uppercase tracking-[0.24em] text-accent">
                Welcome Back
              </span>
              <h1 class="mt-6 font-display text-5xl font-bold leading-tight text-slate-900">
                Your wallet, rewards, and support dashboard in one calm flow.
              </h1>
              <p class="mt-5 max-w-lg text-base leading-7 text-slate-600">
                Sign in to track balances, review KYC, redeem rewards, and keep every transaction within reach.
              </p>

              <div class="mt-10 grid gap-4 sm:grid-cols-3">
                <div class="rounded-2xl border border-slate-200 bg-white/85 p-4 shadow-[0_18px_40px_rgba(37,99,235,0.12)] backdrop-blur-sm">
                  <p class="text-sm font-semibold text-slate-900">Fast Access</p>
                  <p class="mt-2 text-sm text-slate-600">Reach wallet actions, support, and rewards from one secure place.</p>
                </div>
                <div class="rounded-2xl border border-slate-200 bg-white/85 p-4 shadow-[0_18px_40px_rgba(37,99,235,0.12)] backdrop-blur-sm">
                  <p class="text-sm font-semibold text-slate-900">Secure Sign-In</p>
                  <p class="mt-2 text-sm text-slate-600">Built around your existing auth and token flow without extra steps.</p>
                </div>
                <div class="rounded-2xl border border-slate-200 bg-white/85 p-4 shadow-[0_18px_40px_rgba(37,99,235,0.12)] backdrop-blur-sm">
                  <p class="text-sm font-semibold text-slate-900">Rewards Ready</p>
                  <p class="mt-2 text-sm text-slate-600">Keep points and redemptions close to your main workflow.</p>
                </div>
              </div>
            </div>
          </section>

          <section class="mx-auto w-full max-w-md lg:max-w-lg">
            <div class="overflow-hidden rounded-[28px] border border-slate-200 bg-white/95 shadow-[0_28px_70px_rgba(37,99,235,0.18)] backdrop-blur-xl">
              <div class="border-b border-slate-200 bg-slate-50/90 px-6 py-6 sm:px-8">
                <div class="flex items-start justify-between gap-4">
                  <div>
                    <span class="inline-flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-600 via-sky-500 to-cyan-500 text-xl font-bold text-white shadow-lg shadow-blue-500/30">
                      DW
                    </span>
                    <h2 class="mt-4 font-display text-3xl font-bold text-slate-900">Sign in</h2>
                    <p class="mt-2 text-sm leading-6 text-slate-600">
                      Pick up where you left off with your DigitalWallet account.
                    </p>
                  </div>
                  <a
                    routerLink="/signup"
                    class="hidden shrink-0 whitespace-nowrap rounded-full border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition-all duration-200 hover:border-accent/40 hover:text-accent sm:inline-flex"
                  >
                    Create account
                  </a>
                </div>
              </div>

              <div class="px-6 py-6 sm:px-8 sm:py-8">
                <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate class="space-y-5">
                  <div class="space-y-2">
                    <label for="login-email" class="block text-sm font-medium text-slate-700">Email address</label>
                    <div class="group relative">
                      <span class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4 text-slate-400 transition-colors group-focus-within:text-accent">
                        <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                          <path d="M2.94 5.5A2.5 2.5 0 0 1 5.1 4.25h9.8a2.5 2.5 0 0 1 2.16 1.25L10 9.91 2.94 5.5Zm-.44 1.8v7.6a2.5 2.5 0 0 0 2.5 2.5h10a2.5 2.5 0 0 0 2.5-2.5V7.3l-6.8 4.1a1.35 1.35 0 0 1-1.4 0L2.5 7.3Z"/>
                        </svg>
                      </span>
                      <input
                        id="login-email"
                        type="email"
                        formControlName="email"
                        autocomplete="email"
                        placeholder="you@example.com"
                        class="w-full rounded-2xl border border-slate-300 bg-white py-3.5 pl-12 pr-4 text-sm text-slate-900 placeholder-slate-400 transition-all duration-200 focus:border-accent/50 focus:outline-none focus:ring-4 focus:ring-accent/15"
                        [class.border-rose-500]="submitted && form.controls.email.invalid"
                        [attr.aria-invalid]="submitted && form.controls.email.invalid"
                      />
                    </div>
                    @if (submitted && form.controls.email.invalid) {
                      <p class="text-sm text-rose-400">Valid email is required.</p>
                    }
                  </div>

                  <div class="space-y-2">
                    <div class="flex items-center justify-between gap-3">
                      <label for="login-password" class="block text-sm font-medium text-slate-700">Password</label>
                    </div>
                    <div class="group relative">
                      <span class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4 text-slate-400 transition-colors group-focus-within:text-accent">
                        <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                          <path fill-rule="evenodd" d="M10 2.75a4.25 4.25 0 0 0-4.25 4.25v1H5A2.25 2.25 0 0 0 2.75 10.25v5A2.25 2.25 0 0 0 5 17.5h10a2.25 2.25 0 0 0 2.25-2.25v-5A2.25 2.25 0 0 0 15 8h-.75V7A4.25 4.25 0 0 0 10 2.75ZM12.75 8V7a2.75 2.75 0 1 0-5.5 0v1h5.5Z" clip-rule="evenodd"/>
                        </svg>
                      </span>
                      <input
                        id="login-password"
                        [type]="showPassword ? 'text' : 'password'"
                        formControlName="password"
                        autocomplete="current-password"
                        placeholder="Enter your password"
                        class="w-full rounded-2xl border border-slate-300 bg-white py-3.5 pl-12 pr-14 text-sm text-slate-900 placeholder-slate-400 transition-all duration-200 focus:border-accent/50 focus:outline-none focus:ring-4 focus:ring-accent/15"
                        [class.border-rose-500]="submitted && form.controls.password.invalid"
                        [attr.aria-invalid]="submitted && form.controls.password.invalid"
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
                      <p class="text-sm text-rose-400">Password is required.</p>
                    }
                  </div>

                  <button
                    type="submit"
                    [disabled]="loading"
                    class="inline-flex w-full items-center justify-center rounded-2xl bg-accent px-4 py-3.5 text-sm font-semibold text-white shadow-lg shadow-blue-500/20 transition-all duration-200 hover:-translate-y-0.5 hover:bg-accent-hover hover:shadow-xl hover:shadow-blue-500/20 disabled:translate-y-0 disabled:opacity-60"
                  >
                    {{ loading ? 'Signing in...' : 'Sign In' }}
                  </button>
                </form>

                <div class="mt-6 rounded-2xl border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
                  <p class="font-medium text-slate-800">Need a new account?</p>
                  <p class="mt-1">
                    Register with email OTP verification and jump straight into your dashboard.
                    <a routerLink="/signup" class="ml-1 font-semibold text-accent transition-colors hover:text-accent-hover">Sign up</a>
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

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
