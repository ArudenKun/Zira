using Clerk.Net.Client.Models;
using Mediator;

namespace Application.Features.Users.Queries;

internal sealed class GetUserQueryHandler : IQueryHandler<GetUserQuery, User>
{
    public ValueTask<User> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
