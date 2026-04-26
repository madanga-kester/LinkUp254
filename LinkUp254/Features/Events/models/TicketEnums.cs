namespace LinkUp254.Features.Events.Models;

public enum TicketStatus
{
    Reserved,
    Active,
    Used,
    Cancelled,
    Refunded,
    Transferred,
    Expired
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Refunded,
    PartiallyRefunded
}

public enum VerificationStatus
{
    Unverified,
    Verified,
    Invalid,
    PendingReview
}