namespace Server.API.Email;

interface IEmailService
{
  Task<Result> SendEmailAsync(EmailMessage message);
}