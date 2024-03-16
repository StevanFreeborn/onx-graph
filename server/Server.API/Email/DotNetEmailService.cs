namespace Server.API.Email;

class DotNetEmailService(
  IOptions<SmtpOptions> smtpOptions,
  ILogger<DotNetEmailService> logger,
  IEmailClient client
) : IEmailService
{
  private readonly SmtpOptions _smtpOptions = smtpOptions.Value;
  private readonly ILogger<DotNetEmailService> _logger = logger;
  private readonly IEmailClient _client = client;

  public async Task<Result> SendEmailAsync(EmailMessage message)
  {
    var attempts = 0;

    while (attempts < 3)
    {
      try
      {
        var email = new MailMessage(_smtpOptions.SenderEmail, message.To)
        {
          Subject = message.Subject,
          Body = message.HtmlContent,
          IsBodyHtml = true
        };

        await _client.SendMailAsync(email);
        return Result.Ok();
      }
      catch (Exception ex) when (ex is SmtpFailedRecipientException or SmtpException)
      {
        attempts++;
        _logger.LogError(ex, "Failed to send email. Attempt {AttemptNumber}", attempts);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send email");
        break;
      }
    }

    return Result.Fail(new EmailFailedError());
  }
}

interface IEmailClient
{
  Task SendMailAsync(MailMessage message);
}

class SmtpEmailClient : IEmailClient, IDisposable
{
  private readonly SmtpClient _smtpClient;

  public SmtpEmailClient(IOptions<SmtpOptions> smtpOptions)
  {
    var smtpOptionsValue = smtpOptions.Value;
    _smtpClient = new SmtpClient(smtpOptionsValue.SmtpAddress, smtpOptionsValue.SmtpPort);

    if (string.IsNullOrEmpty(smtpOptionsValue.SenderPassword) is false)
    {
      _smtpClient.Credentials = new NetworkCredential(smtpOptionsValue.SenderEmail, smtpOptionsValue.SenderPassword);
      _smtpClient.EnableSsl = true;
    }
  }

  public void Dispose()
  {
    _smtpClient.Dispose();
  }

  public async Task SendMailAsync(MailMessage message) =>
    await _smtpClient.SendMailAsync(message);
}

class SmtpOptions
{
  public string SmtpAddress { get; set; } = string.Empty;
  public int SmtpPort { get; set; }
  public string SenderEmail { get; set; } = string.Empty;
  public string SenderPassword { get; set; } = string.Empty;
}

class EmailFailedError : Error
{
  public EmailFailedError() : base("Failed to send email")
  {
  }
}

class SmtpOptionsSetup(IConfiguration configuration) : IConfigureOptions<SmtpOptions>
{
  private const string SectionName = nameof(SmtpOptions);
  private readonly IConfiguration _configuration = configuration;

  public void Configure(SmtpOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}