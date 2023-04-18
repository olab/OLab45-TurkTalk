using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
using NLog;
using NuGet.Protocol.Plugins;
using OLabWebAPI.Common;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.ModeratorSimulator
{
  public class OLabHttpClient
  {
    private WorkerThreadParameter _param;
    private readonly Settings _settings;
    private readonly ILogger _logger;
    private HttpClient _client = new HttpClient();

    public OLabHttpClient(WorkerThreadParameter param, AuthenticateResponse authInfo)
    {
      _param = param;
      _settings = param.Settings;
      _logger = param.Logger;
      _client.BaseAddress = new Uri($"{_settings.OLabRestApiUrl}/");

      _logger.Debug($"OLab base address: {_client.BaseAddress}");

      if (authInfo != null)
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authInfo.AuthInfo.Token}");
    }

    public async Task<AuthenticateResponse> LoginAsync(LoginRequest model)
    {
      var url = $"auth/login";
      _logger.Debug($"login url: {url}");
      HttpResponseMessage response = await _client.PostAsJsonAsync(
          url,
          model);
      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var loginResponse = await response.Content.ReadFromJsonAsync(typeof(AuthenticateResponse));
      if (loginResponse != null)
        return loginResponse as AuthenticateResponse;

      return null;
    }

    public async Task<MapsFullDto> LoadMapAsync(uint mapId)
    {
      var url = $"maps/{mapId}";

      _logger.Debug($"{_param.Moderator.UserId}: get map url: {url}");

      HttpResponseMessage response = await _client.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var apiResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<MapsFullDto>));
      if (apiResponse != null)
      {
        var olabApiResponse = apiResponse as OLabAPIResponse<MapsFullDto>;
        return olabApiResponse.Data;
      }

      return null;
    }

    public async Task<OLabWebAPI.Dto.Designer.ScopedObjectsDto> LoadMapScopedObjectsAsync(uint mapId)
    {
      var url = $"maps/{mapId}/scopedObjects";

      var response = await _client.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var apiResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<OLabWebAPI.Dto.Designer.ScopedObjectsDto>));
      if (apiResponse != null)
      {
        var olabApiResponse = apiResponse as OLabAPIResponse<OLabWebAPI.Dto.Designer.ScopedObjectsDto>;
        return olabApiResponse.Data;
      }

      return null;
    }

    public async Task<MapsNodesFullRelationsDto> LoadMapNodeAsync(MapTrail mapTrail, NodeTrail nodeTrail = null)
    {
      uint mapId = mapTrail.MapId;
      uint nodeId = nodeTrail != null ? nodeTrail.NodeId : 0;
      _logger.Info($"Playing node: {mapId}/{nodeId}");

      var url = $"maps/{mapId}/node/{nodeId}";

      var model = new DynamicScopedObjectsDto
      {
        Map = null,
        Server = null,
        Node = null
      };

      HttpResponseMessage response = await _client.PostAsJsonAsync(url, model);
      response.EnsureSuccessStatusCode();

      var apiResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<MapsNodesFullRelationsDto>));

      if (apiResponse != null)
      {
        var olabApiResponse = apiResponse as OLabAPIResponse<MapsNodesFullRelationsDto>;
        return olabApiResponse.Data;
      }

      return null;
    }

    public async Task<ScopedObjectsDto> LoadMapNodeScopedAsync(MapTrail mapTrail, NodeTrail nodeTrail = null)
    {
      uint mapId = mapTrail.MapId;
      uint nodeId = nodeTrail != null ? nodeTrail.NodeId : 0;

      var url = $"nodes/{nodeId}/scopedObjects";

      var response = await _client.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var apiResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<ScopedObjectsDto>));

      if (apiResponse != null)
      {
        var olabApiResponse = apiResponse as OLabAPIResponse<ScopedObjectsDto>;
        return olabApiResponse.Data;
      }

      return null;
    }
  }
}
