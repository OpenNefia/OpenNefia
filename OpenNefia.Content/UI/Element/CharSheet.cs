using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Skills;

namespace OpenNefia.Content.UI.Element
{
    public class CharSheet : UiElement
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public const int SheetWidth = 700;
        public const int SheetHeight = 400;

        private IAssetInstance IeSheet;
        private EntityUid CharaEntity;
        public CharSheet(EntityUid charaEntity)
        {
            CharaEntity = charaEntity;
            IeSheet = Assets.Get(Protos.Asset.IeSheet);
        }


        private void Init()
        {
            if (_entityManager.TryGetComponent<LevelComponent>(CharaEntity, out var level))
            {
                
            }
            if (_entityManager.TryGetComponent<CharaComponent>(CharaEntity, out var chara))
            {
                
            }
            if (_entityManager.TryGetComponent<SkillsComponent>(CharaEntity, out var skills))
            {

            }
        }


        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
        }

        public override void Draw()
        {
            base.Draw();
            GraphicsEx.SetColor(0, 0, 0, 75);
            IeSheet.Draw(X + 4, Y + 4, SheetWidth, SheetHeight);
            GraphicsEx.SetColor(Color.White);
            IeSheet.Draw(X, Y, SheetWidth, SheetHeight);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }
    }
}
