using NSubstitute;
using ProjectBuilder.Repositories;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder
{
    public abstract class AppServiceTestBase
    {
        protected IObjectMapper ObjectMapper { get; }
        protected IAraEnumerationRepository AraEnumerationRepository { get; }
        protected IBridgeToPavementsRepository BridgeToPavementsRepository { get; }
        protected IImportSessionsRepository ImportSessionsRepository { get; }

        protected AppServiceTestBase()
        {
            ObjectMapper = Substitute.For<IObjectMapper>();
            AraEnumerationRepository = Substitute.For<IAraEnumerationRepository>();
            BridgeToPavementsRepository = Substitute.For<IBridgeToPavementsRepository>();
            ImportSessionsRepository = Substitute.For<IImportSessionsRepository>();
        }
    }
}