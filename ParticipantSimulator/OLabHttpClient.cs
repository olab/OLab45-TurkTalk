using Microsoft.AspNetCore.Mvc;
using NLog;
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
    private readonly Settings _settings;
    private readonly ILogger _logger;
    private HttpClient _client = new HttpClient();

    public OLabHttpClient(WorkerThreadParameter param, AuthenticateResponse? authInfo)
    {
      _settings = param.Settings;
      _logger = param.Logger;
      _client.BaseAddress = new Uri($"{_settings.OLabRestApiUrl}/");

      _logger.Debug($"OLab base address: {_client.BaseAddress}");

      if (authInfo != null)
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authInfo.AuthInfo.Token}");
    }

    public async Task<AuthenticateResponse?> LoginAsync(LoginRequest model)
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

    public async Task LoadMapAsync(uint mapId)
    {
      var url = $"maps/{mapId}";
      _logger.Debug($"get map url: {url}");
      HttpResponseMessage response = await _client.GetAsync(url);
      response.EnsureSuccessStatusCode();

      url = $"maps/{mapId}/scopedObjects";
      _logger.Debug($"get map objects url: {url}");
      response = await _client.GetAsync(url);
      response.EnsureSuccessStatusCode();
    }

    public async Task PlayMapNodeAsync(uint mapId, uint nodeId)
    {
      try
      {
        var url = $"maps/{mapId}/node/{nodeId}";
        _logger.Debug($"get node url: {url}");
        var model = new DynamicScopedObjectsDto
        {
          Map = null,
          Server = null,
          Node = null
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync(url, model);
        response.EnsureSuccessStatusCode();

        url = $"nodes/{nodeId}/scopedObjects";
        _logger.Debug($"get node objects url: {url}");
        response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

      }
      catch (Exception ex)
      {
        _logger.Error(ex);
        throw;
      }

      return;
    }

  }
}
