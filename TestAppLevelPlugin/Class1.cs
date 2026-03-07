using projectFrameCut.ApplicationAPIBase.Effect;
using projectFrameCut.ApplicationAPIBase.Plugins;
using projectFrameCut.Render.RenderAPIBase.ClipAndTrack;
using projectFrameCut.Render.RenderAPIBase.EffectAndMixture;
using projectFrameCut.Render.RenderAPIBase.Plugins;
using projectFrameCut.Render.RenderAPIBase.Sources;
using System.Text.Json;

namespace nobody
{
    // All the code in this file is included in all platforms.
    public partial class TestStandaloneAppLevelPlugin : IApplicationPluginBase
    {
        public Dictionary<string, Func<IEffectBundle>> EffectBundleProvider => new();

        Dictionary<string, Dictionary<string, string>> IPluginBase.LocalizationProvider => new();

        Dictionary<string, Func<string, string, IClip>> IPluginBase.ClipProvider => new();

        Dictionary<string, Func<string, string, ISoundTrack>> IPluginBase.SoundTrackProvider => new();

        Dictionary<string, Func<Guid, Guid, projectFrameCut.Render.RenderAPIBase.ClipAndTrack.ITransform>> IPluginBase.TransformProvider => new();

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

        IClip IPluginBase.ClipCreator(JsonElement element)
        {
            throw new NotImplementedException();
        }

        View? IApplicationPluginBase.SettingPageProvider(ref IApplicationPluginBase instance)
        {
            throw new NotImplementedException();
        }

        ISoundTrack IPluginBase.SoundTrackCreator(JsonElement element)
        {
            throw new NotImplementedException();
        }
    }
}
