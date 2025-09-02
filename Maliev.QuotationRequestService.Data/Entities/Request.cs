namespace Maliev.QuotationRequestService.Data.Entities
{
    public class Request
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? TelephoneNumber { get; set; }
        public string Country { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxIdentification { get; set; }
        public string Message { get; set; }
        public string? InternalComment { get; set; }
        public bool? Done { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<RequestFile> RequestFiles { get; set; } = new List<RequestFile>();
    }
}