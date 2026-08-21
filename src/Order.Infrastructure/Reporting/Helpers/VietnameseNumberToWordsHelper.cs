using System.Text;

namespace WolverineApp.Infrastructure.Reporting.Helpers;

public static class VietnameseNumberToWordsHelper
{
    private static readonly string[] Digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
    private static readonly string[] Units = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };

    public static string ConvertToWords(decimal amount, string currencyUnit = "đồng")
    {
        if (amount == 0)
        {
            return $"Không {currencyUnit} chẵn";
        }

        var isNegative = amount < 0;
        var absAmount = Math.Abs(Math.Truncate(amount));
        var numberStr = absAmount.ToString("0");

        var groups = new List<string>();
        for (int i = numberStr.Length; i > 0; i -= 3)
        {
            int start = Math.Max(0, i - 3);
            int length = i - start;
            groups.Add(numberStr.Substring(start, length));
        }

        var sb = new StringBuilder();
        if (isNegative) sb.Append("Âm ");

        for (int i = groups.Count - 1; i >= 0; i--)
        {
            var groupNum = int.Parse(groups[i]);
            if (groupNum == 0) continue;

            var groupWords = ReadThreeDigits(groups[i], i < groups.Count - 1);
            sb.Append(groupWords);
            if (!string.IsNullOrWhiteSpace(Units[i]))
            {
                sb.Append(" ").Append(Units[i]).Append(" ");
            }
            else
            {
                sb.Append(" ");
            }
        }

        var result = sb.ToString().Trim();
        while (result.Contains("  "))
        {
            result = result.Replace("  ", " ");
        }

        if (string.IsNullOrWhiteSpace(result))
        {
            result = "Không";
        }
        else
        {
            result = char.ToUpper(result[0]) + result[1..];
        }

        return $"{result} {currencyUnit} chẵn";
    }

    private static string ReadThreeDigits(string group, bool readZeroHundred)
    {
        int num = int.Parse(group);
        int hundreds = num / 100;
        int tens = (num % 100) / 10;
        int ones = num % 10;

        var sb = new StringBuilder();

        if (hundreds > 0 || readZeroHundred)
        {
            sb.Append(Digits[hundreds]).Append(" trăm ");
        }

        if (tens > 1)
        {
            sb.Append(Digits[tens]).Append(" mươi ");
            if (ones == 1) sb.Append("mốt");
            else if (ones == 5) sb.Append("lăm");
            else if (ones > 0) sb.Append(Digits[ones]);
        }
        else if (tens == 1)
        {
            sb.Append("mười ");
            if (ones == 5) sb.Append("lăm");
            else if (ones > 0) sb.Append(Digits[ones]);
        }
        else // tens == 0
        {
            if ((hundreds > 0 || readZeroHundred) && ones > 0)
            {
                sb.Append("lẻ ").Append(Digits[ones]);
            }
            else if (ones > 0)
            {
                sb.Append(Digits[ones]);
            }
        }

        return sb.ToString().Trim();
    }
}
