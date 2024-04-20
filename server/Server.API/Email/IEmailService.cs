namespace Server.API.Email;

/// <summary>
/// A service for sending emails
/// </summary>
interface IEmailService
{
  /// <summary>
  /// Sends an email
  /// </summary>
  /// <param name="message">The email message to send</param>
  /// <returns>A <see cref="Result"/> indicating the outcome of the operation</returns>
  Task<Result> SendEmailAsync(EmailMessage message);
}