using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Editor
{
    /// <summary>
    /// Renders an <see cref="System.Windows.Controls.Image"/> in greyscale. 
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;Image.Effect&gt;
    ///     &lt;effect:GrayscaleEffect /&gt;
    /// &lt;/Image.Effect&gt;
    /// </code>
    /// </example>
    public class GrayscaleEffect : ShaderEffect
    {
        private static readonly PixelShader Shader = new PixelShader
        {
            UriSource = Global.GetPackUri("Shaders/GrayscaleEffect.ps")
        };

        public GrayscaleEffect()
        {
            PixelShader = Shader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(DesaturationFactorProperty);
        }

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(GrayscaleEffect), 0);
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty DesaturationFactorProperty = DependencyProperty.Register("DesaturationFactor", typeof(double), typeof(GrayscaleEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0), CoerceDesaturationFactor));
        public double DesaturationFactor
        {
            get { return (double)GetValue(DesaturationFactorProperty); }
            set { SetValue(DesaturationFactorProperty, value); }
        }

        private static object CoerceDesaturationFactor(DependencyObject d, object value)
        {
            var effect = (GrayscaleEffect)d;
            var newFactor = (double)value;

            if (newFactor < 0.0 || newFactor > 1.0)
            {
                return effect.DesaturationFactor;
            }

            return newFactor;
        }
    }
}