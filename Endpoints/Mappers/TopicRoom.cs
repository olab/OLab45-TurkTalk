using AutoMapper;
using OLab.Common.Interfaces;
using OLab.Data.Mappers;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.TurkTalk.Endpoints.Mappers;

public class TopicRoomMapper : OLabMapper<TtalkTopicRoom, TopicRoom>
{
  public TopicRoomMapper(
    IOLabLogger logger,
    bool enableWikiTranslation = true) : base(logger)
  {

  }

  public TopicRoom PhysicalToDto(TtalkTopicRoom phys, ConferenceTopic topic)
  {
    var dto = PhysicalToDto(phys);

    if (dto != null)
      dto.Topic = topic;

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
      cfg.CreateMap<TtalkTopicRoom, TopicRoom>().ReverseMap();
      cfg.CreateMap<TtalkConferenceTopic, ConferenceTopic>().ReverseMap();      
      cfg.CreateMap<TtalkConference, Conference>().ReverseMap();      
      cfg.CreateMap<TtalkTopicParticipant, TopicParticipant>().ReverseMap();      
    });
  }
}
