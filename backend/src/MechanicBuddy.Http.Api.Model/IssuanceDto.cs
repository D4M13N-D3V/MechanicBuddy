using System;

namespace Carmasters.Http.Api.Model
{
    public class IssuanceDto
    {
        public IssuanceDto()
        {

        }
        public IssuanceDto( DateTime? SentOn, DateTime IssuedOn, string IssuedBy, string ReceiverEmail)
        {
           
            this.SentOn = SentOn;
            this.IssuedOn = IssuedOn;
            this.IssuedBy = IssuedBy;
            this.ReceiverEmail = ReceiverEmail;
        }

     
        public DateTime? SentOn { get; set; }
        public DateTime IssuedOn { get; set; }
        public string IssuedBy { get; set; }
        public string ReceiverEmail { get; set; }
    }

	public class OfferIssuanceDto: IssuanceDto
    {
        public OfferIssuanceDto() { }
        public OfferIssuanceDto(Guid Id,String Number, DateTime? SentOn, DateTime IssuedOn, string IssuedBy, string AcceptedBy, DateTime? AcceptedOn, string ReceiverEmail)
            : base( SentOn,IssuedOn,IssuedBy,ReceiverEmail)
        {
            this.Id = Id;
            this.AcceptedBy = AcceptedBy;
            this.Number = Number;
            this.AcceptedOn = AcceptedOn;
        }
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string AcceptedBy { get; set; }
        public DateTime? AcceptedOn { get; set; }
    }

    public class WorkIssuanceDto : IssuanceDto
    {
        public WorkIssuanceDto(  DateTime? sentOn, DateTime issuedOn, string issuedBy, string receiverEmail, int invoiceNumber, short dueDays, bool isPaid)
            : base(  sentOn, issuedOn, issuedBy, receiverEmail)
        { 
            InvoiceNumber = invoiceNumber;
            DueDays = dueDays;
            IsPaid = isPaid;
        }
         
        public int InvoiceNumber { get; set; }
        public short DueDays { get; set; }
        public bool IsPaid { get; set; }
    }
     
}
