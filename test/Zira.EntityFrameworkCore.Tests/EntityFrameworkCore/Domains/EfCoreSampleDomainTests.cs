using Zira.Samples;
using Xunit;

namespace Zira.EntityFrameworkCore.Domains;

[Collection(ZiraTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<ZiraEntityFrameworkCoreTestModule>
{

}
