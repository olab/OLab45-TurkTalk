using AutoMapper;
using Microsoft.CodeAnalysis.FlowAnalysis;
using OLab.Common.Interfaces;
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

  public override Conference PhysicalToDto(TtalkConference phys)
  {
    var topicMapper = new ConferenceTopicMapper(Logger);

    var dto = base.PhysicalToDto(phys);
    foreach (var topic in phys.TtalkConferenceTopics)
      dto.AddTopic(topicMapper.PhysicalToDto(topic, dto));

    return dto;
  }
}
