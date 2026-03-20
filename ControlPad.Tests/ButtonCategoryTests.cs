using ControlPad;

namespace ControlPad.Tests
{
    public class ButtonCategoryTests
    {
        [Fact]
        public void Constructor_InitializesNameIdAndEmptyActions()
        {
            var category = new ButtonCategory("Actions", 2);

            Assert.Equal("Actions", category.Name);
            Assert.Equal(2, category.Id);
            Assert.NotNull(category.ButtonActions);
            Assert.Empty(category.ButtonActions);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            var category = new ButtonCategory("Macros", 9);

            Assert.Equal("Macros", category.ToString());
        }

        [Fact]
        public void ButtonActions_CanAddAndRemoveEntries()
        {
            var category = new ButtonCategory("Media", 1);
            var actionType = new ActionType(EActionType.MuteMainAudio, "Mute");
            var action = new ButtonAction(actionType) { ActionProperty = null };

            category.ButtonActions.Add(action);
            Assert.Single(category.ButtonActions);

            category.ButtonActions.Remove(action);
            Assert.Empty(category.ButtonActions);
        }
    }
}
