using OLab.Api.Model;

namespace OLab.TurkTalk.ParticipantSimulator
{
  public partial class ParticipantThread
  {
    public async Task<AuthenticateResponse> OLabLoginStepAsync()
    {
      var sleepMs = _param.Rnd.Next( 0, _param.Participant.GetDelayMs( _param.Settings ) );

      _logger.Info( $"{_param.Participant.UserId}: logging in" );

      // pause for a random time up to a max time 
      Thread.Sleep( sleepMs );

      var loginResult = await _olabClient.LoginAsync( new LoginRequest
      {
        Username = _param.Participant.UserId,
        Password = _param.Participant.Password
      } );

      _logger.Info( $"{_param.Participant.UserId}: logged into OLab" );

      return loginResult;
    }
  }
}