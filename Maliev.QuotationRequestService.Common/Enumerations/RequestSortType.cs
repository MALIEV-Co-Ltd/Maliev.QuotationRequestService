namespace Maliev.QuotationRequestService.Common.Enumerations
{
    /// <summary>
    /// Request Sort Type.
    /// </summary>
    public enum RequestSortType
    {
        /// <summary>
        /// The quotation identifier ascending
        /// </summary>
        RequestId_Ascending,

        /// <summary>
        /// The quotation identifier descending
        /// </summary>
        RequestId_Descending,

        /// <summary>
        /// The quotation created date ascending
        /// </summary>
        RequestCreatedDate_Ascending,

        /// <summary>
        /// The quotation created date descending
        /// </summary>
        RequestCreatedDate_Descending,

        /// <summary>
        /// The quotation modified date ascending
        /// </summary>
        RequestModifiedDate_Ascending,

        /// <summary>
        /// The quotation modified date descending
        /// </summary>
        RequestModifiedDate_Descending,
    }
}