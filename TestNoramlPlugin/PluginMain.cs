using projectFrameCut.Render.RenderAPIBase.ClipAndTrack;
using projectFrameCut.Render.RenderAPIBase.EffectAndMixture;
using projectFrameCut.Render.RenderAPIBase.Plugins;
using projectFrameCut.Render.RenderAPIBase.Sources;
using System.Text.Json;

namespace nobody
{
    public partial class MyExamplePlugin : IPluginBase
    {
        Dictionary<string, Dictionary<string, string>> IPluginBase.LocalizationProvider => new();

        Dictionary<string, Func<string, string, ISoundTrack>> IPluginBase.SoundTrackProvider => new();

        Dictionary<string, Func<Guid, Guid, ITransform>> IPluginBase.TransformProvider => new();

        Dictionary<string, Func<IEffect>> IPluginBase.EffectProvider => new();

        Dictionary<string, IEffectFactory> IPluginBase.EffectFactoryProvider => new();

        Dictionary<string, Func<IEffect>> IPluginBase.ContinuousEffectProvider => new();

        Dictionary<string, IEffectFactory> IPluginBase.ContinuousEffectFactoryProvider => new();

        Dictionary<string, Func<IEffect>> IPluginBase.BindableArgumentEffectProvider => new();

        Dictionary<string, IEffectFactory> IPluginBase.BindableArgumentEffectFactoryProvider => new();

        Dictionary<string, Func<IComputer>> IPluginBase.ComputerProvider => new();

        Dictionary<string, Func<string, IVideoSource>> IPluginBase.VideoSourceProvider => new();

        Dictionary<string, Func<string, IAudioSource>> IPluginBase.AudioSourceProvider => new();

        Dictionary<string, Func<string, IVideoWriter>> IPluginBase.VideoWriterProvider => new();

        Dictionary<string, string> IPluginBase.Configuration { get; set; } = new();

        Dictionary<string, Dictionary<string, string>> IPluginBase.ConfigurationDisplayString => new();

        public IClip ClipCreator(JsonElement element)
        {
            throw new NotImplementedException();
        }

        public ISoundTrack SoundTrackCreator(JsonElement element)
        {
            throw new NotImplementedException();
        }
    }
}
