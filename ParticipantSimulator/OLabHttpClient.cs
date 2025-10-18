using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

//using Newtonsoft.Json;
using NLog;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Model;
using System.Net.Http.Json;

namespace OLab.TurkTalk.ParticipantSimulator
{
  public class OLabHttpClient
  {
    private readonly WorkerThreadParameter _param;
    private readonly Settings _settings;
    private readonly ILogger _logger;
    private readonly HttpClient _client = new HttpClient();

    public OLabHttpClient(WorkerThreadParameter param, AuthenticateResponse authInfo)
    {
      _param = param;
      _settings = param.Settings;
      _logger = param.Logger;
      _client.BaseAddress = new Uri( $"{_settings.OLabRestApiUrl}/" );
      _client.Timeout = TimeSpan.FromSeconds( 60 );

      _logger.Debug( $"{_param.Participant.UserId}: url: {_client.BaseAddress} timeout: {_client.Timeout.TotalMilliseconds} ms" );
    }

    public async Task<AuthenticateResponse> LoginAsync(LoginRequest model)
    {
      var url = $"auth/login";

      _logger.Debug( $"{model.Username}: login. url: {url}" );

      HttpResponseMessage response = null;

      var tries = _param.Settings.ApiRetryCount;

      while ( tries-- > 0 )
      {
        try
        {
          response = await _client.PostAsJsonAsync(
              url,
              model );
          response.EnsureSuccessStatusCode();

          string jsonString = await response.Content.ReadAsStringAsync();
          // Deserialize the updated product from the response body.
          var loginResponse = JsonConvert.DeserializeObject<OLabApiResult<AuthenticateResponse>>( jsonString );

          if ( loginResponse != null )
          {
            var authResponse = (loginResponse as OLabApiResult<AuthenticateResponse>).Data;
            _client.DefaultRequestHeaders.Add( "Authorization", $"Bearer {authResponse.AuthInfo.Token}" );
            _client.DefaultRequestHeaders.Add( "OLabSessionId", string.Empty );
            return authResponse;
          }
        }
        catch ( Exception ex )
        {
          _logger.Warn( $"{model.Username}: login exception: {ex.Message}. Tries remaining {tries}" );
          Thread.Sleep( _param.Settings.PauseMs.GetDelayMs( 1000, 3000 ) );
        }
      }

      return null;
    }

    public async Task<MapsFullDto> LoadMapAsync(uint mapId)
    {
      var url = $"maps/{mapId}";

      _logger.Debug( $"{_param.Participant.UserId}: get map url: {url} timeout: {_client.Timeout.TotalMilliseconds} ms" );

      var tries = _param.Settings.ApiRetryCount;

      while ( tries-- > 0 )
      {
        try
        {
          var response = await _client.GetAsync( url );
          response.EnsureSuccessStatusCode();

          string jsonString = await response.Content.ReadAsStringAsync();
          var apiResponse = JsonConvert.DeserializeObject<OLabApiResult<MapsFullDto>>( jsonString );

          if ( apiResponse != null )
          {
            var OLabApiResult = apiResponse as OLabApiResult<MapsFullDto>;
            return OLabApiResult.Data;
          }

        }
        catch ( Exception ex )
        {
          _logger.Warn( $"{_param.Participant.UserId}: load map exception: {ex.Message} Tries remaining {tries}" );
          Thread.Sleep( _param.Settings.PauseMs.GetDelayMs( 1000, 3000 ) );
        }
      }

      return null;
    }

    public async Task<OLab.Api.Dto.Designer.ScopedObjectsDto> LoadMapScopedObjectsAsync(uint mapId)
    {
      var url = $"maps/{mapId}/scopedObjects";

      var tries = _param.Settings.ApiRetryCount;

      while ( tries-- > 0 )
      {
        try
        {
          var response = await _client.GetAsync( url );
          response.EnsureSuccessStatusCode();

          string jsonString = await response.Content.ReadAsStringAsync();
          var apiResponse = JsonConvert.DeserializeObject<OLabApiResult<OLab.Api.Dto.Designer.ScopedObjectsDto>>( jsonString );

          if ( apiResponse != null )
          {
            var OLabApiResult = apiResponse as OLabApiResult<OLab.Api.Dto.Designer.ScopedObjectsDto>;
            return OLabApiResult.Data;
          }
        }
        catch ( Exception ex )
        {
          _logger.Warn( $"{_param.Participant.UserId}: load map scoped exception: {ex.Message} Tries remaining {tries}" );
          Thread.Sleep( _param.Settings.PauseMs.GetDelayMs( 1000, 3000 ) );
        }

      }

      return null;
    }

    public async Task<MapsNodesFullRelationsDto> LoadMapNodeAsync(MapTrail mapTrail, NodeTrail nodeTrail = null)
    {
      var mapId = mapTrail.MapId;
      var nodeId = nodeTrail != null ? nodeTrail.NodeId : 0;
      _logger.Info( $"{_param.Participant.UserId}: playing node: {mapId}/{nodeId}" );

      var url = $"maps/{mapId}/node/{nodeId}";

      var tries = _param.Settings.ApiRetryCount;

      while ( tries-- > 0 )
      {
        try
        {
          var body = new DynamicScopedObjectsDto
          {
            NewPlay = true
          };

          var response = await _client.PostAsJsonAsync( url, body );
          response.EnsureSuccessStatusCode();

          string jsonString = await response.Content.ReadAsStringAsync();
          var apiResponse = JsonConvert.DeserializeObject<OLabApiResult<MapsNodesFullRelationsDto>>( jsonString );

          if ( apiResponse != null )
          {
            var OLabApiResult = apiResponse as OLabApiResult<MapsNodesFullRelationsDto>;
            _client.DefaultRequestHeaders.Add( "OLabSessionId", OLabApiResult.Data.ContextId );

            return OLabApiResult.Data;
          }
        }
        catch ( Exception ex )
        {
          _logger.Warn( $"{_param.Participant.UserId}: load map node {nodeId} exception: {ex.Message} Tries remaining {tries}" );
          Thread.Sleep( _param.Settings.PauseMs.GetDelayMs( 1000, 3000 ) );
        }
      }

      return null;
    }

    public async Task<ScopedObjectsDto> LoadMapNodeScopedAsync(MapTrail mapTrail, NodeTrail nodeTrail = null)
    {
      var mapId = mapTrail.MapId;
      var nodeId = nodeTrail != null ? nodeTrail.NodeId : 0;

      var url = $"nodes/{nodeId}/scopedObjects";

      var tries = _param.Settings.ApiRetryCount;

      while ( tries-- > 0 )
      {
        try
        {
          var response = await _client.GetAsync( url );
          response.EnsureSuccessStatusCode();

          string jsonString = await response.Content.ReadAsStringAsync();
          var apiResponse = JsonConvert.DeserializeObject<OLabApiResult<ScopedObjectsDto>>( jsonString );

          if ( apiResponse != null )
          {
            var OLabApiResult = apiResponse as OLabApiResult<ScopedObjectsDto>;
            return OLabApiResult.Data;
          }
        }
        catch ( Exception ex )
        {
          _logger.Warn( $"{_param.Participant.UserId}: load map node scoped {nodeId} exception: {ex.Message} Tries remaining {tries}" );
          Thread.Sleep( _param.Settings.PauseMs.GetDelayMs( 1000, 3000 ) );
        }
      }

      return null;
    }
  }
}
