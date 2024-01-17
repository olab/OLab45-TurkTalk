﻿using AutoMapper;
using OLab.Common.Interfaces;
using OLab.Data.Mappers;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.TurkTalk.Endpoints.Mappers;

public class TopicParticipantMapper : OLabMapper<TtalkTopicParticipant, TopicParticipant>
{
  public TopicParticipantMapper(
    IOLabLogger logger,
    bool enableWikiTranslation = true) : base(logger)
  {

  }

  /// <summary>
  /// Default (overridable) AutoMapper cfg
  /// </summary>
  /// <returns>MapperConfiguration</returns>
  protected override MapperConfiguration GetConfiguration()
  {
    return new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<TtalkTopicParticipant, TopicParticipant>().ReverseMap();
    });
  }
}
