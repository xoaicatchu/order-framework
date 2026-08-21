import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import {
  CreateReportInput,
  ReportApiService,
  ReportFilter,
  SemanticDataset
} from './core/report-api.service';

type ViewKey = 'overview' | 'builder' | 'datasets';
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
  imports: [FormsModule],
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
  reportCode = 'report-demo';
  reportName = 'Báo cáo đơn hàng tháng';
  reportDescription = 'Tổng hợp đơn hàng và doanh thu theo khoảng thời gian.';
  outputFormat: 'Pdf' | 'Html' = 'Pdf';

  filters: ReportFilter[] = [
    {
      fieldName: 'createdAt',
      label: 'Khoảng ngày tạo',
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
    { name: 'Báo cáo đơn hàng tháng', dataset: 'Đơn hàng', format: 'PDF', status: 'Ready', updated: '12 phút trước', accent: 'violet' },
    { name: 'Doanh thu theo khách hàng', dataset: 'Đơn hàng', format: 'HTML', status: 'Draft', updated: 'Hôm qua', accent: 'blue' },
    { name: 'Tổng quan vận hành', dataset: 'Thống kê đơn hàng', format: 'PDF', status: 'Running', updated: 'Hôm qua', accent: 'amber' }
  ];

  get selectedDataset(): SemanticDataset {
    return this.datasets.find((dataset) => dataset.code === this.selectedDatasetCode) ?? this.datasets[0] ?? {
      code: 'empty', name: 'Chưa có nguồn dữ liệu', category: 'Chưa kết nối',
      description: 'Kết nối API để tải danh mục dữ liệu.', fields: []
    };
  }

  get statusLabel(): string {
    return { demo: 'Dữ liệu mẫu', connecting: 'Đang kết nối', connected: 'Đã kết nối API', error: 'Chạy ngoại tuyến' }[this.connectionState];
  }

  setView(view: ViewKey): void { this.activeView = view; }
  openBuilder(): void { this.activeView = 'builder'; this.showConnectionPanel = false; }

  selectDataset(code: string): void {
    this.selectedDatasetCode = code;
    const available = this.datasets.find((dataset) => dataset.code === code)?.fields ?? [];
    this.selectedFields = available.slice(0, 3).map((field) => field.key);
    this.filters = available.filter((field) => field.filterable).slice(0, 1).map((field) => ({
      fieldName: field.key,
      label: field.label,
      filterType: field.type === 'date' ? 'date_range' : field.type === 'enum' ? 'select' : 'text',
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
      this.notify('Nguồn dữ liệu này không còn trường nào có thể lọc.', 'info');
      return;
    }
    this.filters = [...this.filters, {
      fieldName: candidate.key,
      label: candidate.label,
      filterType: candidate.type === 'date' ? 'date_range' : candidate.type === 'enum' ? 'select' : 'text',
      required: false,
      defaultValue: null
    }];
  }

  removeFilter(index: number): void { this.filters = this.filters.filter((_, currentIndex) => currentIndex !== index); }

  async connectApi(): Promise<void> {
    if (!this.accessToken.trim()) {
      this.notify('Hãy nhập access token từ hệ thống đăng nhập trước.', 'error');
      return;
    }

    this.connectionState = 'connecting';
    this.api.configure(this.apiBaseUrl, this.accessToken);
    try {
      const response = await firstValueFrom(this.api.catalog);
      if (!response.success || !response.data) throw new Error(response.message ?? 'API không trả về danh mục dữ liệu.');
      this.datasets = response.data.dataSources.map((source) => ({
        code: source.id,
        name: source.name,
        category: source.category,
        description: source.description,
        fields: source.fields.map((field) => ({
          key: field.id,
          label: field.name,
          type: field.type,
          filterable: field.canFilter,
          enumValues: field.options ?? undefined
        }))
      }));
      this.selectedDatasetCode = this.datasets[0]?.code ?? this.selectedDatasetCode;
      this.connectionState = 'connected';
      this.notify('Đã tải danh mục nguồn dữ liệu từ API.', 'success');
    } catch (error) {
      this.connectionState = 'error';
      this.notify(error instanceof Error ? error.message : 'Không kết nối được API.', 'error');
    }
  }

  async saveConfiguration(): Promise<void> {
    if (!this.reportName.trim() || !this.selectedDatasetCode) {
      this.notify('Tên báo cáo và nguồn dữ liệu là bắt buộc.', 'error');
      return;
    }

    const input: CreateReportInput = {
      name: this.reportName.trim(),
      dataSourceId: this.selectedDatasetCode,
      columns: this.selectedFields,
      filters: this.filters.map((filter) => ({
        field: filter.fieldName,
        type: filter.filterType,
        label: filter.label,
        required: filter.required,
        defaultValue: filter.defaultValue
      }))
    };

    if (!this.api.configured) {
      this.upsertRecentReport('Draft');
      this.notify('Đã lưu bản nháp trên màn hình. Kết nối API để lưu vào hệ thống.', 'info');
      return;
    }

    try {
      const response = await firstValueFrom(this.api.createReport(input));
      if (!response.success || !response.data) throw new Error(response.message ?? 'Không tạo được báo cáo.');
      this.reportCode = response.data.code;
      this.upsertRecentReport('Ready');
      this.notify('Đã lưu báo cáo vào workspace.', 'success');
    } catch (error) {
      this.notify(error instanceof Error ? error.message : 'Lưu báo cáo thất bại.', 'error');
    }
  }

  async exportReport(): Promise<void> {
    const filters = this.exportFilters();
    if (!this.api.configured) {
      const demoContent = `<html><body><h1>${this.reportName}</h1><p>Demo export · ${new Date().toLocaleString()}</p><p>Kết nối API để xuất dữ liệu thật.</p></body></html>`;
      this.download(new Blob([demoContent], { type: 'text/html' }), `${this.reportCode}.html`);
      this.notify('Đã tải file HTML mẫu. Kết nối API để xuất PDF/dữ liệu thật.', 'info');
      return;
    }

    if (this.reportCode === 'report-demo') {
      await this.saveConfiguration();
      if (this.reportCode === 'report-demo') return;
    }

    try {
      const response = await firstValueFrom(this.api.exportReport(this.reportCode, {
        format: this.outputFormat === 'Pdf' ? 'pdf' : 'html',
        filters
      }));
      const contentType = response.headers.get('content-type') ?? 'application/octet-stream';
      const extension = this.outputFormat === 'Pdf' ? 'pdf' : 'html';
      this.download(response.body ?? new Blob([], { type: contentType }), `${this.reportCode}.${extension}`);
      this.notify('Đã xuất báo cáo thành công.', 'success');
    } catch (error) {
      this.notify(error instanceof Error ? error.message : 'Xuất báo cáo thất bại.', 'error');
    }
  }

  private exportFilters(): Record<string, unknown> {
    const result: Record<string, unknown> = {};
    for (const filter of this.filters) {
      if (filter.filterType === 'date_range') {
        result[filter.fieldName] = {
          from: this.filterValues[`${filter.fieldName}_from`] || null,
          to: this.filterValues[`${filter.fieldName}_to`] || null
        };
      } else if (this.filterValues[filter.fieldName]?.trim()) {
        result[filter.fieldName] = this.filterValues[filter.fieldName].trim();
      }
    }
    return result;
  }

  private upsertRecentReport(status: RecentReport['status']): void {
    this.recentReports = [{
      name: this.reportName,
      dataset: this.selectedDataset.name,
      format: this.outputFormat.toUpperCase(),
      status,
      updated: 'Vừa xong',
      accent: 'violet'
    }, ...this.recentReports.filter((report) => report.name !== this.reportName)];
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
    window.setTimeout(() => { if (this.toastMessage === message) this.toastMessage = ''; }, 4200);
  }

  private demoDatasets(): SemanticDataset[] {
    return [
      {
        code: 'orders', name: 'Đơn hàng', category: 'Bán hàng', description: 'Đơn hàng, khách hàng và doanh thu trong tenant hiện tại.',
        fields: [
          { key: 'orderNumber', label: 'Mã đơn hàng', type: 'string', filterable: true },
          { key: 'customerName', label: 'Khách hàng', type: 'string', filterable: true },
          { key: 'totalAmount', label: 'Tổng tiền', type: 'currency', filterable: true },
          { key: 'status', label: 'Trạng thái', type: 'enum', filterable: true, enumValues: ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled'] },
          { key: 'createdAt', label: 'Ngày tạo', type: 'date', filterable: true }
        ]
      },
      {
        code: 'order-statistics', name: 'Thống kê đơn hàng', category: 'Vận hành', description: 'Số lượng và doanh thu được tổng hợp theo trạng thái.',
        fields: [
          { key: 'status', label: 'Trạng thái', type: 'enum', filterable: true },
          { key: 'orderCount', label: 'Số đơn', type: 'number', filterable: false },
          { key: 'totalRevenue', label: 'Doanh thu', type: 'currency', filterable: false }
        ]
      },
      {
        code: 'audit-logs', name: 'Hoạt động hệ thống', category: 'Kiểm soát', description: 'Hoạt động người dùng và sự kiện cần theo dõi.',
        fields: [
          { key: 'timestamp', label: 'Thời gian', type: 'date', filterable: true },
          { key: 'action', label: 'Hành động', type: 'string', filterable: true },
          { key: 'userId', label: 'Người dùng', type: 'string', filterable: true }
        ]
      }
    ];
  }
}
