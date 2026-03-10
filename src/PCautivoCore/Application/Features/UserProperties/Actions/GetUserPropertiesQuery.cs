using PCautivoCore.Application.Features.UserProperties.Dtos;
using PCautivoCore.Domain.Interfaces;
using PCautivoCore.Shared.Responses;
using MediatR;

namespace PCautivoCore.Application.Features.UserProperties.Actions;

public record GetUserPropertiesQuery(int UserId) : IRequest<Result<IEnumerable<UserPropertyDto>>>;

public class GetUserPropertiesQueryHandler(
    IUserPropertyRepository userPropertyRepository
) : IRequestHandler<GetUserPropertiesQuery, Result<IEnumerable<UserPropertyDto>>>
{
    public async Task<Result<IEnumerable<UserPropertyDto>>> Handle(GetUserPropertiesQuery request, CancellationToken cancellationToken)
    {
        var properties = await userPropertyRepository.GetUserPropertiesWithDetails(request.UserId);

        return properties.ToList();
    }
}
