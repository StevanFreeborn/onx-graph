namespace Server.API.Email;

interface IEmailService
{
  Task SendEmailAsync(EmailMessage message);
}