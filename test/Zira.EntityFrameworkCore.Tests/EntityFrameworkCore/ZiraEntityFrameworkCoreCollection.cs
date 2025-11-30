using Xunit;

namespace Zira.EntityFrameworkCore;

[CollectionDefinition(ZiraTestConsts.CollectionDefinitionName)]
public class ZiraEntityFrameworkCoreCollection : ICollectionFixture<ZiraEntityFrameworkCoreFixture>
{

}
