import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface ApiResponse<T> {
  success: boolean;
  code: string;
  message?: string | null;
  data?: T;
  errors?: Record<string, string[]>;
}

export interface SemanticField {
  key: string;
  label: string;
  type: string;
  filterable: boolean;
  enumValues?: string[];
}

export interface SemanticDataset {
  code: string;
  name: string;
  category: string;
  description?: string | null;
  fields: SemanticField[];
}

export interface ReportCatalog {
  dataSources: ReportDataSource[];
}

export interface ReportDataSource {
  id: string;
  name: string;
  category: string;
  description?: string | null;
  fields: ReportField[];
}

export interface ReportField {
  id: string;
  name: string;
  type: string;
  canFilter: boolean;
  options?: string[] | null;
}

export interface ReportFilter {
  fieldName: string;
  label: string;
  filterType: string;
  required: boolean;
  defaultValue?: string | null;
  dataSourceUrl?: string | null;
}

export interface ReportConfigurationInput {
  code: string;
  name: string;
  datasetCode: string;
  selectedFields: string[];
  filters: ReportFilter[];
  customTemplateContent?: string | null;
}

export interface CreateReportInput {
  name: string;
  dataSourceId: string;
  columns: string[];
  filters: Array<{
    field: string;
    type: string;
    label?: string;
    required: boolean;
    defaultValue?: string | null;
  }>;
  code?: string;
}

export interface CreateReportResponse {
  code: string;
  name: string;
  dataSourceId: string;
}

export interface ExportReportInput {
  format: 'pdf' | 'html';
  filters: Record<string, unknown>;
}

export interface ExecuteReportInput {
  criteria: Record<string, unknown>;
  format: number;
}

@Injectable({ providedIn: 'root' })
export class ReportApiService {
  private readonly http = inject(HttpClient);
  private baseUrl = 'http://localhost:5000';
  private accessToken = '';

  configure(baseUrl: string, accessToken: string): void {
    this.baseUrl = baseUrl.replace(/\/+$/, '');
    this.accessToken = accessToken.trim();
  }

  get configured(): boolean {
    return this.accessToken.length > 0;
  }

  get datasets(): Observable<ApiResponse<SemanticDataset[]>> {
    return this.http.get<ApiResponse<SemanticDataset[]>>(
      `${this.baseUrl}/api/reports/semantic-datasets`,
      { headers: this.headers() }
    );
  }

  get catalog(): Observable<ApiResponse<ReportCatalog>> {
    return this.http.get<ApiResponse<ReportCatalog>>(
      `${this.baseUrl}/api/reports/catalog`,
      { headers: this.headers() }
    );
  }

  createReport(input: CreateReportInput): Observable<ApiResponse<CreateReportResponse>> {
    return this.http.post<ApiResponse<CreateReportResponse>>(
      `${this.baseUrl}/api/reports`,
      input,
      { headers: this.headers(this.idempotencyKey('report-create')) }
    );
  }

  exportReport(code: string, input: ExportReportInput): Observable<HttpResponse<Blob>> {
    return this.http.post(`${this.baseUrl}/api/reports/${encodeURIComponent(code)}/export`, input, {
      headers: this.headers(this.idempotencyKey('report-export')),
      observe: 'response',
      responseType: 'blob'
    });
  }

  saveConfiguration(input: ReportConfigurationInput): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(
      `${this.baseUrl}/api/reports/configurations`,
      input,
      { headers: this.headers(this.idempotencyKey('report-config')) }
    );
  }

  validateTemplate(content: string): Observable<ApiResponse<{ isValid: boolean; errorMessage?: string }>> {
    return this.http.post<ApiResponse<{ isValid: boolean; errorMessage?: string }>>(
      `${this.baseUrl}/api/reports/templates/validate`,
      { templateContent: content },
      { headers: this.headers(this.idempotencyKey('template-validate')) }
    );
  }

  executeConfiguration(code: string, input: ExecuteReportInput): Observable<HttpResponse<Blob>> {
    return this.http.post(`${this.baseUrl}/api/reports/configurations/${encodeURIComponent(code)}/execute`, input, {
      headers: this.headers(this.idempotencyKey('report-execute')),
      observe: 'response',
      responseType: 'blob'
    });
  }

  private headers(idempotencyKey?: string): HttpHeaders {
    let headers = new HttpHeaders({
      Accept: 'application/json',
      'X-Correlation-Id': this.correlationId()
    });

    if (this.accessToken) {
      headers = headers.set('Authorization', `Bearer ${this.accessToken}`);
    }

    if (idempotencyKey) {
      headers = headers.set('Idempotency-Key', idempotencyKey);
    }

    return headers;
  }

  private idempotencyKey(prefix: string): string {
    return `${prefix}-${this.correlationId()}`;
  }

  private correlationId(): string {
    return globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(16).slice(2)}`;
  }
}
