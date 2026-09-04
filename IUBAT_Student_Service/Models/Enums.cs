namespace IUBAT_Student_Service.Models
{
    public enum RequestType
    {
        IDCardReplacement,
        TranscriptRequest,
        CertificateRequest
    }

    public enum RequestStatus
    {
        Pending,
        Processing,
        Completed,
        Rejected
    }
}
