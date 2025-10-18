using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using OLab.Api.TurkTalk.Commands;
using OLab.Api.TurkTalk.Contracts;
using OLab.Api.TurkTalk.Methods;

namespace OLab.TurkTalk.ParticipantSimulator
{
  public partial class ParticipantThread
  {
    private void OnAtriumAassignedCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<AtriumAssignmentCommand>( json );
      _learner = commandMethod.Data;
    }

    private void OnRoomAssignmentCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<AtriumAssignmentCommand>( json );
      _roomAssigned = true;
    }

    private void OnRoomUnassignmentCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<AtriumAssignmentCommand>( json );
      _roomAssigned = false;
    }

    private void OnJumpRoomCommand(string json)
    {
      JumpNodePayload = JsonConvert.DeserializeObject<JumpNodePayload>( json );
    }

    private void MethodCallbacks(HubConnection connection)
    {
      connection.On<object>( "Command", (payload) =>
      {
        var json = payload.ToString();

        var commandMethod = JsonConvert.DeserializeObject<CommandMethod>( json );
        _logger.Info( $"{_param.Participant.UserId}: command received: {commandMethod.Command}" );

        if ( commandMethod.Command == "atriumassignment" )
          OnAtriumAassignedCommand( json );

        if ( commandMethod.Command == "roomassignment" )
          OnRoomAssignmentCommand( json );

        if ( commandMethod.Command == "learnerunassignment" )
          OnRoomUnassignmentCommand( json );

        if ( commandMethod.Command == "jumpnode" )
          OnJumpRoomCommand( json );

        return Task.CompletedTask;
      } );

      connection.On<string, string, string>( "message", (data, sessionId, from) =>
      {
        _logger.Info( $"{_param.Participant.UserId}: message {data} from {from}" );
        return Task.CompletedTask;
      } );

      connection.On<string, string, string>( "jumpnode", (data, sessionId, from) =>
      {
        _logger.Info( $"{_param.Participant.UserId}: jumpnode {data} from {from}" );
        return Task.CompletedTask;
      } );
    }

  }
}
