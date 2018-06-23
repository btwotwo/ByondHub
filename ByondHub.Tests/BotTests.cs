using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ByondHub.DiscordBot.Core.Server.Modules;
using ByondHub.DiscordBot.Core.Server.Services;
using ByondHub.Shared.Server;
using ByondHub.Shared.Server.Updates;
using Discord;
using Moq;
using Xunit;

namespace ByondHub.Tests
{
    public class DisordBotServerModuleTests
    {
        [Fact]
        public async void ServerModule_ServerStartSuccess_ReturnsSuccessMessage()
        {
            const string expectedMessage = "Server 'test': Server started. Port: 1234";

            var mockedRequester = new Mock<IServerRequester>();
            var successStartResult =
                new ServerStartStopResult {Error = false, Id = "test", Message = "Server started.", Port = 1234};
            mockedRequester.Setup(x => x.SendStartRequestAsync("test", 1234)).ReturnsAsync(successStartResult);

            var serverModule = new ServerModuleWrapper(mockedRequester.Object);
            await serverModule.StartServerAsync("test", 1234);

            Assert.Equal(serverModule.LastMessage, expectedMessage);
        }

        [Fact]
        public async void ServerModule_ServerStartFail_ReturnsFailMessage()
        {
            const string expectedMessage = "Server 'test' error: Test Error Message";
            var mockedRequester = new Mock<IServerRequester>();
            var failStartResult =
                new ServerStartStopResult {Error = true, ErrorMessage = "Test Error Message", Id = "test", Port = 1234};
            mockedRequester.Setup(x => x.SendStartRequestAsync("test", 1234)).ReturnsAsync(failStartResult);

            var serverModule = new ServerModuleWrapper(mockedRequester.Object);
            await serverModule.StartServerAsync("test", 1234);

            Assert.Equal(expectedMessage, serverModule.LastMessage);
        }

        [Fact]
        public async void ServerModule_ServerStopSuccess_ReturnsSuccessMessage()
        {
            const string expectedMessage = "Server 'test': Server stopped.";
            var mockedRequester = new Mock<IServerRequester>();
            var successStopResult =
                new ServerStartStopResult {Error = false, Id = "test", Message = "Server stopped.", Port = 1234};
            mockedRequester.Setup(x => x.SendStopRequestAsync("test")).ReturnsAsync(successStopResult);

            var serverModule = new ServerModuleWrapper(mockedRequester.Object);
            await serverModule.StopServerAsync("test");

            Assert.Equal(expectedMessage, serverModule.LastMessage);
        }

        [Fact]
        public async void ServerModule_ServerUpdateNotUpToDate_ReturnsSuccessMessage()
        {
            const string expectedMessage =
                "Server \"test\" was compiled on branch \"test-branch\" and on commit \"aaabbbccc\" (Test commit).\nBuild log:\nOUTPUT";
            var mockedRequester = new Mock<IServerRequester>();
            var successUpdateResult = new UpdateResult()
            {
                Branch = "test-branch",
                CommitHash = "aaabbbccc",
                CommitMessage = "Test commit",
                Id = "test",
                Output = "OUTPUT"
            };
            mockedRequester.Setup(x => x.SendUpdateRequestAsync("test", "master", "")).ReturnsAsync(successUpdateResult);

            var serverModule = new ServerModuleWrapper(mockedRequester.Object);
            await serverModule.UpdateServerAsync("test");

            Assert.Equal(expectedMessage, serverModule.LastMessage);
        }

        [Fact]
        public async void ServerModule_ServerUpdateUpToDate_ReturnsSuccessMessage()
        {
            const string expectedMessage =
                "Update request for \"test\" is finished." +
                " Server is up-to-date on branch \"test-branch\" and on commit \"aaabbbccc\" (Test commit).";
            var mockedRequester = new Mock<IServerRequester>();
            var successUpdateResult = new UpdateResult()
            {
                Branch = "test-branch",
                CommitHash = "aaabbbccc",
                CommitMessage = "Test commit",
                Id = "test",
                UpToDate = true
            };
            mockedRequester.Setup(x => x.SendUpdateRequestAsync("test", "master", "")).ReturnsAsync(successUpdateResult);

            var serverModule = new ServerModuleWrapper(mockedRequester.Object);
            await serverModule.UpdateServerAsync("test");
            
            Assert.Equal(expectedMessage, serverModule.LastMessage);
        }

        [Fact]
        public async void ServerModule_ServerUpdateFail_ReturnsFailMessage()
        {
            const string expectedMessage = "Update request for \"test\" is finished. Got error: ERROR.";
            var failUpdateResult = new UpdateResult() {Error = true, ErrorMessage = "ERROR."};
            var mockedRequester = new Mock<IServerRequester>();
            mockedRequester.Setup(x => x.SendUpdateRequestAsync("test", "master", "")).ReturnsAsync(failUpdateResult);

            var serverModule = new ServerModuleWrapper(mockedRequester.Object);
            await serverModule.UpdateServerAsync("test");

            Assert.Equal(expectedMessage, serverModule.LastMessage);
        }
    }

    internal class ServerModuleWrapper : ServerModule
    {
        public string LastMessage { get; set; }
        public ServerModuleWrapper(IServerRequester requester) : base(requester, null, null)
        {}
        protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            LastMessage = message;
            return new DummyUserMessage();
        }
    }

    internal class DummyUserMessage : IUserMessage
    {
        public ulong Id { get; }
        public DateTimeOffset CreatedAt { get; }
        public Task DeleteAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public MessageType Type { get; }
        public MessageSource Source { get; }
        public bool IsTTS { get; }
        public bool IsPinned { get; }
        public string Content { get; }
        public DateTimeOffset Timestamp { get; }
        public DateTimeOffset? EditedTimestamp { get; }
        public IMessageChannel Channel { get; }
        public IUser Author { get; }
        public IReadOnlyCollection<IAttachment> Attachments { get; }
        public IReadOnlyCollection<IEmbed> Embeds { get; }
        public IReadOnlyCollection<ITag> Tags { get; }
        public IReadOnlyCollection<ulong> MentionedChannelIds { get; }
        public IReadOnlyCollection<ulong> MentionedRoleIds { get; }
        public IReadOnlyCollection<ulong> MentionedUserIds { get; }
        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task PinAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task UnpinAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllReactionsAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IUser>> GetReactionUsersAsync(string emoji, int limit = 100, ulong? afterUserId = null, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name,
            TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions { get; }
    }
}
