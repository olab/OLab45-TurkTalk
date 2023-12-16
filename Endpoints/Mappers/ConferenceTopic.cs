using AutoMapper;
using OLab.Common.Interfaces;
using OLab.Data.Mappers;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
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

  /// <summary>
  /// This prevents using the mapper without the conference by mistake
  /// </summary>
  /// <param name="phys"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  public override ConferenceTopic PhysicalToDto(TtalkConferenceTopic phys)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Convert a physical object to new dto. 
  /// </summary>
  /// <remarks>
  /// Allows for derived class specific overrides that 
  /// don't fit well with default implementation
  /// </remarks>
  /// <param name="physTopic">Physical object</param>
  /// <returns>Dto object</returns>
  public virtual ConferenceTopic PhysicalToDto(TtalkConferenceTopic physTopic, Conference conference)
  {
    var dto = _mapper.Map<ConferenceTopic>(physTopic);
    dto = PhysicalToDto(physTopic, dto);

    dto.Conference = conference;

    return dto;
  }

  // <summary>
  // Default(overridable) AutoMapper cfg
  // </summary>
  // <returns>MapperConfiguration</returns>
  protected override MapperConfiguration GetConfiguration()
  {
    return new MapperConfiguration(cfg =>
     cfg.CreateMap<TtalkConferenceTopic, ConferenceTopic>()
      .ForMember(dest => dest.Conference, opt => opt.Ignore())
      .ReverseMap()
    );
  }
}
