using Mediator;

namespace Application.Features.Users.Commands;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    public ValueTask<Unit> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
