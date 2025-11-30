using Zira.Samples;
using Xunit;

namespace Zira.EntityFrameworkCore.Applications;

[Collection(ZiraTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<ZiraEntityFrameworkCoreTestModule>
{

}
