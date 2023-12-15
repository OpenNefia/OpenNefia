using OpenNefia.Content.Buffs;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Quests;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Game;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Ranks;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Journal
{
    public sealed class VanillaJournalPagesSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFameSystem _fames = default!;
        [Dependency] private readonly IRankSystem _ranks = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        #region Elona.News

        public void News_Render(JournalPagePrototype proto, P_JournalPageRenderEvent ev)
        {
            var textTitle = Loc.GetPrototypeString(proto, "Title");

            // TODO adventurer news
            ev.OutElonaMarkup = @$"- {textTitle} -

No News";
        }

        #endregion

        #region Elona.Quest

        private string FormatQuestStatus(QuestComponent quest)
        {
            string statusColor;
            string statusText;
            if (quest.State == QuestState.Completed)
            {
                statusColor = "#006464";
                statusText = Loc.GetString("Elona.Journal.Pages.Quest.Status.Complete");
            }
            else
            {
                statusColor = "#646400";
                statusText = Loc.GetString("Elona.Journal.Pages.Quest.Status.Job");
            }
            var status = $"<size=10><style=bold><color={statusColor}>[{statusText}]";

            var client = Loc.GetString("Elona.Journal.Pages.Quest.Client", ("clientName", quest.ClientName));
            var location = Loc.GetString("Elona.Journal.Pages.Quest.Location", ("mapName", quest.ClientOriginatingMapName));
            string remaining;
            if (quest.Deadline == null)
                remaining = _quests.FormatDeadlineText(quest.TimeAllotted);
            else
                remaining = Loc.GetString("Elona.Journal.Pages.Quest.Remaining", ("deadline", _quests.FormatDeadlineText(quest.TimeAllotted)));
            var reward = Loc.GetString("Elona.Journal.Pages.Quest.Reward", ("reward", _quests.LocalizeQuestRewardText(quest.Owner)));

            string detailText;
            if (quest.State == QuestState.Completed)
            {
                detailText = Loc.GetString("Elona.Journal.Pages.Quest.ReportToTheClient");
            }
            else
            {
                detailText = _quests.LocalizeQuestDetailText(quest.Owner, _gameSession.Player);
            }

            var text = new[] { status, client, location, remaining, reward, detailText };
            return string.Join('\n', text);
        }

        private string FormatSidequestStatus()
        {
            // TODO
            return string.Empty;
        }

        public void Quest_Render(JournalPagePrototype proto, P_JournalPageRenderEvent ev)
        {
            var mainQuestInfoText = string.Empty; // TODO sidequest

            var questInfos = _quests.EnumerateAcceptedOrCompletedQuests()
                .Select(FormatQuestStatus)
                .ToArray();
            var questInfoText = string.Join("\n\n", questInfos);

            var sidequestInfoText = string.Empty; // TODO sidequest

            var textTitle = Loc.GetPrototypeString(proto, "Title");
            var textQuestsTitle = Loc.GetString("Elona.Journal.Pages.Quest.MainQuest.Title");

            var textSidequestsTitle = Loc.GetString("Elona.Journal.Pages.CompletedQuests.SubQuest");

            ev.OutElonaMarkup = $@"- {textTitle} -

<color=#006400>{textQuestsTitle}
{mainQuestInfoText}
{questInfoText}

<color=#006400>[{textSidequestsTitle}]
{sidequestInfoText}
";
        }

        #endregion

        #region Elona.QuestItem

        public void QuestItem_Render(JournalPagePrototype proto, P_JournalPageRenderEvent ev)
        {
            // TODO sidequest
            ev.OutElonaMarkup = $@"- {Loc.GetPrototypeString(proto, "Title")} -";
        }

        #endregion

        #region Elona.TitleAndRanking

        private string? FormatRank((PrototypeId<RankPrototype>, Rank) rankPair)
        {
            var (id, rank) = rankPair;
            if (rank.Place >= Rank.MaxRankPlace)
                return null;

            var rankTitle = _ranks.GetRankTitle(id, null);
            if (rankTitle == null)
                return null;

            var rankPlace = rank.Place;
            var rankIncomeText = Loc.GetString("Elona.Journal.Pages.TitleAndRanking.Pay", ("income", _ranks.CalcRankIncome(id)));
            var rankDeadlineText = string.Empty;
            if (rank.TimeUntilDecay != null)
            {
                rankDeadlineText = "\n" + Loc.GetString("Elona.Journal.Pages.TitleAndRanking.Deadline", ("deadline", rank.TimeUntilDecay.Value.TotalDays));
            }

            return $@"{rankTitle} Rank.{rankPlace}
{rankIncomeText}{rankDeadlineText}";
        }

        public void TitleAndRanking_Render(JournalPagePrototype proto, P_JournalPageRenderEvent ev)
        {
            // >>>>>>>> shade2/command.hsp:951 	noteadd " - Title & Ranking - ":noteadd "" ...
            var player = _gameSession.Player;

            var textTitle = Loc.GetPrototypeString(proto, "Title");

            var playerFame = _fames.GetFame(player);
            var fameText = Loc.GetString("Elona.Journal.Pages.TitleAndRanking.Fame", ("fame", playerFame));

            var rankTexts = _ranks.EnumerateRanks()
                .Select(FormatRank)
                .WhereNotNull()
                .ToArray();
            var rankText = string.Join("\n\n", rankTexts);

            // TODO arena
            var exBattleWins = 0;
            var exBattleMaxLevel = 0;
            var arenaText = Loc.GetString("Elona.Journal.Pages.TitleAndRanking.Arena",
                ("exBattleWins", exBattleWins),
                ("exBattleMaxLevel", exBattleMaxLevel));

            ev.OutElonaMarkup = $@"- {textTitle} -

{fameText}

{rankText}

{arenaText}";
            // <<<<<<<< shade2/command.hsp:964 	noteadd lang("EXバトル: 勝利 "+gExBattleWin+"回 最高Lv"+g ..
        }

        #endregion

        #region Elona.IncomeAndExpense

        public void IncomeAndExpense_Render(JournalPagePrototype proto, P_JournalPageRenderEvent ev)
        {
            // TODO buildings, taxes, bills, hired servants

            var player = _gameSession.Player;

            var textTitle = Loc.GetPrototypeString(proto, "Title");
            var textSalaryTitle = Loc.GetString("Elona.Journal.Pages.IncomeAndExpense.Salary.Title");
            var textBillsTitle = Loc.GetString("Elona.Journal.Pages.IncomeAndExpense.Bills.Title");

            var salaryGold = Loc.GetString("Elona.Journal.Pages.IncomeAndExpense.Salary.Sum", ("salaryGold", 0)); // TODO salary

            var laborExpenses = Loc.GetString("Elona.Journal.Pages.IncomeAndExpense.Bills.Labor", ("laborExpenses", 0));
            var buildingExpenses = Loc.GetString("Elona.Journal.Pages.IncomeAndExpense.Bills.Maintenance", ("buildingExpenses", 0));
            var taxExpenses = Loc.GetString("Elona.Journal.Pages.IncomeAndExpense.Bills.Tax", ("taxExpenses", 0));
            var totalExpenses = Loc.GetString("Elona.Journal.Pages.IncomeAndExpense.Bills.Sum", ("totalExpenses", 0));

            var unpaidBillCount = Loc.GetString("Elona.Journal.Pages.IncomeAndExpense.Bills.Unpaid", ("unpaidBillCount", 0)); // TODO bills

            ev.OutElonaMarkup = $@"- {textTitle} -

{textSalaryTitle}
<size=12><color=#000064>{salaryGold}

{textBillsTitle}
<size=12><color=#640000>{laborExpenses}
<size=12><color=#640000>{buildingExpenses}
<size=12><color=#640000>{taxExpenses}
<size=12><color=#640000>{totalExpenses}

{unpaidBillCount}
";
        }

        #endregion

        #region Elona.CompletedQuests

        public void CompletedQuests_Render(JournalPagePrototype proto, P_JournalPageRenderEvent ev)
        {
            var textTitle = Loc.GetPrototypeString(proto, "Title");

            var subQuestInfoText = string.Empty; // TODO subquests
            ev.OutElonaMarkup = $@"- {textTitle} -

{subQuestInfoText}";
        }

        #endregion
    }

    [PrototypeEvent(typeof(JournalPagePrototype))]
    public sealed class P_JournalPageRenderEvent
    {
        /// <summary>
        /// Text of the journal, using the 1.22 markup format.
        /// </summary>
        public string OutElonaMarkup { get; set; } = string.Empty;
    }
}
