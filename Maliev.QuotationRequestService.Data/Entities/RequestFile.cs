namespace Maliev.QuotationRequestService.Data.Entities
{
    public class RequestFile
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Bucket { get; set; }
        public string ObjectName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Request Request { get; set; } = null!;
    }
}