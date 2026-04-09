import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';
import { ApiError } from './api-error.model';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  get<T>(path: string, queryParams?: Record<string, unknown>): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(this.buildUrl(path), {
        params: this.buildParams(queryParams),
      })
      .pipe(map((response) => this.unwrap(response)), catchError((error) => this.mapError(error)));
  }

  post<T>(
    path: string,
    body?: unknown,
    queryParams?: Record<string, unknown>,
  ): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(this.buildUrl(path), body, {
        params: this.buildParams(queryParams),
      })
      .pipe(map((response) => this.unwrap(response)), catchError((error) => this.mapError(error)));
  }

  put<T>(
    path: string,
    body?: unknown,
    queryParams?: Record<string, unknown>,
  ): Observable<T> {
    return this.http
      .put<ApiResponse<T>>(this.buildUrl(path), body, {
        params: this.buildParams(queryParams),
      })
      .pipe(map((response) => this.unwrap(response)), catchError((error) => this.mapError(error)));
  }

  patch<T>(
    path: string,
    body?: unknown,
    queryParams?: Record<string, unknown>,
  ): Observable<T> {
    return this.http
      .patch<ApiResponse<T>>(this.buildUrl(path), body, {
        params: this.buildParams(queryParams),
      })
      .pipe(map((response) => this.unwrap(response)), catchError((error) => this.mapError(error)));
  }

  delete<T>(path: string, queryParams?: Record<string, unknown>): Observable<T> {
    return this.http
      .delete<ApiResponse<T>>(this.buildUrl(path), {
        params: this.buildParams(queryParams),
      })
      .pipe(map((response) => this.unwrap(response)), catchError((error) => this.mapError(error)));
  }

  private buildUrl(path: string): string {
    return `${this.baseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  }

  private buildParams(queryParams?: Record<string, unknown>): HttpParams {
    let params = new HttpParams();

    if (!queryParams) {
      return params;
    }

    Object.entries(queryParams).forEach(([key, value]) => {
      if (value === null || value === undefined || value === '') {
        return;
      }

      params = params.set(key, String(value));
    });

    return params;
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (response.success) {
      return response.data;
    }

    const apiError: ApiError = {
      status: 400,
      message: response.message || 'Request failed.',
      errors: response.errors ?? [],
    };

    throw apiError;
  }

  private mapError(error: unknown): Observable<never> {
    if (error instanceof HttpErrorResponse) {
      const payload = error.error as Partial<ApiResponse<unknown>> | null;
      const mappedError: ApiError = {
        status: error.status,
        message: payload?.message ?? error.message ?? 'Unexpected server error.',
        errors: payload?.errors ?? [],
      };

      return throwError(() => mappedError);
    }

    if (typeof error === 'object' && error !== null && 'message' in error) {
      return throwError(() => error as ApiError);
    }

    return throwError(() => ({
      status: 0,
      message: 'Unexpected client error.',
      errors: [],
    }) as ApiError);
  }
}
