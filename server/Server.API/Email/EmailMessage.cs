/// <summary>
/// Represents an email message
/// </summary>
class EmailMessage
{
  /// <summary>
  /// The email address to send the email to
  /// </summary>
  public string To { get; set; } = string.Empty;

  /// <summary>
  /// The subject of the email
  /// </summary>
  public string Subject { get; set; } = string.Empty;

  /// <summary>
  /// The plain text content of the email
  /// </summary>
  public string HtmlContent { get; set; } = string.Empty;
}