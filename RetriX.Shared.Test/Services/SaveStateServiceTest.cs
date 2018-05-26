using Moq;
using Plugin.FileSystem;
using RetriX.Shared.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RetriX.Shared.Test.Services
{
    public class SaveStateServiceTest : TestBase<SaveStateService>, IDisposable
    {
        private const int InitializationDelayMs = 50;

        private const string StateSavedToSlotMessageTitle = "Title";
        private const string StateSavedToSlotMessageBody = "Body {0}";

        private const string GameId = nameof(GameId);
        private const uint SlotID = 4;

        static readonly byte[] TestSavePayload = Enumerable.Range(0, byte.MaxValue).Select(d => (byte)d).ToArray();

        protected override SaveStateService InstantiateTarget()
        {
            var output = new SaveStateService(CrossFileSystem.Current);
            output.SetGameId(GameId);
            return output;
        }

        public void Dispose()
        {
            Target.ClearSavesAsync().Wait();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task InvalidIdIsHandledCorrectly(string gameId)
        {
            await Task.Delay(InitializationDelayMs);

            Target.SetGameId(gameId);
            var result = await Target.SlotHasDataAsync(SlotID);
            Assert.False(result);

            var stream = await Target.GetStreamForSlotAsync(SlotID, FileAccess.ReadWrite);
            Assert.Null(stream);

            stream = await Target.GetStreamForSlotAsync(SlotID, FileAccess.Read);
            Assert.Null(stream);
        }

        [Fact]
        public async Task SaveFilesAreCreatedWhenNeeded()
        {
            await Task.Delay(InitializationDelayMs);

            var slotHasData = await Target.SlotHasDataAsync(SlotID);
            Assert.False(slotHasData);

            var stream = await Target.GetStreamForSlotAsync(SlotID, FileAccess.Read);
            Assert.Null(stream);

            using (stream = await Target.GetStreamForSlotAsync(SlotID, FileAccess.ReadWrite))
            {
                Assert.NotNull(stream);
            }

            slotHasData  = await Target.SlotHasDataAsync(SlotID);
            Assert.True(slotHasData);

            using (stream = await Target.GetStreamForSlotAsync(SlotID, FileAccess.Read))
            {
                Assert.NotNull(stream);
            }
        }

        [Fact]
        public async Task DifferentSlotsAreIndependent()
        {
            await Task.Delay(InitializationDelayMs);

            using (var stream = await Target.GetStreamForSlotAsync(SlotID, FileAccess.ReadWrite))
            {
                Assert.NotNull(stream);
                await stream.WriteAsync(TestSavePayload, 0, TestSavePayload.Length);
            }

            var otherSlotID = SlotID + 1;
            using (var stream = await Target.GetStreamForSlotAsync(otherSlotID, FileAccess.ReadWrite))
            {
                Assert.NotNull(stream);
            }

            using (var stream = await Target.GetStreamForSlotAsync(SlotID, FileAccess.Read))
            {
                Assert.NotNull(stream);
                Assert.Equal(TestSavePayload.Length, stream.Length);
            }

            using (var stream = await Target.GetStreamForSlotAsync(otherSlotID, FileAccess.Read))
            {
                Assert.NotNull(stream);
                Assert.Equal(0, stream.Length);
            }
        }

        [Fact]
        public async Task ConcurrentOperationsAreBlocked()
        {
            await Task.Delay(InitializationDelayMs);

            var otherSlotID = SlotID + 1;
            var loadTasks = new Task<Stream>[]
            {
                Target.GetStreamForSlotAsync(SlotID, FileAccess.ReadWrite),
                Target.GetStreamForSlotAsync(otherSlotID, FileAccess.ReadWrite)
            };

            await Task.WhenAll(loadTasks);

            var stream = loadTasks[0].Result;
            Assert.NotNull(stream);
            stream.Dispose();

            stream = loadTasks[1].Result;
            Assert.Null(stream);
        }
    }
}
