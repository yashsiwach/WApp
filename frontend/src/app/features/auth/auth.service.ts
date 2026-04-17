import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import { AuthResponse, KycInfo, LoginRequest, RegisterRequest } from '../../shared/models/auth.model';

export interface KycSubmitRequest {
  docType: string;
  fileUrl: string;
}

export interface OtpSendRequest {
  email: string;
}

export interface OtpVerifyRequest {
  email: string;
  code: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private readonly api: ApiService) {}

  // Create a new user account after the OTP flow has been completed.
  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/signup', payload);
  }

  // Request a verification code for the entered email address.
  sendOtp(payload: OtpSendRequest): Observable<{ message: string }> {
    return this.api.post<{ message: string }>('/api/auth/otp/send', payload);
  }

  // Confirm the OTP before allowing the signup flow to continue.
  verifyOtp(payload: OtpVerifyRequest): Observable<string> {
    return this.api.post<string>('/api/auth/otp/verify', payload);
  }

  // Authenticate an existing user and return their tokens/profile.
  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/login', payload);
  }

  // Submit a KYC document reference for manual review.
  kycSubmit(payload: KycSubmitRequest): Observable<KycInfo> {
    return this.api.post<KycInfo>('/api/auth/kyc/submit', payload);
  }

  // Load all KYC submissions so the UI can display the latest review status.
  getKycStatus(): Observable<KycInfo[]> {
    return this.api.get<KycInfo[]>('/api/auth/kyc/status');
  }
}
