using OpenNefia.Content.Combat;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;
using OpenNefia.Content.DisplayName;

namespace OpenNefia.Content.Damage
{
    public sealed partial class DamageSystem
    {
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;

        private void DisplayAttackMessage(EntityUid target, ref AfterDamageAppliedEvent args)
        {
            if (args.ExtraArgs.NoAttackText || !IsAlive(args.Attacker))
                return;

            var capitalize = true;
            if (args.ExtraArgs.AttackCount > 0)
            {
                _mes.Display(Loc.GetString("Elona.Damage.Furthermore"));
                capitalize = false;
            }

            var attackSkill = args.ExtraArgs.AttackSkill ?? Protos.Skill.MartialArts;

            string? text = null;

            if (args.ExtraArgs.MessageTense == DamageHPMessageTense.Active)
            {
                if (IsAlive(args.ExtraArgs.Weapon))
                {
                    var verb = Loc.GetPrototypeString(attackSkill, "Damage.VerbActive");
                    if (!Loc.TryGetPrototypeString(attackSkill, "Damage.AttacksWith",
                        out text,
                        ("attacker", args.Attacker),
                        ("verb", verb),
                        ("target", target),
                        ("weapon", args.ExtraArgs.Weapon.Value)))
                    {
                        text = Loc.GetString("Elona.Damage.Attacks.Active.Armed",
                            ("attacker", args.Attacker),
                            ("verb", verb),
                            ("target", target),
                            ("weapon", args.ExtraArgs.Weapon.Value));
                    }
                }
                else
                {
                    var damageTextType = CompOrNull<UnarmedDamageTextComponent>(args.Attacker)?.DamageTextType ?? "Elona.Default";
                    var verb = Loc.GetString($"Elona.Damage.UnarmedText.{damageTextType}.VerbActive");
                    text = Loc.GetString("Elona.Damage.Attacks.Active.Unarmed",
                            ("attacker", args.Attacker),
                            ("verb", verb),
                            ("target", target));
                }
            }
            else if (args.ExtraArgs.MessageTense == DamageHPMessageTense.Passive)
            {
                if (IsAlive(args.ExtraArgs.Weapon))
                {
                    // TODO DisplayNAme without article
                    var weaponName = _displayNames.GetBaseName(args.ExtraArgs.Weapon.Value);
                    var verb = Loc.GetPrototypeString(attackSkill, "Damage.VerbPassive");
                    text = Loc.GetString("Elona.Damage.Attacks.Passive.Armed",
                        ("attacker", args.Attacker),
                        ("verb", verb),
                        ("target", target),
                        ("weapon", weaponName));
                }
                else
                {
                    var damageTextType = CompOrNull<UnarmedDamageTextComponent>(args.Attacker)?.DamageTextType ?? "Elona.Default";
                    var verb = Loc.GetString($"Elona.Damage.UnarmedText.{damageTextType}.VerbPassive");
                    text = Loc.GetString("Elona.Damage.Attacks.Passive.Unarmed",
                            ("attacker", args.Attacker),
                            ("verb", verb),
                            ("target", target));
                }
            }

            if (text != null)
                _mes.Display(text, entity: target, noCapitalize: !capitalize);
        }

        private void DisplayElementalDamageMessage(EntityUid target, ref EntityWoundedEvent args)
        {
            //  >>>>>>>> elona122/shade2/chara_func.hsp:1352 #deffunc txtEleDmg int er,int cc,int tc,int ele ...
            string text;
            if (args.DamageType is ElementalDamageType ele)
            {
                text = Loc.GetPrototypeString(ele.ElementID, "Wounded",
                    ("entity", target),
                    ("attacker", args.Attacker));
            }
            else
            {
                text = Loc.GetString("Elona.Damage.Wounded",
                    ("entity", target),
                    ("attacker", args.Attacker));
            }
            _mes.Display(text, entity: target);
            // <<<<<<<< elona122/shade2/chara_func.hsp:1430 	return  ...
        }

