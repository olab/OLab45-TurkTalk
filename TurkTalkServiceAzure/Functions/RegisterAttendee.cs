using Azure.Core.Serialization;
using Dawn;
using DotnetIsolated_ClassBased;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.SignalR.Management;
using OLab.Api.Common.Contracts;
using OLab.Api.Model;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Service.Azure.BusinessObjects;
using OLab.TurkTalk.Service.Azure.Interfaces;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.Functions;

public partial class Functions
{
  [Function("RegisterAttendee")]
  public async Task RegisterAttendeeAsync(
    [SignalRTrigger("Hub", "messages", "RegisterAttendee", "payload")]
        SignalRInvocationContext invocationContext,
    FunctionContext functionContext,
    RegisterAttendeePayload payload)
  {
    Learner learner = null;
    Room room = null;

    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));
      Guard.Argument(payload.RoomName).NotNull("RoomName");
    }
    catch (System.Exception)
    {

      throw;
    }
  }
}