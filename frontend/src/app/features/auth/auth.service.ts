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

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/signup', payload);
  }

  sendOtp(payload: OtpSendRequest): Observable<{ message: string }> {
    return this.api.post<{ message: string }>('/api/auth/otp/send', payload);
  }

  verifyOtp(payload: OtpVerifyRequest): Observable<string> {
    return this.api.post<string>('/api/auth/otp/verify', payload);
  }

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/login', payload);
  }

  kycSubmit(payload: KycSubmitRequest): Observable<KycInfo> {
    return this.api.post<KycInfo>('/api/auth/kyc/submit', payload);
  }

  getKycStatus(): Observable<KycInfo[]> {
    return this.api.get<KycInfo[]>('/api/auth/kyc/status');
  }
}
