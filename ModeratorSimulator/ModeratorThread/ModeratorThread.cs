using OLab.Api.Model;
using OLab.Api.TurkTalk.BusinessObjects;

namespace OLab.TurkTalk.ModeratorSimulator
{
  public partial class ModeratorThread
  {
    private readonly WorkerThreadParameter _param;
    private readonly global::NLog.ILogger _logger;
    private AuthenticateResponse _authInfo;
    private readonly Learner _learner;

    public ModeratorThread(WorkerThreadParameter param)
    {
      _param = param;
      _logger = param.Logger;
    }

    public void RunProc()
    {

      try
      {
        var loginTask = OLabLoginStepAsync();
        loginTask.Wait();

        _authInfo = loginTask.Result;
        if ( _authInfo == null )
        {
          _logger.Error( $"{_param.Moderator.UserId}: unable to login" );
          return;
        }

        _room = new SignalRRoom( _param, _logger, _authInfo );

        var mapPlayTask = MapPlayTaskAsync();
        mapPlayTask.Wait();

      }
      catch ( Exception ex )
      {
        _logger.Error( $"{_param.Moderator.UserId}: exception '{ex.Message}'" );
      }
      finally
      {
        // decrement the countdown event timer because 
        // the thread work has completed.
        _param.CountdownEvent.Signal();
      }

    }
  }
}
