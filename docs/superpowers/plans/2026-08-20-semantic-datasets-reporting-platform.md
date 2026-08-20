# Semantic Datasets & No-Code Report Platform Implementation Plan

> **Goal:** Implement the Semantic Dataset Catalog and Visual No-Code Report Configuration & Execution Engine, allowing deployment engineers to create new reports in minutes without writing raw SQL or C# code.

**Architecture:** 
- `SemanticDataset`: System-defined business datasets containing field metadata and secure base queries.
- `ReportConfiguration`: Tenant-specific report configurations referencing a semantic dataset with selected fields, filter configurations, and Liquid templates.
- `SemanticDatasetService`: Synthesizes safe parameterized queries, enforces tenant isolation, executes via Dapper, and auto-generates Liquid templates.
- `ReportsController`: Exposes APIs for dataset catalog discovery, visual report configuration CRUD, dynamic form schema retrieval, and report execution.

**Tech Stack:** .NET 10, C# 13, EF Core, Dapper, Fluid (Liquid), QuestPDF, SQLite/PostgreSQL.

---

### Task 1: Domain Entities & Database Mapping
- [ ] Create `SemanticDataset` entity in `Domain/Reporting/SemanticDataset.cs`.
- [ ] Create `ReportConfiguration` entity in `Domain/Reporting/ReportConfiguration.cs`.
- [ ] Register `DbSet<SemanticDataset>` and `DbSet<ReportConfiguration>` in `ApplicationDbContext.cs`.
- [ ] Seed standard `Sales_Orders_Dataset` with Vietnamese field metadata in `DbInitializer.cs`.

### Task 2: Semantic Dataset Engine & Safe Query Synthesizer
- [ ] Create `ISemanticDatasetService` in `Application/Common/Reporting/ISemanticDatasetService.cs`.
- [ ] Implement `SemanticDatasetService` in `Infrastructure/Reporting/SemanticDatasetService.cs` with:
  - Catalog listing & field schema retrieval.
  - Safe parameterized query builder with mandatory `@TenantId` injection.
  - Dapper query execution returning normalized dynamic records.
  - Auto-generation of default Liquid HTML table layout for selected fields.

### Task 3: Visual Report Configuration & Execution Engine
- [ ] Implement Report Configuration management & execution pipeline.
- [ ] Support dynamic form filter schema extraction from `FilterConfigJson`.
- [ ] Bind synthesized query results to `LiquidReportEngine` for PDF/HTML/Excel rendering.

### Task 4: API Endpoints in `ReportsController`
- [ ] Add `GET /api/reports/semantic-datasets` (Dataset catalog with Vietnamese fields).
- [ ] Add `POST /api/reports/configurations` (Save visual report configuration).
- [ ] Add `GET /api/reports/configurations/{code}/form-schema` (Auto-generated filter form schema).
- [ ] Add `POST /api/reports/configurations/{code}/execute` (Execute report and export PDF/HTML).

### Task 5: End-to-End Automated Verification & Progress Reporting
- [ ] Run automated script testing the full 4-step deployment engineer workflow.
- [ ] Verify multi-tenant isolation, filter execution, and PDF/HTML output.
- [ ] Commit all code changes to GitHub repository.
