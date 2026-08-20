using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WolverineApp.Application.Common.Reporting;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Infrastructure.Reporting.Helpers;

namespace WolverineApp.Infrastructure.Reporting.Renderers;

public class QuestPdfDocumentRenderer : IDocumentRenderer
{
    public ReportOutputFormat SupportedFormat => ReportOutputFormat.Pdf;

    static QuestPdfDocumentRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> RenderAsync(
        string templateCode,
        string compiledHtml,
        object dataModel,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        OrderDto? order = dataModel as OrderDto;
        if (order is null && dataModel is not null)
        {
            var json = JsonSerializer.Serialize(dataModel);
            try
            {
                order = JsonSerializer.Deserialize<OrderDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
            }
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // 1. Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(orgCol =>
                        {
                            orgCol.Item().Text("HỆ THỐNG QUẢN LÝ DOANH NGHIỆP PHÂN TÁN (EDAP)").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                            orgCol.Item().Text("Đơn vị triển khai: Enterprise Operations").FontSize(9).FontColor(Colors.Grey.Darken1);
                            orgCol.Item().Text("Địa chỉ: Tầng 8, Tòa nhà Công nghệ cao, Hà Nội").FontSize(9).FontColor(Colors.Grey.Darken1);
                            orgCol.Item().Text("Hotline: 1900-6868 | Website: https://enterprise-platform.vn").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        if (order is not null)
                        {
                            var qrPayload = $"ORDER:{order.OrderNumber}|TOTAL:{order.TotalAmount:N0}|DATE:{order.CreatedAt:yyyy-MM-dd}";
                            var qrBytes = BarcodeQrHelper.GenerateQrCodePngBytes(qrPayload, 4);
                            if (qrBytes.Length > 0)
                            {
                                row.ConstantItem(65).Image(qrBytes);
                            }
                        }
                    });

                    col.Item().PaddingTop(10).AlignCenter().Text("HÓA ĐƠN BÁN HÀNG & PHIẾU XUẤT KHO").Bold().FontSize(16).FontColor(Colors.Red.Darken2);
                    col.Item().AlignCenter().Text($"(Mã chứng từ: #{order?.OrderNumber ?? "N/A"} - Ngày lập: {order?.CreatedAt:dd/MM/yyyy HH:mm})").Italic().FontSize(9);
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // 2. Body Content
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Row(custRow =>
                    {
                        custRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text(txt => { txt.Span("Khách hàng: ").Bold(); txt.Span(order?.CustomerName ?? "Khách lẻ"); });
                            c.Item().Text(txt => { txt.Span("Email liên hệ: ").Bold(); txt.Span(order?.CustomerEmail ?? "N/A"); });
                        });

                        custRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text(txt => { txt.Span("Trạng thái đơn: ").Bold(); txt.Span(order?.Status ?? "Created"); });
                            c.Item().Text(txt => { txt.Span("Hình thức thanh toán: ").Bold(); txt.Span("Chuyển khoản / COD"); });
                        });
                    });

                    col.Item().PaddingTop(12);

                    // Bảng chi tiết sản phẩm
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30); // STT
                            columns.RelativeColumn(3);  // Tên sản phẩm
                            columns.RelativeColumn(1.5f); // SKU
                            columns.ConstantColumn(50); // SL
                            columns.RelativeColumn(2);  // Đơn giá
                            columns.RelativeColumn(2);  // Thành tiền
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text("STT").Bold().FontSize(9);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Tên sản phẩm / Dịch vụ").Bold().FontSize(9);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Mã SKU").Bold().FontSize(9);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text("SL").Bold().FontSize(9);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Đơn giá").Bold().FontSize(9);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Thành tiền").Bold().FontSize(9);
                        });

                        if (order?.Items != null && order.Items.Count > 0)
                        {
                            int stt = 1;
                            foreach (var item in order.Items)
                            {
                                var bg = stt % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text(stt.ToString()).FontSize(9);
                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.ProductName).FontSize(9);
                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Sku ?? "").FontSize(9);
                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"${item.UnitPrice:N2}").FontSize(9);
                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"${item.Total:N2}").Bold().FontSize(9);
                                stt++;
                            }
                        }
                        else
                        {
                            table.Cell().ColumnSpan(6).Padding(10).AlignCenter().Text("Không có chi tiết sản phẩm.").Italic();
                        }
                    });

                    col.Item().PaddingTop(10);

                    // Tổng tiền
                    col.Item().AlignRight().Row(sumRow =>
                    {
                        sumRow.ConstantItem(300).Column(sc =>
                        {
                            sc.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Tổng cộng tiền hàng:").Bold();
                                r.RelativeItem().AlignRight().Text($"${order?.TotalAmount ?? 0:N2}").Bold().FontSize(13).FontColor(Colors.Red.Darken2);
                            });
                        });
                    });

                    col.Item().PaddingTop(5).Text(txt =>
                    {
                        var words = VietnameseNumberToWordsHelper.ConvertToWords(order?.TotalAmount ?? 0, "đô la Mỹ");
                        txt.Span("Số tiền bằng chữ: ").Bold().Italic();
                        txt.Span(words).Italic().FontColor(Colors.Blue.Darken3);
                    });

                    col.Item().PaddingTop(25);

                    // Chữ ký 3 bên
                    col.Item().Row(sigRow =>
                    {
                        sigRow.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("NGƯỜI LẬP PHIẾU").Bold().FontSize(9);
                            c.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(8);
                            c.Item().PaddingTop(35).Text("Admin").FontSize(9);
                        });

                        sigRow.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("THỦ KHO XUẤT").Bold().FontSize(9);
                            c.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(8);
                            c.Item().PaddingTop(35).Text("...........................").FontSize(9);
                        });

                        sigRow.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("KHÁCH HÀNG NHẬN").Bold().FontSize(9);
                            c.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(8);
                            c.Item().PaddingTop(35).Text(order?.CustomerName ?? "...........................").FontSize(9);
                        });
                    });
                });

                // 3. Footer
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text("Chứng từ điện tử trích xuất từ hệ thống EDAP Core Engine").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(txt =>
                        {
                            txt.Span("Trang ");
                            txt.CurrentPageNumber();
                            txt.Span(" / ");
                            txt.TotalPages();
                        });
                    });
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }
}
