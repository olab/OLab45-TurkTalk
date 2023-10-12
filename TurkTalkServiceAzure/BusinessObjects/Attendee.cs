using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.BusinessObjects;
public class Attendee
{
  public string ConferenceName { get; set; }
  public string TopicName { get; set; }
  public string RoomName { get; set; }
  public int RoomInstance { get; set; }

  public Attendee()
  {

  }

}
