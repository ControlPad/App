namespace ControlPad
{
    public static class SliderTranslationCurve
    {
        private const double Epsilon = 1e-6;

        public static bool IsSupportedPreset(string? preset)
        {
            return preset is "ease" or "linear" or "ease-in" or "ease-out" or "ease-in-out" or "custom";
        }

        public static (double x1, double y1, double x2, double y2) GetPresetControlPoints(string preset)
        {
            return preset switch
            {
                "ease" => (0.25d, 0.1d, 0.25d, 1d),
                "linear" => (0d, 0d, 1d, 1d),
                "ease-in" => (0.42d, 0d, 1d, 1d),
                "ease-out" => (0d, 0d, 0.58d, 1d),
                "ease-in-out" => (0.42d, 0d, 0.58d, 1d),
                _ => (0d, 0d, 1d, 1d),
            };
        }

        public static float Apply(float input)
        {
            double t = Math.Clamp(input, 0f, 1f);
            var controlPoints = Settings.TranslationCurvePreset == "custom"
                ? (Settings.TranslationCurveX1, Settings.TranslationCurveY1, Settings.TranslationCurveX2, Settings.TranslationCurveY2)
                : GetPresetControlPoints(Settings.TranslationCurvePreset);

            return (float)Evaluate(controlPoints.Item1, controlPoints.Item2, controlPoints.Item3, controlPoints.Item4, t);
        }

        private static double Evaluate(double x1, double y1, double x2, double y2, double xTarget)
        {
            x1 = Math.Clamp(x1, 0d, 1d);
            x2 = Math.Clamp(x2, 0d, 1d);
            y1 = Math.Clamp(y1, 0d, 1d);
            y2 = Math.Clamp(y2, 0d, 1d);
            xTarget = Math.Clamp(xTarget, 0d, 1d);

            if (xTarget <= 0d)
                return 0d;
            if (xTarget >= 1d)
                return 1d;

            double lower = 0d;
            double upper = 1d;
            double t = xTarget;

            for (int i = 0; i < 20; i++)
            {
                t = (lower + upper) * 0.5d;
                double x = Bezier(t, 0d, x1, x2, 1d);
                if (Math.Abs(x - xTarget) < Epsilon)
                    break;
                if (x < xTarget)
                    lower = t;
                else
                    upper = t;
            }

            return Bezier(t, 0d, y1, y2, 1d);
        }

        private static double Bezier(double t, double p0, double p1, double p2, double p3)
        {
            double oneMinusT = 1d - t;
            return oneMinusT * oneMinusT * oneMinusT * p0
                + 3d * oneMinusT * oneMinusT * t * p1
                + 3d * oneMinusT * t * t * p2
                + t * t * t * p3;
        }
    }
}
