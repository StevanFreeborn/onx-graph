namespace Server.API.Authentication;

/// <summary>
/// A repository for managing tokens in MongoDB.
/// </summary>
/// <inheritdoc cref="ITokenRepository"/>
class MongoTokenRepository(
  MongoDbContext context,
  TimeProvider timeProvider
) : ITokenRepository
{
  private readonly MongoDbContext _context = context;
  private readonly TimeProvider _timeProvider = timeProvider;

  public async Task<BaseToken> CreateTokenAsync(BaseToken token)
  {
    await _context.Tokens.InsertOneAsync(token);
    return token;
  }

  public async Task<BaseToken?> GetTokenAsync(string refreshToken)
  {
    return await _context.Tokens
      .Find(x => x.Token == refreshToken)
      .FirstOrDefaultAsync();
  }

  public async Task RemoveAllInvalidRefreshTokensAsync(string userId)
  {
    var filter = Builders<BaseToken>.Filter.And(
      Builders<BaseToken>.Filter.Eq(x => x.UserId, userId),
      Builders<BaseToken>.Filter.Eq(x => x.TokenType, TokenType.Refresh),
      Builders<BaseToken>.Filter.Or(
        Builders<BaseToken>.Filter.Eq(x => x.Revoked, true),
        Builders<BaseToken>.Filter.Lt(x => x.ExpiresAt, _timeProvider.GetUtcNow().DateTime)
      )
    );

    await _context.Tokens.DeleteManyAsync(filter);
  }

  public Task RevokeAllRefreshTokensForUserAsync(string userId)
  {
    var filter = Builders<BaseToken>.Filter.And(
      Builders<BaseToken>.Filter.Eq(x => x.UserId, userId),
      Builders<BaseToken>.Filter.Eq(x => x.TokenType, TokenType.Refresh)
    );

    var update = Builders<BaseToken>.Update.Set(x => x.Revoked, true);

    return _context.Tokens.UpdateManyAsync(filter, update);
  }

  public async Task UpdateTokenAsync(BaseToken updatedToken)
  {
    var filter = Builders<BaseToken>.Filter.Eq(x => x.Id, updatedToken.Id);
    await _context.Tokens.ReplaceOneAsync(filter, updatedToken);
  }

  public Task RevokeUserVerificationTokensAsync(string userId)
  {
    throw new NotImplementedException();
  }
}