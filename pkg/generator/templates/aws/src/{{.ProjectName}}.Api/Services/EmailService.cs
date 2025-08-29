{{if .IncludeMail}}
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    Task SendEmailAsync(string to, string subject, string body, string? fromName = null, bool isHtml = false);
}

public class EmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        return SendEmailAsync(to, subject, body, _options.FromName, isHtml);
    }

    public async Task SendEmailAsync(string to, string subject, string body, string? fromName = null, bool isHtml = false)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName ?? _options.FromName, _options.FromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // For development (MailHog), disable SSL
            if (_options.Host == "localhost" || _options.Host == "mailhog")
            {
                await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.None);
            }
            else
            {
                await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
            }

            if (!string.IsNullOrEmpty(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}

public class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = false;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "";
}
{{end}}