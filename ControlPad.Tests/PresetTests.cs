using System.ComponentModel;
using ControlPad;

namespace ControlPad.Tests
{
    public class PresetTests
    {
        [Fact]
        public void Constructor_SetsInitialValues()
        {
            var preset = new Preset(2, "Gaming");

            Assert.Equal(2, preset.Id);
            Assert.Equal("Gaming", preset.Name);
        }

        [Fact]
        public void Id_SetToDifferentValue_RaisesPropertyChanged()
        {
            var preset = new Preset(1, "Default");
            PropertyChangedEventArgs? captured = null;
            preset.PropertyChanged += (_, e) => captured = e;

            preset.Id = 3;

            Assert.NotNull(captured);
            Assert.Equal(nameof(Preset.Id), captured!.PropertyName);
        }

        [Fact]
        public void Name_SetToDifferentValue_RaisesPropertyChanged()
        {
            var preset = new Preset(1, "Default");
            PropertyChangedEventArgs? captured = null;
            preset.PropertyChanged += (_, e) => captured = e;

            preset.Name = "Streaming";

            Assert.NotNull(captured);
            Assert.Equal(nameof(Preset.Name), captured!.PropertyName);
        }

        [Fact]
        public void SettingSameValues_DoesNotRaisePropertyChanged()
        {
            var preset = new Preset(1, "Default");
            int events = 0;
            preset.PropertyChanged += (_, _) => events++;

            preset.Id = 1;
            preset.Name = "Default";

            Assert.Equal(0, events);
        }
    }
}
