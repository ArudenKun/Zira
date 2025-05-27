using Application.Abstractions.Caching;
using Clerk.Net.Client.Models;

namespace Application.Features.Users.Queries;

public sealed class GetUserQuery : ICacheQuery<User> { }
