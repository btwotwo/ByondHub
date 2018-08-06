using System;
using AutoFixture;
using ByondHub.Core.Configuration;
using ByondHub.Core.Server;
using ByondHub.Core.Server.ServerState;
using ByondHub.Core.Utility.Byond;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ByondHub.Tests.HubTests
{
    public partial class ServerInstanceTests
    {
        private readonly Fixture _fixture;
        private readonly Config _config;
        private readonly Mock<IServerUpdater> _updaterMock;
        private readonly Mock<IByondWrapper> _byondMock;
        private readonly Mock<ILogger<ServerInstance>> _loggerMock;
        private readonly Mock<IDreamDaemonProcess> _dreamDaemonProcessMock;

        public ServerInstanceTests()
        {
            _fixture = new Fixture();
            _config = _fixture.Create<Config>();
            _updaterMock = new Mock<IServerUpdater>();
            _byondMock = new Mock<IByondWrapper>();
            _loggerMock = new Mock<ILogger<ServerInstance>>();
            _dreamDaemonProcessMock = new Mock<IDreamDaemonProcess>();
        }

        [Fact]
        public void StartServer_CorrectStart_CallsByondWithCorrectPath()
        {
            var build = _fixture
                .Build<BuildModel>()
                .With(x => x.Path, "/testdir/build")
                .With(x => x.ExecutableName, "testExec").Create();


            DreamDaemonArguments args = null;
            _byondMock.Setup(x => x.StartDreamDaemon(It.IsAny<DreamDaemonArguments>()))
                .Callback<DreamDaemonArguments>(x => args = x);

            var instance = CreateServerInstance(build);

            instance.Start(1234);

            Assert.NotNull(args);
            Assert.Equal("/testdir/build/testExec.dmb", args.ExecutablePath);
            Assert.Equal(1234, args.Port);
        }

        [Fact]
        public void StartServer_CorrectStart_ReturnsSuccessObject()
        {
            var build = CreateBuildModel();
            build.Id = "testId";


            _byondMock.Setup(x => x.StartDreamDaemon(It.IsAny<DreamDaemonArguments>()))
                .Returns(_dreamDaemonProcessMock.Object);
            var instance = CreateServerInstance(build);

            var result = instance.Start(1234);

            Assert.NotNull(result);
            Assert.Equal("testId", result.Id);
            Assert.Equal(1234, result.Port);
        }

        [Fact]
        public void StartServer_ErrorHapenned_ReturnsError()
        {
            var build = CreateBuildModel();
            build.Id = "testId";

            _byondMock.Setup(x => x.StartDreamDaemon(It.IsAny<DreamDaemonArguments>()))
                .Throws(new InvalidOperationException("Bad error happened!"));

            var instance = CreateServerInstance(build);

            var result = instance.Start(1234);
            
            Assert.NotNull(result);
            Assert.True(result.Error);
            Assert.Equal("Failed to start server. Exception: \"Bad error happened!\"", result.ErrorMessage);
            Assert.Equal("testId", result.Id);
        }

        [Fact]
        public void StartServer_Correct_SetsState()
        {
            var build = CreateBuildModel();

            _byondMock.Setup(x => x.StartDreamDaemon(It.IsAny<DreamDaemonArguments>())).Returns(_dreamDaemonProcessMock.Object);

            var instance = CreateServerInstance(build);

            instance.Start(1234);

            Assert.IsType<StartedServerState>(instance.State);
        }

        [Fact]
        public void StartServer_StartTwoTimes_ReturnsError()
        {
            var build = CreateBuildModel();
            build.Id = "test";
            _byondMock.Setup(x => x.StartDreamDaemon(It.IsAny<DreamDaemonArguments>()))
                .Returns(_dreamDaemonProcessMock.Object);

            var instance = CreateServerInstance(build);
            instance.Start(1234);

            var error = instance.Start(1234);

            Assert.NotNull(error);
            Assert.True(error.Error);
            Assert.Equal("Server is already started.", error.ErrorMessage);
        }

        [Fact]
        public void StartServer_SuccessStart_HandlesUnexpectedExit()
        {
            var build = CreateBuildModel();
            build.Id = "test";

            _byondMock.Setup(x => x.StartDreamDaemon(It.IsAny<DreamDaemonArguments>()))
                .Returns(_dreamDaemonProcessMock.Object);
            var instance = CreateServerInstance(build);
            instance.Start(1234);
            Assert.IsType<StartedServerState>(instance.State);

            _dreamDaemonProcessMock.Raise(x => x.UnexpectedExit += null, _dreamDaemonProcessMock.Object, 111);

            Assert.IsType<StoppedServerState>(instance.State);

        }

        private BuildModel CreateBuildModel()
        {
            return _fixture.Create<BuildModel>();
        }

        private ServerInstance CreateServerInstance(BuildModel build)
        {
            return new ServerInstance(build, _updaterMock.Object, _byondMock.Object, new OptionsWrapper<Config>(_config), _loggerMock.Object);
        }
    }
}
