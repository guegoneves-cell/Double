using System.Net;
using System.Net.Mail;
using System.Text;
using IBOPE.DoubleBlind.Application.Interfaces;
using IBOPE.DoubleBlind.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IBOPE.DoubleBlind.Infrastructure.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailWithAttachmentAsync(
        string to,
        string subject,
        string body,
        string attachmentFileName,
        string attachmentContent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) ||
            string.IsNullOrWhiteSpace(_settings.From) ||
            string.IsNullOrWhiteSpace(_settings.UserName) ||
            string.IsNullOrWhiteSpace(_settings.Password))
        {
            throw new InvalidOperationException(
                "Configuração SMTP incompleta. Defina Host, From, UserName e Password em appsettings.json.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.From),
            Subject = subject,
            Body = body,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        message.To.Add(to);

        var attachmentBytes = Encoding.UTF8.GetBytes(attachmentContent);
        using var attachmentStream = new MemoryStream(attachmentBytes);
        message.Attachments.Add(new Attachment(attachmentStream, attachmentFileName, "text/plain"));

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
        };

        _logger.LogInformation("Enviando e-mail para {Recipient} com anexo {Attachment}", to, attachmentFileName);

        await client.SendMailAsync(message, cancellationToken);
    }
}
