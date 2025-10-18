using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using OLab.Api.Model;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Api.TurkTalk.Commands;
using OLab.Api.TurkTalk.Methods;

namespace OLab.TurkTalk.ModeratorSimulator
{
  public class SignalRRoom
  {
    private readonly IList<Learner> _learners;
    private readonly WorkerThreadParameter _param;
    private readonly global::NLog.ILogger _logger;
    private readonly AuthenticateResponse _authInfo;

    public SignalRRoom(
      WorkerThreadParameter param,
      global::NLog.ILogger logger,
      AuthenticateResponse authInfo)
    {
      _learners = new List<Learner>();
      _param = param;
      _logger = logger;
      _authInfo = authInfo;
    }

  }

  public partial class ModeratorThread
  {
    private IList<Learner> _atriumLearners;
    private SignalRRoom _room;
    private static readonly Mutex atriumMutex = new Mutex();

    private void MethodCallbacks()
    {
      _connection.On<object>( "Command", (payload) =>
      {
        var json = payload.ToString();

        var commandMethod = JsonConvert.DeserializeObject<CommandMethod>( json );
        _logger.Info( $"{_param.Moderator.UserId}: command received: {commandMethod.Command}" );

        if ( commandMethod.Command == "atriumupdate" )
          OnAtriumUpdateCommand( json );

        else if ( commandMethod.Command == "moderatorassignment" )
          OnModeratorAssignmentCommand( json );

        else if ( commandMethod.Command == "learnerlist" )
          OnLearnerListCommand( json );

        else
          _logger.Error( $"{_param.Moderator.UserId}: unimplmented command: {commandMethod.Command}" );

        return Task.CompletedTask;
      } );

      _connection.On<string, string, string>( "message", (data, sessionId, from) =>
      {
        _logger.Info( $"{_param.Moderator.UserId}: message {data} from {from}" );
        return Task.CompletedTask;
      } );

    }

    private void OnLearnerListCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<LearnerListCommand>( json );
      var learners = commandMethod.Data;

      _logger.Info( $"{_param.Moderator.UserId}: existing learners count: {learners.Count}" );
    }

    private void OnAtriumUpdateCommand(string json)
    {
      try
      {
        atriumMutex.WaitOne();

        var commandMethod = JsonConvert.DeserializeObject<AtriumUpdateCommand>( json );
        _atriumLearners = commandMethod.Data;

        _logger.Info( $"{_param.Moderator.UserId}: atrium update received: {commandMethod.Command}" );
        _logger.Info( $"{_param.Moderator.UserId}: atrium contents count: {_atriumLearners.Count}" );

        var autoAcceptResult = AcceptLearnersAsync();
        autoAcceptResult.Wait();

        _logger.Info( $"{_param.Moderator.UserId}: atrium update completed" );

      }
      finally
      {
        atriumMutex.ReleaseMutex();
      }
    }

    private async Task AcceptLearnersAsync()
    {
      foreach ( var atriumLearner in _atriumLearners )
      {
        _logger.Info( $"{_param.Moderator.UserId}: testing atrium user: {atriumLearner.UserId}" );

        // look for atrium user in participants list
        var roomLearner = _turkTalkTrail.Participants.FirstOrDefault( x => x.UserId == atriumLearner.UserId );

        if ( roomLearner != null )
        {
          if ( _turkTalkTrail.AutoAccept || roomLearner.AutoAccept )
          {
            _logger.Info( $"{_param.Moderator.UserId}: assigning atrium user: {atriumLearner.UserId} to {_roomName}" );

            await _connection.InvokeAsync(
              "assignattendee",
              atriumLearner.ToJson(),
              _roomName );

            var pauseMs = _turkTalkTrail.GetDelayMs( _param.Settings );
            Thread.Sleep( pauseMs );
          }
        }
        else
          _logger.Info( $"{_param.Moderator.UserId}: skipping atrium user: {atriumLearner.UserId}" );
      }
    }

    private void OnModeratorAssignmentCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<AtriumAssignmentCommand>( json );
      _roomAssigned = true;
    }

  }
}
