using AutoMapper;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Profiles;

public class ReprocessorExporterProfile : Profile
{
    public ReprocessorExporterProfile()
    {
        CreateMap<Nation, NationDetailsResponseDto>().ReverseMap();

        CreateMap<Organisation, OrganisationDetailsResponseDto>()
            .ForMember(dest => dest.OrganisationName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.OrganisationType, opt => opt.MapFrom(src => src.OrganisationType!.Name))
            .ForMember(dest => dest.RegisteredAddress, opt => opt.MapFrom(src => CreateAddressString(src)))
            .ForMember(dest => dest.Persons, opt => opt.MapFrom(src => src.PersonOrganisationConnections));
        
        CreateMap<PersonOrganisationConnection, OrganisationPersonDto>()
			.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Person!.User!.UserId))
			.ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Person!.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Person!.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Person!.Email))
            .ForMember(dest => dest.TelephoneNumber, opt => opt.MapFrom(src => src.Person!.Telephone))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobTitle))
            .ForMember(dest => dest.ServiceRole, opt => opt.MapFrom(src => src.Enrolments.FirstOrDefault(e => e.ConnectionId == src.Id && !e.IsDeleted).ServiceRole.Name));
    }

    private static string CreateAddressString(Organisation RegisteredAddress)
    {
        string firstAddressLine = string.Join(" ", new[] { RegisteredAddress.BuildingNumber, RegisteredAddress.BuildingName }.Where(addressPart => !string.IsNullOrEmpty(addressPart)));
        return string.Join(
            ", ",
            new[]
            {
                firstAddressLine,
                RegisteredAddress.SubBuildingName,
                RegisteredAddress.Street,
                RegisteredAddress.Locality,
                RegisteredAddress.Town,
                RegisteredAddress.Country,
                RegisteredAddress.Postcode
            }.Where(addressPart => !string.IsNullOrEmpty(addressPart)));
    }
}
