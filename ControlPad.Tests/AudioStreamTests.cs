using ControlPad;

namespace ControlPad.Tests
{
    public class AudioStreamTests
    {
        [Fact]
        public void Constructor_UsesMicNameAsDisplayName_WhenMicNameProvided()
        {
            var stream = new AudioStream("spotify", "USB Mic");

            Assert.Equal("USB Mic", stream.DisplayName);
        }

        [Fact]
        public void Constructor_UsesProcessAsDisplayName_WhenOnlyProcessProvided()
        {
            var stream = new AudioStream("discord", null);

            Assert.Equal("discord", stream.DisplayName);
        }

        [Fact]
        public void Constructor_UsesMainAudio_WhenProcessAndMicAreNull()
        {
            var stream = new AudioStream(null, null);

            Assert.Equal("Main Audio", stream.DisplayName);
        }

        [Fact]
        public void Constructor_PrioritizesMicNameOverProcess_WhenBothProvided()
        {
            var stream = new AudioStream("game", "Headset Mic");

            Assert.Equal("Headset Mic", stream.DisplayName);
        }
    }
}
