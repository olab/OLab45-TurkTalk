using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.CodeAnalysis.FlowAnalysis;
using OLab.Api.Models;
using OLab.Common.Interfaces;
using OLab.Data.Dtos;
using OLab.Data.Mappers;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.Mappers;

public class ConferenceTopicMapper : OLabMapper<TtalkConferenceTopic, ConferenceTopic>
{
  public ConferenceTopicMapper(
    IOLabLogger logger,
    bool enableWikiTranslation = true) : base(logger)
  {

  }

  public ConferenceTopic PhysicalToDto(
    TtalkConferenceTopic phys, 
    string topicName,
    Conference conference)
  {
    var dto = base.PhysicalToDto(phys);
    dto.Name = topicName;

    // load the topic atrium
    dto.Conference = conference;
    var atriumAttendees = dto.Attendees.Where(x => x.RoomId == 0).ToList();
    dto.Atrium.Load(atriumAttendees);

    return dto;
  }

  /// <summary>
  /// Default (overridable) AutoMapper cfg
  /// </summary>
  /// <returns>MapperConfiguration</returns>
  protected override MapperConfiguration GetConfiguration()
  {
    return new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<TtalkConference, Conference>()
        .ReverseMap();
      cfg.CreateMap<TtalkConferenceTopic, ConferenceTopic>()
        .ForMember(dest => dest.Attendees, opt => opt.MapFrom(src => src.TtalkTopicParticipants))
        .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.TtalkTopicRooms))
        .ReverseMap();
      cfg.CreateMap<TtalkTopicParticipant, TopicParticipant>()
        .ReverseMap();
      cfg.CreateMap<TtalkTopicRoom, TopicRoom>()
        .ReverseMap();
    });
  }
}
