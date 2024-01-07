using AutoMapper;
using OLab.Common.Interfaces;
using OLab.Data.Mappers;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;

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
    Conference conference)
  {
    var dto = base.PhysicalToDto(phys);

    dto.Name = phys.Name;
    dto.Conference = conference;

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
        .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.TtalkTopicRooms))
        .ReverseMap();
      cfg.CreateMap<TtalkTopicParticipant, TopicParticipant>()
        .ReverseMap();
      cfg.CreateMap<TtalkTopicRoom, TopicRoom>()
        .ReverseMap();
    });
  }
}
