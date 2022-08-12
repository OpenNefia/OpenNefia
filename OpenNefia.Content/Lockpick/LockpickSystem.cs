using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Skills;
using OpenNefia.Content.RandomEvent;

namespace OpenNefia.Content.Lockpick
{
    public interface ILockpickSystem : IEntitySystem
    {
        bool TryToLockpick(EntityUid user, int difficulty);
    }

    public sealed class LockpickSystem : EntitySystem, ILockpickSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;

        public override void Initialize()
        {
        }

        public bool TryToLockpick(EntityUid user, int difficulty)
        {
            if (difficulty <= 0)
                return true;

            while (true)
            {
                var lockpick = _inv.EntityQueryInInventory<LockpickComponent>(user).FirstOrDefault();
                if (lockpick == null)
                {
                    _mes.Display(Loc.GetString("Elona.Lockpick.DoNotHaveLockpicks"));
                    return false;
                }

                _mes.Display(Loc.GetString("Elona.Lockpick.UseLockpick"));
                _audio.Play(Protos.Sound.Locked1, user);

                var skeletonKey = _inv.EntityQueryInInventory<SkeletonKeyComponent>(user).FirstOrDefault();

                var power = _skills.Level(user, Protos.Skill.LockPicking);
                if (skeletonKey != null)
                {
                    power = (int)(power * 1.5 + 5);
                    _mes.Display(Loc.GetString("Elona.Lockpick.UseSkeletonKey"));
                }

                var failed = false;
                if (power * 2 < difficulty)
                {
                    _mes.Display(Loc.GetString("Elona.Lockpick.TooDifficult"));
                    failed = true;
                }
                else if (power / 2 >= difficulty)
                {
                    _mes.Display(Loc.GetString("Elona.Lockpick.Easy"));
                }
                else if (_rand.Next(_rand.Next(power * 2) + 1) < difficulty)
                {
                    _mes.Display(Loc.GetString("Elona.Lockpick.Fail"));
                    failed = true;
                }

                if (failed)
                {
                    if (_rand.OneIn(3))
                    {
                        _mes.Display(Loc.GetString("Elona.Lockpick.LockpickBreaks"));
                        _stacks.Use(lockpick.Owner, 1);
                    }
                    if (!_playerQuery.YesOrNo(Loc.GetString("Elona.Lockpick.PromptTryAgain")))
                    {
                        return false;
                    }
                }
                else
                {
                    break;
                }
            }

            _mes.Display(Loc.GetString("Elona.Lockpick.Succeed"));
            _skills.GainSkillExp(user, Protos.Skill.LockPicking, 100);
            return true;
        }
    }
}