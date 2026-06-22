using System.Globalization;

namespace IEMS.Core.Services;

public class AmountToWordsService
{
    private static readonly string[] Units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
    private static readonly string[] Tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
    private static readonly string[] Teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };

    public string ConvertToWords(decimal amount)
    {
        // Handle negative amounts (e.g. refunds) instead of dropping the sign and the integer part
        if (amount < 0) return "Minus " + ConvertToWords(-amount);

        // Round to paise up-front so the fractional part can never carry to 100
        amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        if (amount == 0) return "Zero Rupees Only";

        var integerPart = (long)amount;
        var decimalPart = (int)Math.Round((amount - integerPart) * 100, MidpointRounding.AwayFromZero);

        // Safety net: if rounding still produced 100 paise, roll it into rupees
        if (decimalPart >= 100)
        {
            integerPart += decimalPart / 100;
            decimalPart %= 100;
        }

        var words = ConvertIntegerToWords(integerPart);
        if (string.IsNullOrEmpty(words)) words = "Zero"; // amounts under ₹1 (e.g. 0.50)

        if (integerPart == 1)
            words += " Rupee";
        else
            words += " Rupees";

        if (decimalPart > 0)
        {
            words += " and " + ConvertIntegerToWords(decimalPart);
            if (decimalPart == 1)
                words += " Paisa";
            else
                words += " Paise";
        }

        return words + " Only";
    }

    private string ConvertIntegerToWords(long number)
    {
        if (number == 0) return "";

        var words = "";

        if (number >= 10000000) // Crore
        {
            words += ConvertIntegerToWords(number / 10000000) + " Crore ";
            number %= 10000000;
        }

        if (number >= 100000) // Lakh
        {
            words += ConvertIntegerToWords(number / 100000) + " Lakh ";
            number %= 100000;
        }

        if (number >= 1000) // Thousand
        {
            words += ConvertIntegerToWords(number / 1000) + " Thousand ";
            number %= 1000;
        }

        if (number >= 100) // Hundred
        {
            words += ConvertIntegerToWords(number / 100) + " Hundred ";
            number %= 100;
        }

        if (number >= 20)
        {
            words += Tens[number / 10] + " ";
            number %= 10;
        }
        else if (number >= 10)
        {
            words += Teens[number - 10] + " ";
            return words.Trim();
        }

        if (number > 0)
        {
            words += Units[number] + " ";
        }

        return words.Trim();
    }

    public string FormatCurrency(decimal amount)
    {
        return amount.ToString("C", new CultureInfo("en-IN"));
    }

    public bool IsValidAmount(decimal amount)
    {
        return amount >= 0 && amount <= 99999999999.99m; // Max 99,999 crore
    }
}