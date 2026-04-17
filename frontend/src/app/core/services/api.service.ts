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

  // Send a GET request and normalize the backend envelope into the requested payload type.
  get<T>(path: string, queryParams?: Record<string, unknown>): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(this.buildUrl(path), {
        params: this.buildParams(queryParams),
      })
      .pipe(map((response) => this.unwrap(response)), catchError((error) => this.mapError(error)));
  }

  // Send a POST request and unwrap the standard API response shape.
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

  // Send a PUT request for full resource updates using the shared response/error handling.
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

  // Send a PATCH request for partial updates while keeping error mapping consistent.
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

  // Send a DELETE request and return the unwrapped response body.
  delete<T>(path: string, queryParams?: Record<string, unknown>): Observable<T> {
    return this.http
      .delete<ApiResponse<T>>(this.buildUrl(path), {
        params: this.buildParams(queryParams),
      })
      .pipe(map((response) => this.unwrap(response)), catchError((error) => this.mapError(error)));
  }

  // Prefix relative paths with the configured API base URL.
  private buildUrl(path: string): string {
    return `${this.baseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  }

  // Drop empty query values so the backend only receives meaningful filters.
  private buildParams(queryParams?: Record<string, unknown>): HttpParams {
    let params = new HttpParams();

    if (!queryParams) {
      return params;
    }

    Object.entries(queryParams).forEach(([key, value]) => {
      // Skip empty values so optional filters do not produce noisy query strings.
      if (value === null || value === undefined || value === '') {
        return;
      }

      // Convert all supported filter values to strings because HttpParams is string-based.
      params = params.set(key, String(value));
    });

    return params;
  }

  // Convert the shared `{ success, data }` envelope into the raw payload or a typed API error.
  private unwrap<T>(response: ApiResponse<T>): T {
    if (response.success) {
      // Successful responses always carry the typed payload in `data`.
      return response.data;
    }

    // Re-throw API envelope failures in the same shape used for transport errors.
    const apiError: ApiError = {
      status: 400,
      message: response.message || 'Request failed.',
      errors: response.errors ?? [],
    };

    throw apiError;
  }

  // Normalize transport and server failures into one frontend-friendly error shape.
  private mapError(error: unknown): Observable<never> {
    if (error instanceof HttpErrorResponse) {
      // Preserve backend validation messages when the server responded with a structured error body.
      const payload = error.error as Partial<ApiResponse<unknown>> | null;
      const mappedError: ApiError = {
        status: error.status,
        message: payload?.message ?? error.message ?? 'Unexpected server error.',
        errors: payload?.errors ?? [],
      };

      return throwError(() => mappedError);
    }

    if (typeof error === 'object' && error !== null && 'message' in error) {
      // Already-normalized API errors can be forwarded without reshaping them again.
      return throwError(() => error as ApiError);
    }

    // Fall back to a generic client-side error for unexpected thrown values.
    return throwError(() => ({
      status: 0,
      message: 'Unexpected client error.',
      errors: [],
    }) as ApiError);
  }
}
