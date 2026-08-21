using QRCoder;

namespace WolverineApp.Infrastructure.Reporting.Helpers;

public static class BarcodeQrHelper
{
    public static string GenerateQrCodeBase64(string payload, int pixelsPerModule = 10)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);

        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }

    public static byte[] GenerateQrCodePngBytes(string payload, int pixelsPerModule = 10)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return Array.Empty<byte>();
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(pixelsPerModule);
    }
}
