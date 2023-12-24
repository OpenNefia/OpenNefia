using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Input;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.CharaMake.CharaMakeAttributeRerollLayer;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.AliasSelect")]
    public class CharaMakeAliasLayer : CharaMakeLayer<CharaMakeAliasLayer.ResultData>
    {
        [Child][Localize] private PickRandomAliasPrompt _prompt = default!;

        public CharaMakeAliasLayer()
        {
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.AddRange(_prompt.MakeKeyHints());

            return keyHints;
        }

        public override void Initialize(CharaMakeResultSet args)
        {
            base.Initialize(args);
            if (_prompt != null)
                RemoveChild(_prompt);

            var promptArgs = new PickRandomAliasPrompt.Args
            {
                AliasType = AliasType.Chara
            };
            _prompt = UserInterfaceManager.CreateAndInitializeLayer<PickRandomAliasPrompt, PickRandomAliasPrompt.Args, PickRandomAliasPrompt.Result>(promptArgs);
            AddChild(_prompt);
        }

        public sealed class ResultData : CharaMakeResult
        {
            public string Alias { get; set; }

            public ResultData(string alias)
            {
                Alias = alias;
            }

            public override void ApplyStep(EntityUid entity, EntityGenArgSet args)
            {
                EntityManager.EnsureComponent<AliasComponent>(entity).Alias = Alias;
            }
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            _prompt.GrabFocus();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            _prompt.OnQuery();
        }

        public override void OnQueryFinish()
        {
            base.OnQueryFinish();
            _prompt.OnQueryFinish();
        }

        public override UiResult<CharaMakeUIResult>? GetResult()
        {
            var result = _prompt.GetResult();
            if (result == null)
                return null;

            switch (result)
            {
                case UiResult<PickRandomAliasPrompt.Result>.Finished finished:
                    var charaMakeResult = new CharaMakeUIResult(new ResultData(finished.Value.Alias));
                    return new UiResult<CharaMakeUIResult>.Finished(charaMakeResult);
                case UiResult<PickRandomAliasPrompt.Result>.Cancelled:
                    return new UiResult<CharaMakeUIResult>.Finished(new CharaMakeUIResult(null, CharaMakeStep.GoBack));
                case UiResult<PickRandomAliasPrompt.Result>.Error error:
                    return new UiResult<CharaMakeUIResult>.Error(error.Exception);
            }

            return null;
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            _prompt.GetPreferredBounds(out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _prompt.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _prompt.SetPosition(X, Y);
        }

        public override void Draw()
        {
            base.Draw();
            _prompt.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            _prompt.Update(dt);
            _prompt.AssetBG = CurrentWindowBG;
        }
    }
}
