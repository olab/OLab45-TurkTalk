namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class MapNodeListItem
{
  public uint Id { get; set; }
  public string Name { get; set; }

  public override string ToString()
  {
    return $"{Name}({Id})";
  }
}
