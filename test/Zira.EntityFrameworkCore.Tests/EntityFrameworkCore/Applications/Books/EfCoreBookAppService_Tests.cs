using Zira.Books;
using Xunit;

namespace Zira.EntityFrameworkCore.Applications.Books;

[Collection(ZiraTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<ZiraEntityFrameworkCoreTestModule>
{

}