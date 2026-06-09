namespace IBOPE.DoubleBlind.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailWithAttachmentAsync(
        string to,
        string subject,
        string body,
        string attachmentFileName,
        string attachmentContent,
        CancellationToken cancellationToken = default);
}
