using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
using NLog;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Model;
using System.Net.Http.Json;

namespace OLab.TurkTalk.ModeratorSimulator
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

      _logger.Debug( $"OLab base address: {_client.BaseAddress}" );

      if ( authInfo != null )
        _client.DefaultRequestHeaders.Add( "Authorization", $"Bearer {authInfo.AuthInfo.Token}" );
    }

    public async Task<AuthenticateResponse> LoginAsync(LoginRequest model)
    {
      var url = $"auth/login";
      _logger.Debug( $"login url: {url}" );
      var response = await _client.PostAsJsonAsync(
          url,
          model );
      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var loginResponse = await response.Content.ReadFromJsonAsync( typeof( AuthenticateResponse ) );
      if ( loginResponse != null )
        return loginResponse as AuthenticateResponse;

      return null;
    }

    public async Task<MapsFullDto> LoadMapAsync(uint mapId)
    {
      var url = $"maps/{mapId}";

      _logger.Debug( $"{_param.Moderator.UserId}: get map url: {url}" );

      var response = await _client.GetAsync( url );
      response.EnsureSuccessStatusCode();

      var apiResponse = await response.Content.ReadFromJsonAsync( typeof( OLabApiResult<MapsFullDto> ) );
      if ( apiResponse != null )
      {
        var OLabApiResult = apiResponse as OLabApiResult<MapsFullDto>;
        return OLabApiResult.Data;
      }

      return null;
    }

    public async Task<OLab.Api.Dto.Designer.ScopedObjectsDto> LoadMapScopedObjectsAsync(uint mapId)
    {
      var url = $"maps/{mapId}/scopedObjects";

      var response = await _client.GetAsync( url );
      response.EnsureSuccessStatusCode();

      var apiResponse = await response.Content.ReadFromJsonAsync( typeof( OLabApiResult<OLab.Api.Dto.Designer.ScopedObjectsDto> ) );
      if ( apiResponse != null )
      {
        var OLabApiResult = apiResponse as OLabApiResult<OLab.Api.Dto.Designer.ScopedObjectsDto>;
        return OLabApiResult.Data;
      }

      return null;
    }

    public async Task<MapsNodesFullRelationsDto> LoadMapNodeAsync(MapTrail mapTrail, NodeTrail nodeTrail = null)
    {
      var mapId = mapTrail.MapId;
      var nodeId = nodeTrail != null ? nodeTrail.NodeId : 0;
      _logger.Info( $"Playing node: {mapId}/{nodeId}" );

      var url = $"maps/{mapId}/node/{nodeId}";

      var response = await _client.PostAsJsonAsync( url, new DynamicScopedObjectsDto() );
      response.EnsureSuccessStatusCode();

      var apiResponse = await response.Content.ReadFromJsonAsync( typeof( OLabApiResult<MapsNodesFullRelationsDto> ) );

      if ( apiResponse != null )
      {
        var OLabApiResult = apiResponse as OLabApiResult<MapsNodesFullRelationsDto>;
        return OLabApiResult.Data;
      }

      return null;
    }

    public async Task<ScopedObjectsDto> LoadMapNodeScopedAsync(MapTrail mapTrail, NodeTrail nodeTrail = null)
    {
      var mapId = mapTrail.MapId;
      var nodeId = nodeTrail != null ? nodeTrail.NodeId : 0;

      var url = $"nodes/{nodeId}/scopedObjects";

      var response = await _client.GetAsync( url );
      response.EnsureSuccessStatusCode();

      var apiResponse = await response.Content.ReadFromJsonAsync( typeof( OLabApiResult<ScopedObjectsDto> ) );

      if ( apiResponse != null )
      {
        var OLabApiResult = apiResponse as OLabApiResult<ScopedObjectsDto>;
        return OLabApiResult.Data;
      }

      return null;
    }
  }
}
