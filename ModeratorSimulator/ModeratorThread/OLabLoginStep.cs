using OLab.Api.Model;

namespace OLab.TurkTalk.ModeratorSimulator
{
  public partial class ModeratorThread
  {
    public async Task<AuthenticateResponse> OLabLoginStepAsync()
    {
      var sleepMs = _param.Rnd.Next( 0, _param.Moderator.GetDelayMs( _param.Settings ) );

      _logger.Debug( $"{_param.Moderator.UserId}: sleeping for {sleepMs} ms" );

      // pause for a random time up to a max time 
      Thread.Sleep( sleepMs );

      _logger.Info( $"{_param.Moderator.UserId}: logging in" );

      var olabClient = new OLabHttpClient( _param, null );
      var loginResult = await olabClient.LoginAsync( new LoginRequest
      {
        Username = _param.Moderator.UserId,
        Password = _param.Moderator.Password
      } );

      _logger.Info( $"{_param.Moderator.UserId}: logged into OLab" );

      return loginResult;
    }
  }
}