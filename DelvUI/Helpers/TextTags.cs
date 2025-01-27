using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Helpers
{
    public static class TextTags
    {
        private static string ReplaceTagWithString(string tag, GameObject? actor, string? name = null)
        {
            var n = actor != null ? actor.Name : name ?? "";

            // exp
            switch (tag)
            {
                case "[exp:current-short]":
                    return ExperienceHelper.Instance.CurrentExp.KiloFormat();

                case "[exp:required-short]":
                    return ExperienceHelper.Instance.RequiredExp.KiloFormat();

                case "[exp:rested-short]":
                    return ExperienceHelper.Instance.RestedExp.KiloFormat();

                case "[exp:current]":
                    return ExperienceHelper.Instance.CurrentExp.ToString("N0", CultureInfo.InvariantCulture);

                case "[exp:required]":
                    return ExperienceHelper.Instance.RequiredExp.ToString("N0", CultureInfo.InvariantCulture);

                case "[exp:rested]":
                    return ExperienceHelper.Instance.RestedExp.ToString("N0", CultureInfo.InvariantCulture);

                case "[exp:percent]":
                    return ExperienceHelper.Instance.PercentExp.ToString("N1", CultureInfo.InvariantCulture);
            }

            // name
            switch (tag)
            {
                case "[name]":
                    return n.ToString().CheckForUpperCase();

                case "[name:first]":
                    return n.FirstName().CheckForUpperCase();

                case "[name:first-initial]":
                    return n.FirstName().CheckForUpperCase().Length == 0 ? "" : n.FirstName().CheckForUpperCase()[..1];

                case "[name:first-npcmedium]":
                    return actor?.ObjectKind == ObjectKind.Player ? n.FirstName().CheckForUpperCase() : n.Truncate(15).CheckForUpperCase();

                case "[name:first-npclong]":
                    return actor?.ObjectKind == ObjectKind.Player ? n.FirstName().CheckForUpperCase() : n.Truncate(20).CheckForUpperCase();

                case "[name:first-npcfull]":
                    return actor?.ObjectKind == ObjectKind.Player ? n.FirstName().CheckForUpperCase() : n.ToString().CheckForUpperCase();

                case "[name:last]":
                    return n.LastName().CheckForUpperCase();

                case "[name:last-initial]":
                    return n.LastName().CheckForUpperCase().Length == 0 ? "" : n.LastName().CheckForUpperCase()[..1];

                case "[name:initials]":
                    return n.Initials().CheckForUpperCase();

                case "[name:abbreviate]":
                    return n.Abbreviate().CheckForUpperCase();

                case "[name:veryshort]":
                    return n.Truncate(5).CheckForUpperCase();

                case "[name:short]":
                    return n.Truncate(10).CheckForUpperCase();

                case "[name:medium]":
                    return n.Truncate(15).CheckForUpperCase();

                case "[name:long]":
                    return n.Truncate(20).CheckForUpperCase();
            }

            if (actor is Character character)
            {
                // misc
                switch (tag)
                {
                    case "[distance]":
                        return (character.YalmDistanceX + 1).ToString();

                    case "[company]":
                        return character.CompanyTag.ToString();

                    case "[level]":
                        return character.Level.ToString();

                    case "[job]":
                        return JobsHelper.JobNames.TryGetValue(character.ClassJob.Id, out var jobName) ? jobName : "";
                }

                // health
                switch (tag)
                {
                    case "[health:current]":
                        return character.CurrentHp.ToString();

                    case "[health:current-short]":
                        return character.CurrentHp.KiloFormat();

                    case "[health:current-percent]":
                        return character.CurrentHp == character.MaxHp ? character.CurrentHp.ToString() : $"{Math.Round(100f / character.MaxHp * character.CurrentHp)}";

                    case "[health:current-percent-short]":
                        return character.CurrentHp == character.MaxHp ? character.CurrentHp.KiloFormat() : $"{Math.Round(100f / character.MaxHp * character.CurrentHp)}";

                    case "[health:current-max]":
                        return $"{character.CurrentHp.ToString()}  |  {character.MaxHp}";

                    case "[health:current-max-short]":
                        return $"{character.CurrentHp.KiloFormat()}  |  {character.MaxHp.KiloFormat()}";

                    case "[health:current-max-percent]":
                        return character.CurrentHp == character.MaxHp ? $"{Math.Round(100f / character.MaxHp * character.CurrentHp)} - 100" : $"{character.CurrentHp} - {character.MaxHp}";

                    case "[health:current-max-percent-short]":
                        return character.CurrentHp == character.MaxHp
                            ? $"{Math.Round(100f / character.MaxHp * character.CurrentHp)} - 100"
                            : $"{character.CurrentHp.KiloFormat()}  |  {character.MaxHp.KiloFormat()}";

                    case "[health:max]":
                        return character.MaxHp.ToString();

                    case "[health:max-short]":
                        return character.MaxHp.KiloFormat();

                    case "[health:percent]":
                        return $"{Math.Round(100f / character.MaxHp * character.CurrentHp)}";

                    case "[health:percent-decimal]":
                        return FormattableString.Invariant($"{100f / character.MaxHp * character.CurrentHp:##0.#}");

                    case "[health:deficit]":
                        return $"-{character.MaxHp - character.CurrentHp}";

                    case "[health:deficit-short]":
                        return $"-{(character.MaxHp - character.CurrentHp).KiloFormat()}";
                }

                // mana
                uint currentMp = JobsHelper.CurrentPrimaryResource(character);
                uint maxMp = JobsHelper.MaxPrimaryResource(character);

                switch (tag)
                {

                    case "[mana:current]":
                        return currentMp.ToString();

                    case "[mana:current-short]":
                        return currentMp.KiloFormat();

                    case "[mana:current-percent]":
                        return currentMp == maxMp ? currentMp.ToString() : $"{Math.Round(100f / maxMp * currentMp)}";

                    case "[mana:current-percent-short]":
                        return currentMp == maxMp ? currentMp.KiloFormat() : $"{Math.Round(100f / maxMp * currentMp)}";

                    case "[mana:current-max]":
                        return $"{currentMp}  |  {maxMp}";

                    case "[mana:current-max-short]":
                        return $"{currentMp.KiloFormat()} | {maxMp.KiloFormat()}";

                    case "[mana:current-max-percent]":
                        return currentMp == maxMp ? $"{Math.Round(100f / maxMp * currentMp)}  |  100" : $"{currentMp} - {maxMp}";

                    case "[mana:current-max-percent-short]":
                        return currentMp == maxMp
                            ? $"{Math.Round(100f / maxMp * currentMp)}  |  100"
                            : $"{currentMp.KiloFormat()} - {maxMp.KiloFormat()}";

                    case "[mana:max]":
                        return maxMp.ToString();

                    case "[mana:max-short]":
                        return maxMp.KiloFormat();

                    case "[mana:percent]":
                        return $"{Math.Round(100f / maxMp * currentMp)}";

                    case "[mana:percent-decimal]":
                        return FormattableString.Invariant($"{100f / maxMp * currentMp:##0.#}");

                    case "[mana:deficit]":
                        return $"-{maxMp - currentMp}";

                    case "[mana:deficit-short]":
                        return $"-{(maxMp - currentMp).KiloFormat()}";
                }
            }

            return "";
        }

        public static string GenerateFormattedTextFromTags(GameObject? actor, string text, string? name = null)
        {
            MatchCollection matches = Regex.Matches(text, @"\[(.*?)\]");
            return matches.Aggregate(text, (current, m) => current.Replace(m.Value, ReplaceTagWithString(m.Value, actor, name)));
        }
    }
}
