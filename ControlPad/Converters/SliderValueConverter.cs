namespace ControlPad.Converters
{
    public static class SliderValueConverter
    {
        public static float SliderToFloat(int value, double translationExponent)
        {
            value -= 1;
            float normalized = System.Math.Clamp((float)value / 1022.0f, 0f, 1f);

            if (normalized < 0.005f)
                return 0f;

            return (float)System.Math.Pow(normalized, translationExponent);
        }
    }
}
