using Carmasters.Core.Domain;

namespace Carmasters.Http.Api.Model
{
    public record  IssueInvoiceDto(PaymentType PaymentType, short DueDays, bool SendClientEmail, string ClientEmail);
}
