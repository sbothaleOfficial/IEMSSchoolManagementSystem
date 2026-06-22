using IEMS.Core.Enums;

namespace IEMS.Core.Services;

public class FeeCalculationService
{
    public FeeCalculationResult CalculatePayment(
        decimal amountPaid,
        decimal discount,
        decimal lateFee,
        decimal previousBalance,
        decimal totalFeeAmount)
    {
        if (amountPaid < 0) throw new ArgumentException("Amount paid cannot be negative");
        if (discount < 0) throw new ArgumentException("Discount cannot be negative");
        if (lateFee < 0) throw new ArgumentException("Late fee cannot be negative");

        // CORRECT CALCULATION:
        // Total owed = previousBalance + feeAmount + lateFee - discount
        // Remaining = totalOwed - amountPaid
        // Floor at 0 so a discount larger than the bill cannot create a negative "owed"
        // (which would otherwise refund more money than was actually paid).
        var totalOwed = Math.Round(Math.Max(0, previousBalance + totalFeeAmount + lateFee - discount), 2);
        amountPaid = Math.Round(amountPaid, 2);
        var newRemainingBalance = Math.Max(0, totalOwed - amountPaid);
        var isOverpayment = amountPaid > totalOwed;
        var effectiveAmount = amountPaid; // Amount paid is the effective payment amount

        return new FeeCalculationResult
        {
            EffectiveAmount = effectiveAmount,
            RemainingBalance = newRemainingBalance,
            IsFullyPaid = newRemainingBalance == 0,
            IsOverpayment = isOverpayment,
            OverpaymentAmount = isOverpayment ? amountPaid - totalOwed : 0,
            PaidAmount = amountPaid,
            DiscountApplied = discount,
            LateFeeApplied = lateFee
        };
    }

    public decimal CalculateLateFee(DateTime dueDate, DateTime paymentDate, decimal baseAmount, decimal lateFeePercentage = 0.01m)
    {
        if (paymentDate <= dueDate) return 0;

        var daysLate = (decimal)(paymentDate - dueDate).TotalDays;
        var lateFee = baseAmount * lateFeePercentage * (daysLate / 30.0m); // Monthly basis
        return Math.Round(lateFee, 2);
    }

    public bool IsEligibleForDiscount(PaymentMethod paymentMethod, DateTime paymentDate, DateTime dueDate)
    {
        // Early payment discount logic
        if (paymentDate <= dueDate.AddDays(-7) && paymentMethod == PaymentMethod.ONLINE)
        {
            return true;
        }
        return false;
    }
}

public class FeeCalculationResult
{
    public decimal EffectiveAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool IsFullyPaid { get; set; }
    public bool IsOverpayment { get; set; }
    public decimal OverpaymentAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal LateFeeApplied { get; set; }
}