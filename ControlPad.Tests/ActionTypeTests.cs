using ControlPad;

namespace ControlPad.Tests
{
    public class ActionTypeTests
    {
        [Fact]
        public void Constructor_SetsTypeAndDescription()
        {
            var actionType = new ActionType(EActionType.OpenWebsite, "Open Website");

            Assert.Equal(EActionType.OpenWebsite, actionType.Type);
            Assert.Equal("Open Website", actionType.Description);
        }

        [Fact]
        public void Enum_DefinesExpectedActionTypes()
        {
            var values = Enum.GetValues<EActionType>();

            Assert.Contains(EActionType.MuteProcess, values);
            Assert.Contains(EActionType.MuteMainAudio, values);
            Assert.Contains(EActionType.MuteMic, values);
            Assert.Contains(EActionType.OpenProcess, values);
            Assert.Contains(EActionType.OpenWebsite, values);
            Assert.Contains(EActionType.KeyPress, values);
            Assert.Equal(6, values.Length);
        }
    }
}
