using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.Utils;
using System.Security.Claims;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class NewConnectionPayload
    {
      public string ConnectionId { get; }

      public string UserKey { get; }

      public NewConnectionPayload(
        IOLabConfiguration configuration,
        string connectionId, 
        IOLabAuthentication auth)
      {
        ConnectionId = connectionId;
        UserKey = new UserInfoEncoder().EncryptUser(
          configuration.GetAppSettings().Secret,
          auth.Claims["id"],
          auth.Claims[ClaimTypes.Name],
          auth.Claims["name"],
          auth.Claims["iss"]);
      }
    }
