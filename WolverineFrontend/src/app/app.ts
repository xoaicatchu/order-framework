import { TitleCasePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import {
  ReportApiService,
  ReportConfigurationInput,
  ReportFilter,
  SemanticDataset
} from './core/report-api.service';

type ViewKey = 'overview' | 'builder' | 'templates' | 'datasets';
type ConnectionState = 'demo' | 'connecting' | 'connected' | 'error';

interface RecentReport {
  name: string;
  dataset: string;
  format: string;
  status: 'Ready' | 'Draft' | 'Running';
  updated: string;
  accent: string;
}

@Component({
  selector: 'app-root',
  imports: [FormsModule, TitleCasePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly api = inject(ReportApiService);

  activeView: ViewKey = 'overview';
  connectionState: ConnectionState = 'demo';
  toastMessage = '';
  toastTone: 'success' | 'error' | 'info' = 'info';
  showConnectionPanel = false;

  apiBaseUrl = 'http://localhost:5000';
  accessToken = '';

  datasets: SemanticDataset[] = this.demoDatasets();
  selectedDatasetCode = 'orders';
  selectedFields = ['orderNumber', 'customerName', 'totalAmount'];
  reportCode = 'monthly-orders';
  reportName = 'Monthly orders';
  reportDescription = 'Order activity and revenue overview';
  outputFormat: 'Pdf' | 'Html' = 'Pdf';
  templateContent = `<section class="report">
  <h1>{{ ReportName }}</h1>
  <p>Generated at {{ ExecutedAt }}</p>
  {% for row in Data %}
    <div>{{ row.orderNumber }} · {{ row.customerName }} · {{ row.totalAmount | format_currency: 'VND' }}</div>
  {% endfor %}
</section>`;

  filters: ReportFilter[] = [
    {
      fieldName: 'createdAt',
      label: 'Created date',
      filterType: 'date_range',
      required: true,
      defaultValue: null
    }
  ];

  filterValues: Record<string, string> = {
    createdAt_from: '2026-08-01',
    createdAt_to: '2026-08-31'
  };

  recentReports: RecentReport[] = [
    { name: 'Monthly orders', dataset: 'Orders', format: 'PDF', status: 'Ready', updated: '12 min ago', accent: 'violet' },
    { name: 'Revenue by customer', dataset: 'Orders', format: 'HTML', status: 'Draft', updated: 'Yesterday', accent: 'blue' },
    { name: 'Operations snapshot', dataset: 'Order statistics', format: 'PDF', status: 'Running', updated: 'Yesterday', accent: 'amber' },
    { name: 'Audit activity', dataset: 'Audit logs', format: 'HTML', status: 'Ready', updated: '3 days ago', accent: 'green' }
  ];

  get selectedDataset(): SemanticDataset {
    return this.datasets.find((dataset) => dataset.code === this.selectedDatasetCode) ?? this.datasets[0] ?? {
      code: 'empty',
      name: 'No dataset selected',
      category: 'Unavailable',
      description: 'Connect the API to load the semantic catalog.',
      fields: []
    };
  }

  get selectedFieldDefinitions() {
    return this.selectedDataset.fields.filter((field) => this.selectedFields.includes(field.key));
  }

  get statusLabel(): string {
    return {
      demo: 'Demo mode',
      connecting: 'Connecting',
      connected: 'API connected',
      error: 'Offline fallback'
    }[this.connectionState];
  }

  setView(view: ViewKey): void {
    this.activeView = view;
  }

  openBuilder(): void {
    this.activeView = 'builder';
    this.showConnectionPanel = false;
  }

  selectDataset(code: string): void {
    this.selectedDatasetCode = code;
    const available = this.datasets.find((dataset) => dataset.code === code)?.fields ?? [];
    this.selectedFields = available.slice(0, 3).map((field) => field.key);
    this.filters = available.filter((field) => field.filterable).slice(0, 1).map((field) => ({
      fieldName: field.key,
      label: field.label,
      filterType: field.type === 'date' ? 'date' : 'text',
      required: false,
      defaultValue: null
    }));
  }

  toggleField(key: string): void {
    this.selectedFields = this.selectedFields.includes(key)
      ? this.selectedFields.filter((field) => field !== key)
      : [...this.selectedFields, key];
  }

  addFilter(): void {
    const candidate = this.selectedDataset.fields.find(
      (field) => field.filterable && !this.filters.some((filter) => filter.fieldName === field.key)
    );

    if (!candidate) {
      this.notify('No additional filterable fields are available.', 'info');
      return;
    }

    this.filters = [...this.filters, {
      fieldName: candidate.key,
      label: candidate.label,
      filterType: candidate.type === 'date' ? 'date' : 'text',
      required: false,
      defaultValue: null
    }];
  }

  removeFilter(index: number): void {
    this.filters = this.filters.filter((_, currentIndex) => currentIndex !== index);
  }

  async connectApi(): Promise<void> {
    if (!this.accessToken.trim()) {
      this.notify('Paste an access token from your Identity Provider first.', 'error');
      return;
    }

    this.connectionState = 'connecting';
    this.api.configure(this.apiBaseUrl, this.accessToken);

    try {
      const response = await firstValueFrom(this.api.datasets);
      if (!response.success || !response.data) {
        throw new Error(response.message ?? 'The API did not return datasets.');
      }
      this.datasets = response.data;
      this.selectedDatasetCode = this.datasets[0]?.code ?? this.selectedDatasetCode;
      this.connectionState = 'connected';
      this.notify('Connected. Dataset catalog is up to date.', 'success');
    } catch (error) {
      this.connectionState = 'error';
      this.notify(error instanceof Error ? error.message : 'API connection failed. Demo data is still available.', 'error');
    }
  }

  async saveConfiguration(): Promise<void> {
    if (!this.reportCode.trim() || !this.reportName.trim()) {
      this.notify('Report code and name are required.', 'error');
      return;
    }

    const input: ReportConfigurationInput = {
      code: this.reportCode.trim(),
      name: this.reportName.trim(),
      datasetCode: this.selectedDatasetCode,
      selectedFields: this.selectedFields,
      filters: this.filters,
      customTemplateContent: this.templateContent
    };

    if (!this.api.configured) {
      this.upsertRecentReport('Draft');
      this.notify('Saved in demo mode. Connect the API to persist this configuration.', 'info');
      return;
    }

    try {
      const response = await firstValueFrom(this.api.saveConfiguration(input));
      if (!response.success) {
        throw new Error(response.message ?? 'The report configuration could not be saved.');
      }
      this.upsertRecentReport('Ready');
      this.notify('Report configuration saved to the tenant workspace.', 'success');
    } catch (error) {
      this.notify(error instanceof Error ? error.message : 'Save failed.', 'error');
    }
  }

  async validateTemplate(): Promise<void> {
    if (!this.api.configured) {
      const valid = this.templateContent.trim().length > 0 && !/<script|javascript:/i.test(this.templateContent);
      this.notify(valid ? 'Template syntax looks valid in demo mode.' : 'Template contains invalid or unsafe content.', valid ? 'success' : 'error');
      return;
    }

    try {
      const response = await firstValueFrom(this.api.validateTemplate(this.templateContent));
      const result = response.data;
      this.notify(result?.isValid ? 'Template validated successfully.' : result?.errorMessage ?? 'Template is invalid.', result?.isValid ? 'success' : 'error');
    } catch (error) {
      this.notify(error instanceof Error ? error.message : 'Template validation failed.', 'error');
    }
  }

  async exportReport(): Promise<void> {
    const criteria = Object.fromEntries(
      Object.entries(this.filterValues).filter(([, value]) => value.trim().length > 0)
    );

    if (!this.api.configured) {
      const demoContent = `<html><body><h1>${this.reportName}</h1><p>Demo export · ${new Date().toLocaleString()}</p><p>Connect the API to render the live tenant dataset.</p></body></html>`;
      this.download(new Blob([demoContent], { type: 'text/html' }), `${this.reportCode}-demo.html`);
      this.notify('Demo HTML export downloaded. Connect the API for a live export.', 'info');
      return;
    }

    try {
      const response = await firstValueFrom(this.api.executeConfiguration(this.reportCode, {
        criteria,
        format: this.outputFormat === 'Pdf' ? 0 : 1
      }));
      const contentType = response.headers.get('content-type') ?? (this.outputFormat === 'Pdf' ? 'application/pdf' : 'text/html');
      const blob = response.body ?? new Blob([], { type: contentType });
      this.download(blob, `${this.reportCode}.${this.outputFormat === 'Pdf' ? 'pdf' : 'html'}`);
      this.notify('Report export completed.', 'success');
    } catch (error) {
      this.notify(error instanceof Error ? error.message : 'Export failed.', 'error');
    }
  }

  private upsertRecentReport(status: RecentReport['status']): void {
    this.recentReports = [
      {
        name: this.reportName,
        dataset: this.selectedDataset.name,
        format: this.outputFormat.toUpperCase(),
        status,
        updated: 'Just now',
        accent: 'violet'
      },
      ...this.recentReports.filter((report) => report.name !== this.reportName)
    ];
  }

  private download(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }

  private notify(message: string, tone: App['toastTone']): void {
    this.toastMessage = message;
    this.toastTone = tone;
    window.setTimeout(() => {
      if (this.toastMessage === message) {
        this.toastMessage = '';
      }
    }, 4200);
  }

  private demoDatasets(): SemanticDataset[] {
    return [
      {
        code: 'orders',
        name: 'Orders',
        category: 'Sales',
        description: 'Orders, customers and revenue fields scoped to the active tenant.',
        fields: [
          { key: 'orderNumber', label: 'Order number', type: 'string', filterable: true },
          { key: 'customerName', label: 'Customer', type: 'string', filterable: true },
          { key: 'totalAmount', label: 'Total amount', type: 'currency', filterable: true },
          { key: 'status', label: 'Status', type: 'enum', filterable: true, enumValues: ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled'] },
          { key: 'createdAt', label: 'Created date', type: 'date', filterable: true }
        ]
      },
      {
        code: 'order-statistics',
        name: 'Order statistics',
        category: 'Operations',
        description: 'Aggregated order volume and status distribution.',
        fields: [
          { key: 'status', label: 'Status', type: 'enum', filterable: true },
          { key: 'orderCount', label: 'Order count', type: 'number', filterable: false },
          { key: 'totalRevenue', label: 'Revenue', type: 'currency', filterable: false }
        ]
      },
      {
        code: 'audit-logs',
        name: 'Audit activity',
        category: 'Compliance',
        description: 'Security and business activity available to audit readers.',
        fields: [
          { key: 'timestamp', label: 'Timestamp', type: 'date', filterable: true },
          { key: 'action', label: 'Action', type: 'string', filterable: true },
          { key: 'userId', label: 'User', type: 'string', filterable: true }
        ]
      }
    ];
  }
}
