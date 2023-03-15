using OLabWebAPI.TurkTalk.Commands;

namespace HubServiceInterfaces;

#region IClock
public interface IClock
{
    Task Command(string payload);
}
#endregion
