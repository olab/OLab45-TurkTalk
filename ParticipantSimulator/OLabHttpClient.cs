using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
using NLog;
using NLog.Web.LayoutRenderers;
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

namespace OLab.TurkTalk.ParticipantSimulator
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
      _client.Timeout = TimeSpan.FromSeconds(60);

      _logger.Debug($"{_param.Participant.UserId}: url: {_client.BaseAddress} timeout: {_client.Timeout.TotalMilliseconds} ms");
    }

    public async Task<AuthenticateResponse> LoginAsync(LoginRequest model)
    {
      var url = $"auth/login";

      _logger.Debug($"{model.Username}: login. url: {url} timeout: {_client.Timeout.TotalMilliseconds} ms");

      HttpResponseMessage response = null;

      int tries = 10;

      while (tries-- > 0)
      {
        try
        {
          response = await _client.PostAsJsonAsync(
              url,
              model);
          response.EnsureSuccessStatusCode();

          // Deserialize the updated product from the response body.
          var loginResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<AuthenticateResponse>));

          if (loginResponse != null)
          {
            AuthenticateResponse authResponse = ( loginResponse as OLabAPIResponse<AuthenticateResponse> ).Data;
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse.AuthInfo.Token}");
            return authResponse;
          }
        }
        catch (Exception ex)
        {
          _logger.Warn($"{model.Username}: login exception: {ex.Message}. Tries remaining {tries}");
          Thread.Sleep(1000);
        }
      }

      return null;
    }

    public async Task<MapsFullDto> LoadMapAsync(uint mapId)
    {
      var url = $"maps/{mapId}";

      _logger.Debug($"{_param.Participant.UserId}: get map url: {url} timeout: {_client.Timeout.TotalMilliseconds} ms");

      int tries = 5;

      while (tries-- > 0)
      {
        try
        {
          HttpResponseMessage response = await _client.GetAsync(url);
          response.EnsureSuccessStatusCode();

          var apiResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<MapsFullDto>));
          if (apiResponse != null)
          {
            var olabApiResponse = apiResponse as OLabAPIResponse<MapsFullDto>;
            return olabApiResponse.Data;
          }

        }
        catch (Exception ex)
        {
          _logger.Warn($"{_param.Participant.UserId}: load map exception: {ex.Message} Tries remaining {tries}");
          Thread.Sleep(1000);
        }
      }

      return null;
    }

    public async Task<OLabWebAPI.Dto.Designer.ScopedObjectsDto> LoadMapScopedObjectsAsync(uint mapId)
    {
      var url = $"maps/{mapId}/scopedObjects";

      int tries = 5;

      while (tries-- > 0)
      {
        try
        {
          var response = await _client.GetAsync(url);
          response.EnsureSuccessStatusCode();

          var apiResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<OLabWebAPI.Dto.Designer.ScopedObjectsDto>));
          if (apiResponse != null)
          {
            var olabApiResponse = apiResponse as OLabAPIResponse<OLabWebAPI.Dto.Designer.ScopedObjectsDto>;
            return olabApiResponse.Data;
          }
        }
        catch (Exception ex)
        {
          _logger.Warn($"{_param.Participant.UserId}: load map scoped exception: {ex.Message} Tries remaining {tries}");
          Thread.Sleep(1000);
        }

      }

      return null;
    }

    public async Task<MapsNodesFullRelationsDto> LoadMapNodeAsync(MapTrail mapTrail, NodeTrail nodeTrail = null)
    {
      uint mapId = mapTrail.MapId;
      uint nodeId = nodeTrail != null ? nodeTrail.NodeId : 0;
      _logger.Info($"{_param.Participant.UserId}: playing node: {mapId}/{nodeId}");

      var url = $"maps/{mapId}/node/{nodeId}";

      var model = new DynamicScopedObjectsDto
      {
        Map = null,
        Server = null,
        Node = null
      };

      int tries = 5;

      while (tries-- > 0)
      {
        try
        {
          HttpResponseMessage response = await _client.PostAsJsonAsync(url, model);
          response.EnsureSuccessStatusCode();

          var apiResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<MapsNodesFullRelationsDto>));

          if (apiResponse != null)
          {
            var olabApiResponse = apiResponse as OLabAPIResponse<MapsNodesFullRelationsDto>;
            _client.DefaultRequestHeaders.Add("OLabSessionId", olabApiResponse.Data.ContextId);

            return olabApiResponse.Data;
          }
        }
        catch (Exception ex)
        {
          _logger.Warn($"{_param.Participant.UserId}: load map node exception: {ex.Message} Tries remaining {tries}");
          Thread.Sleep(1000);
        }
      }

      return null;
    }

    public async Task<ScopedObjectsDto> LoadMapNodeScopedAsync(MapTrail mapTrail, NodeTrail nodeTrail = null)
    {
      uint mapId = mapTrail.MapId;
      uint nodeId = nodeTrail != null ? nodeTrail.NodeId : 0;

      var url = $"nodes/{nodeId}/scopedObjects";

      int tries = 5;

      while (tries-- > 0)
      {
        try
        {
          var response = await _client.GetAsync(url);
          response.EnsureSuccessStatusCode();

          var apiResponse = await response.Content.ReadFromJsonAsync(typeof(OLabAPIResponse<ScopedObjectsDto>));

          if (apiResponse != null)
          {
            var olabApiResponse = apiResponse as OLabAPIResponse<ScopedObjectsDto>;
            return olabApiResponse.Data;
          }
        }
        catch (Exception ex)
        {
          _logger.Warn($"{_param.Participant.UserId}: load node scoped exception: {ex.Message} Tries remaining {tries}");
          Thread.Sleep(1000);
        }
      }

      return null;
    }
  }
}
