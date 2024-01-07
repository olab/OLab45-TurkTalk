using AutoMapper;
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

public class ConferenceMapper : OLabMapper<TtalkConference, Conference>
{
  public ConferenceMapper(
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
      cfg.CreateMap<TtalkConference, Conference>().ReverseMap();
      cfg.CreateMap<TtalkConferenceTopic, ConferenceTopicHelper>().ReverseMap();
      cfg.CreateMap<TtalkTopicParticipant, TopicParticipant>().ReverseMap();
    });
  }
}
