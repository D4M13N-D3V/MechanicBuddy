using MechanicBuddy.Core.Domain;

namespace MechanicBuddy.Http.Api.Model
{
    public record  IssueInvoiceDto(PaymentType PaymentType, short DueDays, bool SendClientEmail, string ClientEmail);
}
