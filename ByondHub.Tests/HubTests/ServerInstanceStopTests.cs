using ByondHub.Core.Server.ServerState;
using ByondHub.Core.Utility.Byond;
using Moq;
using Xunit;

namespace ByondHub.Tests.HubTests
{
    public partial class ServerInstanceTests
    {
        [Fact]
        public void Stop_CorrectData_CallsKill()
        {
            var build = CreateBuildModel();

            _byondMock.Setup(x => x.StartDreamDaemon(It.IsAny<DreamDaemonArguments>()))
                .Returns(_dreamDaemonProcessMock.Object);

            var instance = CreateServerInstance(build);
            instance.Start(1324);

            instance.Stop();
            _dreamDaemonProcessMock.Verify(x => x.Kill(), Times.Once);
        }

        [Fact]
        public void Stop_Correct_SetsState()
        {
            var build = CreateBuildModel();

            _byondMock.Setup(x => x.StartDreamDaemon(It.IsAny<DreamDaemonArguments>()))
                .Returns(_dreamDaemonProcessMock.Object);

            var instance = CreateServerInstance(build);
            instance.Start(1234);

            Assert.IsType<StartedServerState>(instance.State);

            instance.Stop();

            Assert.IsType<StoppedServerState>(instance.State);
        }
    }
}
