using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.Api.Common.Contracts;

public class ModeratorAssignmentPayload
{
  public IList<MapNodeListItem> MapNodes { get; set; }
  public Moderator Remote { get; set; }

}
