namespace Server.API.Tests.Integration;

public class SmtpEmailClientTests : IAsyncLifetime
{
  private readonly IContainer _mailHogContainer = new ContainerBuilder()
    .WithImage("mailhog/mailhog")
    .WithPortBinding(1025, true)
    .WithPortBinding(8025, true)
    .Build();

  private IOptions<SmtpOptions> SmtpOptions =>
    Options.Create(
      new SmtpOptions
      {
        SmtpAddress = _mailHogContainer.Hostname,
        SmtpPort = _mailHogContainer.GetMappedPublicPort(1025),
        SenderEmail = "onxGraphTest@teset.com",
        SenderPassword = string.Empty
      }
    );

  private MailHogService MailHogService =>
    new(
      new HttpClient
      {
        BaseAddress = new($"http://{_mailHogContainer.Hostname}:{_mailHogContainer.GetMappedPublicPort(8025)}"),
      }
    );

  public async Task InitializeAsync()
  {
    await _mailHogContainer.StartAsync();
  }

  [Fact]
  public async Task SendMailAsync_WhenCalledWithEmail_ItShouldSendEmail()
  {
    var email = FakeDataFactory.EmailMessage.Generate();
    var emailParts = email.To.Split('@');
    var testMailbox = emailParts[0];
    var testDomain = emailParts[1];

    var mailMessage = new MailMessage(SmtpOptions.Value.SenderEmail, email.To)
    {
      Subject = email.Subject,
      Body = email.HtmlContent,
      IsBodyHtml = true,
    };

    using var sut = new SmtpEmailClient(SmtpOptions);

    await sut.SendMailAsync(mailMessage);

    var emailSearchResult = await MailHogService.SearchEmailAsync(new(MailHogSearchKind.To, email.To));
    emailSearchResult.Count.Should().Be(1);
    emailSearchResult.Items.Should().NotBeNullOrEmpty();
    emailSearchResult.Items.First().To
      .Should()
      .ContainSingle(t =>
        t.Mailbox == testMailbox && t.Domain == testDomain
      );
  }

  [Fact]
  public async Task Dispose_WhenCalled_ItShouldDisposeSmtpClient()
  {
    var sut = new SmtpEmailClient(SmtpOptions);

    sut.Dispose();

    var action = () => sut.SendMailAsync(new MailMessage());

    await action.Should().ThrowAsync<ObjectDisposedException>();
  }

  async Task IAsyncLifetime.DisposeAsync()
  {
    await _mailHogContainer.DisposeAsync();
  }
}