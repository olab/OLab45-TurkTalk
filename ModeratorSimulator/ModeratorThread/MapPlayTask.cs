using OLab.Api.Dto;

namespace OLab.TurkTalk.ModeratorSimulator
{
  public partial class ModeratorThread
  {
    private MapsFullDto _map;
    private MapsNodesFullRelationsDto _node;
    private OLab.Api.Dto.Designer.ScopedObjectsDto _mapScoped;
    private OLab.Api.Dto.Designer.ScopedObjectsDto _nodeScoped;

    public async Task<bool> MapPlayTaskAsync()
    {
      var mapTrail = _param.Moderator.GetMapTrail( _param.Settings );

      var olabClient = new OLabHttpClient( _param, _authInfo );

      _map = await olabClient.LoadMapAsync( mapTrail.MapId );
      _mapScoped = await olabClient.LoadMapScopedObjectsAsync( mapTrail.MapId );

      // if no node trail, load root node
      if ( mapTrail.NodeTrail == null )
      {
        var sleepMs = _param.Rnd.Next( 0, mapTrail.GetDelayMs( _param.Settings ) );
        _logger.Debug( $"{_param.Moderator.UserId}: sleeping for {sleepMs} ms to play {mapTrail.MapId}/0" );
        Thread.Sleep( sleepMs );

        _node = await olabClient.LoadMapNodeAsync( mapTrail );
        _nodeScoped = await olabClient.LoadMapScopedObjectsAsync( mapTrail.MapId );

        return true;
      }

      foreach ( var nodeTrail in mapTrail.NodeTrail )
      {
        var sleepMs = mapTrail.GetDelayMs( _param.Settings );
        _logger.Debug( $"{_param.Moderator.UserId}: sleeping for {sleepMs} ms to play {mapTrail.MapId}/{nodeTrail.NodeId}" );
        Thread.Sleep( sleepMs );

        _node = await olabClient.LoadMapNodeAsync( mapTrail, nodeTrail );
        _nodeScoped = await olabClient.LoadMapScopedObjectsAsync( mapTrail.MapId );

        await SignalRTask( nodeTrail );
      }

      return true;
    }
  }
}