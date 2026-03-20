using ControlPad.Converters;

namespace ControlPad.Tests
{
    public class SliderValueConverterTests
    {
        [Fact]
        public void SliderToFloat_ReturnsZero_ForRawValueOne()
        {
            var result = SliderValueConverter.SliderToFloat(1, 1d);

            Assert.Equal(0f, result);
        }

        [Fact]
        public void SliderToFloat_ReturnsOne_ForRawValue1023_WithExponentOne()
        {
            var result = SliderValueConverter.SliderToFloat(1023, 1d);

            Assert.Equal(1f, result, 5);
        }

        [Fact]
        public void SliderToFloat_ClampsToZero_ForValuesBelowThreshold()
        {
            var result = SliderValueConverter.SliderToFloat(5, 1d);

            Assert.Equal(0f, result);
        }

        [Fact]
        public void SliderToFloat_AppliesExponentCurve()
        {
            var linear = SliderValueConverter.SliderToFloat(512, 1d);
            var curved = SliderValueConverter.SliderToFloat(512, 2d);

            Assert.True(curved < linear);
        }

        [Fact]
        public void SliderToFloat_ClampsInputBelowRange()
        {
            var result = SliderValueConverter.SliderToFloat(-100, 1d);

            Assert.Equal(0f, result);
        }

        [Fact]
        public void SliderToFloat_ClampsInputAboveRange()
        {
            var result = SliderValueConverter.SliderToFloat(5000, 1d);

            Assert.Equal(1f, result, 5);
        }
    }
}
