using System.Collections.ObjectModel;
using ControlPad;

namespace ControlPad.Tests
{
    public class SliderCategoryTests
    {
        [Fact]
        public void Constructor_InitializesNameIdAndEmptyAudioStreams()
        {
            var category = new SliderCategory("Voice", 12);

            Assert.Equal("Voice", category.Name);
            Assert.Equal(12, category.Id);
            Assert.NotNull(category.AudioStreams);
            Assert.Empty(category.AudioStreams);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            var category = new SliderCategory("Music", 4);

            Assert.Equal("Music", category.ToString());
        }

        [Fact]
        public void ChangingIdNameAudioStreams_RaisesPropertyChangedForEach()
        {
            var category = new SliderCategory("Old", 1);
            var changed = new List<string?>();
            category.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            category.Id = 5;
            category.Name = "New";
            category.AudioStreams = new ObservableCollection<AudioStream> { new AudioStream("spotify", null) };

            Assert.Contains(nameof(SliderCategory.Id), changed);
            Assert.Contains(nameof(SliderCategory.Name), changed);
            Assert.Contains(nameof(SliderCategory.AudioStreams), changed);
        }

        [Fact]
        public void SettingSameValues_DoesNotRaisePropertyChanged()
        {
            var category = new SliderCategory("Same", 1);
            var sameCollection = category.AudioStreams;
            int events = 0;
            category.PropertyChanged += (_, _) => events++;

            category.Id = 1;
            category.Name = "Same";
            category.AudioStreams = sameCollection;

            Assert.Equal(0, events);
        }
    }
}
