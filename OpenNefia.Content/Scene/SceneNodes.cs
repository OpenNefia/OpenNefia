using OpenNefia.Content.Audio;
using OpenNefia.Content.Portraits;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Scene
{

    [ImplicitDataDefinitionForInheritors]
    public interface ISceneNode
    {
        void OnEnter(ISceneEngine engine);
    }

    [DataDefinition]
    public sealed class SceneFile
    {
        [DataField]
        public List<ISceneNode> Nodes { get; } = new();
    }

    [DataDefinition]
    public class SceneActorSpec
    {
        [DataField]
        public LocaleKey Name { get; set; } = "Elona.Scene.Common.ActorName.Unknown";

        [DataField]
        public PrototypeId<PortraitPrototype>? PortraitID { get; set; }
    }

    public sealed class SceneSetActorsNode : ISceneNode
    {
        [DataField(required: true)]
        public Dictionary<string, SceneActorSpec> Actors { get; set; } = new();

        public void OnEnter(ISceneEngine engine)
        {
            engine.SetActors(Actors);
        }
    }

    public sealed class SceneChangeBackgroundNode : ISceneNode
    {
        [DataField(required: true)]
        public PrototypeId<AssetPrototype> ID { get; set; } = Protos.Asset.Bg1;

        public void OnEnter(ISceneEngine engine)
        {
            engine.SetBackground(ID);
        }
    }

    public sealed class ScenePlaySoundNode : ISceneNode
    {
        [DataField(required: true)]
        public SoundSpecifier ID { get; set; } = default!;

        public void OnEnter(ISceneEngine engine)
        {
            var id = ID.GetSound();
            if (id == null)
                return;
            IoCManager.Resolve<IAudioManager>().Play(id.Value);
        }
    }

    public sealed class ScenePlayMusicNode : ISceneNode
    {
        [DataField(required: true)]
        public PrototypeId<MusicPrototype>? ID { get; set; }

        public void OnEnter(ISceneEngine engine)
        {
            var music = IoCManager.Resolve<IMusicManager>();
            if (ID != null)
                music.Play(ID.Value);
            else
                music.Stop();
        }
    }

    public sealed class SceneTextNode : ISceneNode
    {
        [DataField(required: true)]
        public List<string> Text { get; set; } = new();

        public void OnEnter(ISceneEngine engine)
        {
            engine.ShowText(Text);
        }
    }

    [DataDefinition]
    public sealed class SceneDialogText
    {
        [DataField(required: true)]
        public string Actor { get; set; } = "";

        [DataField(required: true)]
        public string Text { get; set; } = "";
    }

    public sealed class SceneDialogNode : ISceneNode
    {
        [DataField(required: true)]
        public List<SceneDialogText> Dialog { get; set; } = new();

        public void OnEnter(ISceneEngine engine)
        {
            engine.ShowDialog(Dialog);
        }
    }

    public sealed class SceneWaitNode : ISceneNode
    {
        public void OnEnter(ISceneEngine engine)
        {
            engine.Wait();
        }
    }

    public sealed class SceneFadeOutNode : ISceneNode
    {
        public void OnEnter(ISceneEngine engine)
        {
            engine.FadeOut();
        }
    }

    public sealed class SceneFadeInNode : ISceneNode
    {
        public void OnEnter(ISceneEngine engine)
        {
            engine.FadeIn();
        }
    }
}