        private void DisplayDamageMessagesWounded(EntityUid entity, ref EntityWoundedEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1506 		if dmgType=dmgSub{ ...
            if (args.ExtraArgs.DamageSubLevel > 0)
            {
                DisplayElementalDamageMessage(entity, ref args);
                return;
            }

            if (!TryComp<SkillsComponent>(entity, out var skills))
                return;

            var damageLevel = args.FinalDamage * 6 / skills.MaxHP;

            if (damageLevel > 0)
            {
                if (skills.MaxHP / 2 > skills.HP)
                    damageLevel++;
                if (skills.MaxHP / 4 > skills.HP)
                    damageLevel++;
                if (skills.MaxHP / 10 > skills.HP)
                    damageLevel++;
            }

            if (args.ExtraArgs.MessageTense == DamageHPMessageTense.Active)
            {
                LocaleKey? key = null;
                var color = UiColors.MesWhite;

                if (args.FinalDamage <= 0)
                {
                    key = "Elona.Damage.Levels.Scratch";
                }
                else if (damageLevel == 0)
                {
                    key = "Elona.Damage.Levels.Slightly";
                    color = UiColors.MesYellow;
                }
                else if (damageLevel == 1)
                {
                    key = "Elona.Damage.Levels.Moderately";
                    color = UiColors.MesOrange;
                }
                else if (damageLevel == 2)
                {
                    key = "Elona.Damage.Levels.Severely";
                    color = UiColors.MesPink;
                }
                else if (damageLevel >= 3)
                {
                    key = "Elona.Damage.Levels.Critically";
                    color = UiColors.MesRed;
                }

                if (key != null)
                {
                    _mes.Display(Loc.GetString(key.Value, ("entity", entity), ("attacker", args.Attacker), ("direct", args.ExtraArgs.AttackerIsMessageSubject)), 
                        color, entity: entity, noCapitalize: true);
                }

                goto skipDmgTxt;
            }

            if (_vis.IsInWindowFov(entity))
            {
                LocaleKey? key = null;
                var color = UiColors.MesWhite;

                if (damageLevel == 1)
                {
                    key = "Elona.Damage.Reactions.Screams";
                    color = UiColors.MesOrange;
                }
                else if (damageLevel == 2)
                {
                    key = "Elona.Damage.Reactions.WrithesInPain";
                    color = UiColors.MesPink;
                }
                else if (damageLevel >= 3)
                {
                    key = "Elona.Damage.Reactions.IsSeverelyHurt";
                    color = UiColors.MesRed;
                }
                else if (args.BaseDamage < 0)
                {
                    key = "Elona.Damage.Reactions.IsHealed";
                    color = UiColors.MesBlue;
                }

                if (key != null)
                {
                    _mes.Display(Loc.GetString(key.Value, ("entity", entity), ("attacker", args.Attacker)), color, entity: entity);
                }
            }

        skipDmgTxt:
            _activities.InterruptActivity(entity);
            // <<<<<<<< elona122/shade2/chara_func.hsp:1534 		rowAct_Check tc ...

            // Force message tense to passive beyond here, due to potential for the killed event to be raised later.
            // Possible scenario: Tense is active, these messages are displayed:
            // "The putit slashes the dragon and " + "makes a scratch."
            // Then a handler for EntityWoundedEvent kills the entity, so EntityKilledEvent is raised.
            // EntityKilledEvent event also displays a message, but if tense is still active, then
            // "makes a scratch. " + "kills him." will print.
            // With passive forced here, it instead becomes
            // "makes a scratch. " + "The dragon is killed."
            args.ExtraArgs.MessageTense = DamageHPMessageTense.Passive;
        }

        private void DisplayElementalDeathMessage(EntityUid target, ref EntityKilledEvent args, ElementalDamageType ele)
        {
            string? text = null;

            if (args.ExtraArgs.MessageTense == DamageHPMessageTense.Active)
            {
                if (!Loc.TryGetPrototypeString(ele.ElementID, "Killed.Active", out text,
                    ("target", target),
                    ("attacker", args.Attacker),
                    ("direct", args.ExtraArgs.AttackerIsMessageSubject)))
                {
                    text = Loc.GetString("Elona.Damage.Killed.Active",
                        ("target", target),
                        ("attacker", args.Attacker),
                        ("direct", args.ExtraArgs.AttackerIsMessageSubject));
                }
            }
            else if (args.ExtraArgs.MessageTense == DamageHPMessageTense.Passive)
            {
                if (!Loc.TryGetPrototypeString(ele.ElementID, "Killed.Passive", out text,
                    ("target", target),
                    ("attacker", args.Attacker)))
                {
                    text = Loc.GetString("Elona.Damage.Killed.Passive",
                        ("target", target),
                        ("attacker", args.Attacker));
                }
            }

            var noCapitalize = args.ExtraArgs.MessageTense == DamageHPMessageTense.Active;
            if (text != null)
                _mes.Display(text, UiColors.MesRed, entity: target, noCapitalize: noCapitalize);
        }

        private readonly LocaleKey[] DeathMessageKeys = new LocaleKey[]
        {
            "TransformedIntoMeat",
            "Destroyed",
            "Minced",
            "Killed"
        };

        private void DisplayCombatDeathMessage(EntityUid target, ref EntityKilledEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1603 				p=rnd(4) ...
            var deathType = _rand.Pick(DeathMessageKeys);
            var capitalize = args.ExtraArgs.MessageTense == DamageHPMessageTense.Passive;

            _mes.Display(Loc.GetString($"Elona.DamageType.Combat.{deathType}.{args.ExtraArgs.MessageTense}", ("target", target), ("attacker", args.Attacker), ("direct", args.ExtraArgs.AttackerIsMessageSubject)), UiColors.MesRed, entity: target, noCapitalize: !capitalize);
            // <<<<<<<< shade2/chara_func.hsp:1608 				} ...
        }

        private void DisplayDamageTypeDeathMessage(EntityUid target, ref EntityKilledEvent args)
        {
            if (args.DamageType == null)
                return;

            _mes.Display(args.DamageType.LocalizeDeathMessage(target, args.Attacker, EntityManager));
        }

        private void DisplayDamageMessagesKilled(EntityUid target, ref EntityKilledEvent args)
        {
            if (args.Attacker != null)
            {
                if (args.DamageType is ElementalDamageType ele)
                {
                    DisplayElementalDeathMessage(target, ref args, ele);
                }
                else
                {
                    DisplayCombatDeathMessage(target, ref args);
                }
            }
            else
            {
                DisplayDamageTypeDeathMessage(target, ref args);
            }
        }
    }
}
