using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Effects
{
    class BasicUOEffect : Effect
    {
        public BasicUOEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice, Resources.IsometricEffect)
        {
            MatrixTransform = Parameters["MatrixTransform"];
            WorldMatrix = Parameters["WorldMatrix"];
            Viewport = Parameters["Viewport"];
            // MobileUO: NOTE: Since we don't parse the mojoshader to read the properties, Brightlight doesn't exist as a key in the Parameters dictionary
            Parameters.Add("Brightlight", new EffectParameter());
            Brighlight = Parameters["Brightlight"];

            CurrentTechnique = Techniques["HueTechnique"];
            //Pass = CurrentTechnique.Passes[0];
        }

        public EffectParameter MatrixTransform { get; }
        public EffectParameter WorldMatrix { get; }
        public EffectParameter Viewport { get; }
        public EffectParameter Brighlight { get; }
        public EffectPass Pass { get; }
    }
}
