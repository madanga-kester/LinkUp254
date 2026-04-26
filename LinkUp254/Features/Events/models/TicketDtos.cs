using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LinkUp254.Features.Events.Models;

namespace LinkUp254.Features.Events.Models;

public class CreateTicketTierDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public int Capacity { get; set; }

    public int? MinPerOrder { get; set; }
    public int? MaxPerOrder { get; set; }

    public DateTime? SaleStartsAt { get; set; }
    public DateTime? SaleEndsAt { get; set; }

    public bool? RequirePhoneNumber { get; set; }
    public bool? RequireStudentId { get; set; }

    public bool? IsTransferable { get; set; }
    public bool? IsRefundable { get; set; }
    public int? RefundDeadlineHours { get; set; }

    public bool? IsActive { get; set; }
}

public class UpdateTicketTierDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    public int? Capacity { get; set; }
    public int? MinPerOrder { get; set; }
    public int? MaxPerOrder { get; set; }

    public DateTime? SaleStartsAt { get; set; }
    public DateTime? SaleEndsAt { get; set; }

    public bool? RequirePhoneNumber { get; set; }
    public bool? RequireStudentId { get; set; }

    public bool? IsTransferable { get; set; }
    public bool? IsRefundable { get; set; }
    public int? RefundDeadlineHours { get; set; }

    public bool? IsActive { get; set; }
}

public class TicketTierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int RemainingCapacity { get; set; }
    public int? MinPerOrder { get; set; }
    public int? MaxPerOrder { get; set; }
    public bool RequirePhoneNumber { get; set; }
    public bool RequireStudentId { get; set; }
    public bool IsTransferable { get; set; }
    public bool IsRefundable { get; set; }
}

public class PurchaseTicketDto
{
    [Required]
    public int TierId { get; set; }

    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }

    [StringLength(200)]
    public string? BuyerName { get; set; }

    [StringLength(254)]
    [EmailAddress]
    public string? BuyerEmail { get; set; }

    [StringLength(20)]
    [Phone]
    public string? BuyerPhoneNumber { get; set; }

    [StringLength(200)]
    public string? AttendeeName { get; set; }

    [StringLength(254)]
    [EmailAddress]
    public string? AttendeeEmail { get; set; }

    [StringLength(20)]
    [Phone]
    public string? AttendeePhoneNumber { get; set; }

    [StringLength(500)]
    public string? StudentIdImageUrl { get; set; }

    [StringLength(50)]
    public string? PaymentProvider { get; set; } = "manual";

    [StringLength(100)]
    public string? IdempotencyKey { get; set; }
}

public class PurchaseTicketResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public int? TicketId { get; set; }
    public string? TicketCode { get; set; }

    public bool RequiresPayment { get; set; }
    public string? PaymentProvider { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public MpesaStkPayload? MpesaPayload { get; set; }

    public static PurchaseTicketResult Failure(string message) =>
        new PurchaseTicketResult { IsSuccess = false, Message = message };
}

public class MpesaStkPayload
{
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public int Amount { get; set; }

    [Required]
    [StringLength(100)]
    public string AccountReference { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string TransactionDesc { get; set; } = string.Empty;
}

public class ConfirmPaymentDto
{
    [Required]
    public bool IsSuccess { get; set; }

    [StringLength(100)]
    public string? ProviderTransactionId { get; set; }
}

public class ValidateTicketResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public string? AttendeeName { get; set; }
    public string? TicketTier { get; set; }

    public static ValidateTicketResult Failure(string message) =>
        new ValidateTicketResult { IsSuccess = false, Message = message };
}

public class RefundTicketDto
{
    [StringLength(500)]
    public string? Reason { get; set; }

    [Range(1, 100)]
    public decimal? RefundPercentage { get; set; }
}

public class TransferTicketDto
{
    [Required]
    [StringLength(200)]
    public string NewAttendeeName { get; set; } = string.Empty;

    [Required]
    [StringLength(254)]
    [EmailAddress]
    public string NewAttendeeEmail { get; set; } = string.Empty;

    [StringLength(20)]
    [Phone]
    public string? NewAttendeePhoneNumber { get; set; }
}

public class MpesaCallbackDto
{
    public string Body { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string ResultDesc { get; set; } = string.Empty;
    public string MpesaReceiptNumber { get; set; } = string.Empty;
    public string AccountReference { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Signature { get; set; }
}

public class ValidateTicketRequest
{
    public string? ScannerDeviceId { get; set; }
    public string? CheckInPoint { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class OfflineValidateRequest
{
    [Required]
    public string TicketCode { get; set; } = string.Empty;
    [Required]
    public string QRCodeData { get; set; } = string.Empty;
    [Required]
    public string Signature { get; set; } = string.Empty;
}

public class TicketDetailsDto
{
    public string TicketCode { get; set; } = string.Empty;
    public string EventTitle { get; set; } = string.Empty;
    public string TicketTier { get; set; } = string.Empty;
    public string AttendeeName { get; set; } = string.Empty;
    public DateTime EventStartTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string? QRCodeImageUrl { get; set; }
    public bool IsCheckedIn { get; set; }
    public string Status { get; set; } = string.Empty;
}