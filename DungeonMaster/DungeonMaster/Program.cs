using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace DungeonMaster {
    class Program {

        private static readonly Logger COMPACT_LOGGER = LogManager.GetLogger("COMPACT");
        private static readonly Logger RAW_LOGGER = LogManager.GetLogger("RAW");

        private class DungeonRunner {

            private static readonly string patternWeapon = "Level (\\d+) \\w+(?: \\+(\\d+))?";
            private static readonly string patternStats = "Dungeon Level (\\d+).*?weapon.*?<td>(\\d+).*?<td>(\\d+).*?<td>(\\d+).*?<td>" + patternWeapon + "<\\/td>";
            private static readonly string patternPotion = "(\\w+) potion(?: \\+(\\d+))?";
            private static readonly string patternInventory = "Inventory(?:.*?" + patternPotion + ")?(?:.*?" + patternPotion + ")?(?:.*?" + patternPotion + ")?.*?<\\/td>";
            private static readonly string patternDirection = "(North|South|East|West|Down)";
            private static readonly string patternMove = "(?:Move:(?:.*?" + patternDirection + ")?(?:.*?" + patternDirection + ")?(?:.*?" + patternDirection + ")?(?:.*?" + patternDirection + ")?(?:.*?" + patternDirection + ")?)";
            private static readonly string patternAttack = "(?:(attack))";
            private static readonly string patternLoot = "(?:Pick up (treasure).*?(?:(?:" + patternPotion + ")|(?:" + patternWeapon + ")).*?" + patternMove + ")";
            private static readonly string patternAll = "<html>.*?" + patternStats + ".*?" + patternInventory + ".*?(?:" + patternMove + "|" + patternAttack + "|" + patternLoot + ").*?<\\/html>";
            private static readonly Regex regexParseRoom = new Regex(patternAll, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

            private static readonly string dungeonUrl = "http://www.hacker.org/challenge/misc/d/cave.php?name=yes-man&spw=ead0714db80f8e1ccaf568acd9847044";
            // starting positions in reading order
            private static readonly string[] defaultPaths = new string[] { "eesswwne", "esswwnnse", "sswwnnes", "neesswwen", "nesswwnn", "swwnneews", "nneesswn", "wnneessnw", "wwnneesw" };

            private enum PotionType {
                NONE, SHIELD, WEAPON_BUFF, HEALING, DAMAGE, UNKNOWN
            }

            private class Potion {
                public PotionType type;
                public int level;
                public int invIndex;
                public Potion() {
                    this.type = PotionType.NONE;
                    this.level = -1;
                    this.invIndex = -1;
                }
                public Potion(PotionType type, int level, int index) {
                    this.type = type;
                    this.level = level;
                    this.invIndex = index;
                }
            }

            private class Weapon {
                public int level;
                public int bonus;
                public Weapon() {
                    this.level = -1;
                    this.bonus = -1;
                }
                public Weapon(int level, int bonus) {
                    this.level = level;
                    this.bonus = bonus;
                }
                public void Update(int level, int bonus) {
                    this.level = level;
                    this.bonus = bonus;
                }
            }

            private enum DirectionType {
                NORTH, SOUTH, EAST, WEST, DOWN, UNKNOWN
            }

            private enum EventType {
                REGEX_FAIL, DEATH, NORMAL
            }

            // game status
            private int dungeonLevel = -1;
            private int stairPositionX = -1;
            private int stairPositionY = -1;
            private int playerPosX = -1;
            private int playerPosY = -1;
            private int playerLevel = -1;
            private int playerHitPoints = -1;
            private int playerMaxHp = -1;
            private int playerExperience = -1;
            private Weapon playerWeapon = new Weapon();
            private readonly Potion[] playerPotions = new Potion[3] { new Potion(), new Potion(), new Potion() };
            private readonly HashSet<DirectionType> directions = new HashSet<DirectionType>();

            private int fightRound = 0;
            private bool bossFight = false;

            private bool looting = false;
            private Potion lootPotion = new Potion();
            private Weapon lootWeapon = new Weapon();

            private string plannedPath = "";
            private bool levelCompleted = false;

            // stats
            private int pagesLoaded = 0;
            private int playerAttacksHit = 0;
            private int playerAttacksMissed = 0;
            private int monsterAttacksHit = 0;
            private int monsterAttacksMissed = 0;
            private int playerMonstersKilled = 0;
            private List<Potion> playerPotionsFound = new List<Potion>();
            private int playerPotionsLooted = 0;
            private List<Weapon> playerWeaponsFound = new List<Weapon>();
            private int playerWeaponsLooted = 0;

            // still alive?
            private bool running = false;

            WebClient webClient;

            private void DetermineCurrentPosX() {
                // west available, so not first (left) row
                if (this.directions.Contains(DirectionType.WEST)) {
                    // east also available, so we're in the middle
                    if (this.directions.Contains(DirectionType.EAST)) {
                        this.playerPosX = 1;
                    }
                    // east, got it
                    else {
                        this.playerPosX = 2;
                    }
                }
                // first (ie left) row
                else {
                    this.playerPosX = 0;
                }
            }

            private void DetermineCurrentPosY() {
                // north available, so not top row
                if (this.directions.Contains(DirectionType.NORTH)) {
                    // south also available, so we're in the middle
                    if (this.directions.Contains(DirectionType.SOUTH)) {
                        this.playerPosY = 1;
                    }
                    // south, got it
                    else {
                        this.playerPosY = 2;
                    }
                }
                // top row
                else {
                    this.playerPosY = 0;
                }
            }

            private void DetermineCurrentPosition() {
                DetermineCurrentPosX();
                DetermineCurrentPosY();
                COMPACT_LOGGER.Info("Position determined to be: " + this.playerPosX + ":" + this.playerPosY);
            }

            private string PlanPathToStairs() {
                string path = "";
                // stairs to the east
                if (this.playerPosX < this.stairPositionX) {
                    path += (this.stairPositionX - this.playerPosX == 2) ? "ee" : "e";
                }
                else {
                    // stairs to the west
                    if (this.playerPosX > this.stairPositionX) {
                        path += (this.playerPosX - this.stairPositionX == 2) ? "ww" : "w";
                    }
                    // or the stairs are in the same column; no movement needed
                }
                // stairs to the south
                if (this.playerPosY < this.stairPositionY) {
                    path += (this.stairPositionY - this.playerPosY == 2) ? "ss" : "s";
                }
                else {
                    // stairs to the north
                    if (this.playerPosY > this.stairPositionY) {
                        path += (this.playerPosY - this.stairPositionY == 2) ? "nn" : "n";
                    }
                    // or the stairs are in the same column; no movement needed
                }
                return path;
            }

            private Potion ParsePotion(string typeString, string bonusString, int invIndex) {
                PotionType potionType;
                switch (typeString) {
                    case "Magenta":
                        potionType = PotionType.WEAPON_BUFF;
                        break;
                    case "Aquamarine":
                        potionType = PotionType.HEALING;
                        break;
                    case "Lavender":
                        potionType = PotionType.SHIELD;
                        break;
                    case "Turquoise":
                        potionType = PotionType.DAMAGE;
                        break;
                    default:
                        potionType = PotionType.UNKNOWN;
                        break;
                }
                return new Potion(potionType, string.IsNullOrEmpty(bonusString) ? 0 : int.Parse(bonusString), invIndex);
            }

            private Weapon ParseWeapon(string levelString, string bonusString) {
                return new Weapon(int.Parse(levelString), string.IsNullOrEmpty(bonusString) ? 0 : int.Parse(bonusString));
            }

            private DirectionType ParseDirection(string directionString) {
                switch (directionString) {
                    case "North":
                        return DirectionType.NORTH;
                    case "South":
                        return DirectionType.SOUTH;
                    case "East":
                        return DirectionType.EAST;
                    case "West":
                        return DirectionType.WEST;
                    case "Down":
                        return DirectionType.DOWN;
                    default:
                        COMPACT_LOGGER.Error("Unknown direction: " + directionString);
                        return DirectionType.UNKNOWN;
                }
            }

            private void ResetAfterDeath() {
                this.plannedPath = string.Empty;
                this.levelCompleted = false;
                this.pagesLoaded = 0;
                this.playerAttacksHit = 0;
                this.playerAttacksMissed = 0;
                this.monsterAttacksHit = 0;
                this.monsterAttacksMissed = 0;
                this.playerMonstersKilled = 0;
                this.playerPotionsFound.Clear();
                this.playerPotionsLooted = 0;
                this.playerWeaponsFound.Clear();
                this.playerWeaponsLooted = 0;
            }

            private void PrintStatistics() {
                COMPACT_LOGGER.Info("Here are a few stats about the run:");
                COMPACT_LOGGER.Info("pages loaded: " + this.pagesLoaded);
                COMPACT_LOGGER.Info("player attacks: " + this.playerAttacksHit + "/" + (this.playerAttacksHit + this.playerAttacksMissed));
                COMPACT_LOGGER.Info("monster attacks: " + this.monsterAttacksHit + "/" + (this.monsterAttacksHit + this.monsterAttacksMissed));
                COMPACT_LOGGER.Info("monsters killed: " + this.playerMonstersKilled);
                COMPACT_LOGGER.Info("potions found: " + this.playerPotionsFound.Count + " (Heal:" + this.playerPotionsFound.Count(x => x.type == PotionType.HEALING)
                    + " Damage:" + this.playerPotionsFound.Count(x => x.type == PotionType.DAMAGE) + " Buff:" + this.playerPotionsFound.Count(x => x.type == PotionType.WEAPON_BUFF)
                    + " Shield:" + this.playerPotionsFound.Count(x => x.type == PotionType.SHIELD) + ") of which " + this.playerPotionsLooted + " were looted");
                COMPACT_LOGGER.Info("weapons found: " + this.playerWeaponsFound.Count + " of which " + this.playerWeaponsLooted + " were looted");
            }

            private EventType ParseGameStatus(string webResponse) {
                RAW_LOGGER.Info(webResponse);

                if (Regex.IsMatch(webResponse, "you have died", RegexOptions.IgnoreCase)) {
                    COMPACT_LOGGER.Warn("********************");
                    COMPACT_LOGGER.Warn("* You have died :/ *");
                    COMPACT_LOGGER.Warn("********************");
                    this.webClient.DownloadStringAsync(new Uri(dungeonUrl));
                    return EventType.DEATH;
                }

                Match match = regexParseRoom.Match(webResponse);
                if (!match.Success) {
                    COMPACT_LOGGER.Error("WARNING! Could not parse server response. Regex broken :/ Output was:" + Environment.NewLine + webResponse);
                    return EventType.REGEX_FAIL;
                }

                // dungeon level : 1
                this.dungeonLevel = int.Parse(match.Groups[1].Value);
                // player level : 2
                this.playerLevel = int.Parse(match.Groups[2].Value);
                // player hitpoints : 3
                this.playerHitPoints = int.Parse(match.Groups[3].Value);
                this.playerMaxHp = Math.Max(this.playerHitPoints, this.playerMaxHp);
                // player xp : 4
                this.playerExperience = int.Parse(match.Groups[4].Value);
                // weapon : 5 + 6
                this.playerWeapon = ParseWeapon(match.Groups[5].Value, match.Groups[6].Value);
                // inventory (potions) : 7 - 12
                for (int potionIndex = 0; potionIndex < 3; potionIndex++) {
                    if (string.IsNullOrEmpty(match.Groups[7 + (2 * potionIndex)].Value)) {
                        this.playerPotions[potionIndex] = new Potion(PotionType.NONE, 0, potionIndex);
                    }
                    else {
                        this.playerPotions[potionIndex] = ParsePotion(match.Groups[7 + (2 * potionIndex)].Value, match.Groups[8 + (2 * potionIndex)].Value, potionIndex);
                    }
                }
                // movement (empty room) : 13 - 17
                for (int directionIndex = 0; directionIndex < 5; directionIndex++) {
                    string directionString = match.Groups[13 + directionIndex].Value;
                    // no further directions found
                    if (string.IsNullOrEmpty(directionString)) {
                        break;
                    }
                    // first direction found
                    if (directionIndex == 0) {
                        this.directions.Clear();
                    }
                    this.directions.Add(ParseDirection(directionString));
                }
                // "attack" (monster in room) : 18
                if (!string.IsNullOrEmpty(match.Groups[18].Value)) {
                    this.fightRound++;
                    if (!webResponse.Contains("big monster", StringComparison.OrdinalIgnoreCase)) {
                        COMPACT_LOGGER.Warn("***************");
                        COMPACT_LOGGER.Info("* BOSS FIGTH! *");
                        COMPACT_LOGGER.Warn("***************");
                        this.bossFight = true;
                    }
                }
                else {
                    this.fightRound = 0;
                }
                // "treasure" (monster defeated) : 19
                if (!string.IsNullOrEmpty(match.Groups[19].Value)) {
                    this.looting = true;
                    // potion to loot (type + bonus) : 20 + 21
                    if (!string.IsNullOrEmpty(match.Groups[20].Value)) {
                        this.lootPotion = ParsePotion(match.Groups[20].Value, match.Groups[21].Value, -1);
                        this.playerPotionsFound.Add(this.lootPotion);
                    }
                    // weapon to loot (level + bonus) : 22 + 23
                    if (!string.IsNullOrEmpty(match.Groups[22].Value)) {
                        this.lootWeapon = ParseWeapon(match.Groups[22].Value, match.Groups[23].Value);
                        this.playerWeaponsFound.Add(this.lootWeapon);
                    }
                }
                // movement after loot : 24 - 28
                for (int directionIndex = 0; directionIndex < 5; directionIndex++) {
                    string directionString = match.Groups[24 + directionIndex].Value;
                    // no further directions found
                    if (string.IsNullOrEmpty(directionString)) {
                        break;
                    }
                    // first direction found
                    if (directionIndex == 0) {
                        this.directions.Clear();
                    }
                    this.directions.Add(ParseDirection(directionString));
                }

                // log
                COMPACT_LOGGER.Info("Dungeon:" + this.dungeonLevel + " Fight:" + this.fightRound + " Stairs:" + this.stairPositionX + ":" + this.stairPositionY + " Position:" + this.playerPosX + ":" + this.playerPosY);
                COMPACT_LOGGER.Info("Player:" + this.playerLevel + " Hitpoints:" + this.playerHitPoints + "/" + this.playerMaxHp + " XP:" + this.playerExperience + " Weapon:" + this.playerWeapon.level + "+" + this.playerWeapon.bonus);
                COMPACT_LOGGER.Info("Inventory:" + this.playerPotions[0].type + "+" + this.playerPotions[0].level + "," + this.playerPotions[1].type + "+" + this.playerPotions[1].level + "," + this.playerPotions[2].type + "+" + this.playerPotions[2].level);

                // stats
                if (webResponse.Contains("you killed the monster", StringComparison.OrdinalIgnoreCase)) {
                    this.playerMonstersKilled++;
                }
                if (webResponse.Contains("you hit the monster", StringComparison.OrdinalIgnoreCase)) {
                    this.playerAttacksHit++;
                }
                if (webResponse.Contains("you miss", StringComparison.OrdinalIgnoreCase)) {
                    this.playerAttacksMissed++;
                }
                if (webResponse.Contains("the monster hits you", StringComparison.OrdinalIgnoreCase)) {
                    this.monsterAttacksHit++;
                }
                if (webResponse.Contains("the monster misses", StringComparison.OrdinalIgnoreCase)) {
                    this.monsterAttacksMissed++;
                }

                return EventType.NORMAL;
            }

            private void ProcessFeedbackStringFromWebClient(object sender, DownloadStringCompletedEventArgs e) {
                COMPACT_LOGGER.Debug(".. Received feedback ..");
                this.pagesLoaded++;

                if (!this.running) {
                    COMPACT_LOGGER.Warn("Received feedback after shutdown.");
                    return;
                }

                if (e.Error != null) {
                    COMPACT_LOGGER.Error(e.Error, "Error during web request.");
                    return;
                }

                switch (ParseGameStatus(e.Result)) {
                    case EventType.NORMAL:
                        break;
                    case EventType.DEATH:
                        COMPACT_LOGGER.Info("Restarting...");
                        PrintStatistics();
                        ResetAfterDeath();
                        return;
                    case EventType.REGEX_FAIL:
                        COMPACT_LOGGER.Error("Aborting run.");
                        PrintStatistics();
                        return;
                    default:
                        COMPACT_LOGGER.Error("Unknown game state. Aborting.");
                        return;
                }

                DetermineCurrentPosition();

                // currently standing next to the stairs
                if (this.directions.Contains(DirectionType.DOWN)) {
                    this.stairPositionX = this.playerPosX;
                    this.stairPositionY = this.playerPosY;
                    COMPACT_LOGGER.Info("Found stairs at: " + this.stairPositionX + ":" + this.stairPositionY);
                }

                // need to fight?
                if (this.fightRound > 0) {
                    // use best BUFF potion for boss fight
                    if ((this.fightRound == 1) && this.bossFight && this.playerPotions.Any(x => x.type == PotionType.WEAPON_BUFF)) {
                        Potion buffPotion = this.playerPotions.Where(x => x.type == PotionType.WEAPON_BUFF).OrderBy(x => x.level).Last(x => x.type == PotionType.WEAPON_BUFF);
                        this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&potion=" + buffPotion.invIndex));
                        COMPACT_LOGGER.Info("Using BUFF potion (in boss fight). Level:" + buffPotion.level + " InvIndex:" + buffPotion.invIndex);
                        return;
                    }
                    // got more than one BUFF potion and the fight is starting? (last one is for the boss fight)
                    if ((this.fightRound == 1) && !this.bossFight && this.playerPotions.Count(x => x.type == PotionType.WEAPON_BUFF) > 1) {
                        Potion buffPotion = this.playerPotions.Where(x => x.type == PotionType.WEAPON_BUFF).OrderBy(x => x.level).First(x => x.type == PotionType.WEAPON_BUFF);
                        this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&potion=" + buffPotion.invIndex));
                        COMPACT_LOGGER.Info("Using BUFF potion (in normal fight). Level:" + buffPotion.level + " InvIndex:" + buffPotion.invIndex);
                        return;
                    }
                    // only got a shitty BUFF potion?
                    if ((this.fightRound == 1) && !this.bossFight && this.playerPotions.Count(x => (x.type == PotionType.WEAPON_BUFF) && (x.level < 4)) > 0) {
                        Potion buffPotion = this.playerPotions.Where(x => (x.type == PotionType.WEAPON_BUFF) && (x.level < 4)).OrderBy(x => x.level).First(x => x.type == PotionType.WEAPON_BUFF);
                        this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&potion=" + buffPotion.invIndex));
                        COMPACT_LOGGER.Info("Using shitty BUFF potion (in normal fight). Level:" + buffPotion.level + " InvIndex:" + buffPotion.invIndex);
                        return;
                    }
                    // got a SHIELD potion and the fight is starting?
                    if ((this.fightRound == 1) && this.playerPotions.Any(x => x.type == PotionType.SHIELD)) {
                        Potion shieldPotion = this.playerPotions.Where(x => x.type == PotionType.SHIELD).OrderBy(x => x.level).First(x => x.type == PotionType.SHIELD);
                        this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&potion=" + shieldPotion.invIndex));
                        COMPACT_LOGGER.Info("Using SHIELD potion. Level:" + shieldPotion.level + " InvIndex:" + shieldPotion.invIndex);
                        return;
                    }
                    // got a DAMAGE potion?
                    if (this.playerPotions.Any(x => x.type == PotionType.DAMAGE)) {
                        Potion damagePotion = this.playerPotions.Where(x => x.type == PotionType.DAMAGE).OrderBy(x => x.level).First(x => x.type == PotionType.DAMAGE);
                        this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&potion=" + damagePotion.invIndex));
                        COMPACT_LOGGER.Info("Using DMG potion. Level:" + damagePotion.level + " InvIndex:" + damagePotion.invIndex);
                        return;
                    }
                    // low HP in boss fight
                    if (((float)this.playerHitPoints / this.playerMaxHp < 0.3f) && this.bossFight && this.playerPotions.Any(x => x.type == PotionType.HEALING)) {
                        Potion healPotion = this.playerPotions.Where(x => x.type == PotionType.HEALING).OrderBy(x => x.level).Last(x => x.type == PotionType.HEALING);
                        this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&potion=" + healPotion.invIndex));
                        COMPACT_LOGGER.Info("Using HEAL potion (in boss fight). Level:" + healPotion.level + " InvIndex:" + healPotion.invIndex);
                        return;
                    }
                    // low HP, HEAL potion available & not close to boss
                    if (((float)this.playerHitPoints / this.playerMaxHp < 0.3f) && !this.bossFight && (this.dungeonLevel < 23) && this.playerPotions.Any(x => x.type == PotionType.HEALING)) {
                        Potion healPotion = this.playerPotions.Where(x => x.type == PotionType.HEALING).OrderBy(x => x.level).Last(x => x.type == PotionType.HEALING);
                        this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&potion=" + healPotion.invIndex));
                        COMPACT_LOGGER.Info("Using HEAL potion (in upper levels). Level:" + healPotion.level + " InvIndex:" + healPotion.invIndex);
                        return;
                    }
                    // low HP and many potions in normal fight
                    if (((float)this.playerHitPoints / this.playerMaxHp < 0.3f) && !this.bossFight && this.playerPotions.Count(x => x.type == PotionType.HEALING) > 1) {
                        Potion healPotion = this.playerPotions.Where(x => x.type == PotionType.HEALING).OrderBy(x => x.level).First(x => x.type == PotionType.HEALING);
                        this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&potion=" + healPotion.invIndex));
                        COMPACT_LOGGER.Info("Using HEAL potion (because we got 1+). Level:" + healPotion.level + " InvIndex:" + healPotion.invIndex);
                        return;
                    }
                    this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&attack=1"));
                    COMPACT_LOGGER.Info("Attacking!");
                    return;
                }

                // loot?!
                if (this.looting) {
                    if (this.lootPotion.level > -1) {
                        if (this.playerPotions.Any(x => x.type == PotionType.NONE)) {
                            this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&tres=1"));
                            COMPACT_LOGGER.Info("Looting potion. Type:" + lootPotion.type + " Level:" + lootPotion.level);
                            this.playerPotionsLooted++;
                            this.lootPotion = new Potion();
                            return;
                        }
                        else {
                            COMPACT_LOGGER.Info("Ignoring potion. Type:" + lootPotion.type + " Level:" + lootPotion.level);
                            this.lootPotion = new Potion();
                        }
                    }
                    if (this.lootWeapon.level > -1) {
                        if ((this.playerWeapon.level + this.playerWeapon.bonus < this.lootWeapon.level + this.lootWeapon.bonus)) {
                            this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&tres=1"));
                            COMPACT_LOGGER.Info("Looting weapon. OldLevel:" + this.playerWeapon.level + " OldBonus:" + this.playerWeapon.bonus + " NewLevel:" + this.lootWeapon.level + " NewBonus:" + this.lootWeapon.bonus);
                            this.playerWeaponsLooted++;
                            this.lootWeapon = new Weapon();
                            return;
                        }
                        else {
                            COMPACT_LOGGER.Info("Ignoring weapon. OldLevel:" + this.playerWeapon.level + " OldBonus:" + this.playerWeapon.bonus + " NewLevel:" + this.lootWeapon.level + " NewBonus:" + this.lootWeapon.bonus);
                            this.lootWeapon = new Weapon();
                        }
                    }
                }

                // no plan & completed
                if ((this.plannedPath == string.Empty) && this.levelCompleted) {
                    this.plannedPath = PlanPathToStairs();
                    COMPACT_LOGGER.Info("Done with this level. Heading for stairs: " + this.plannedPath);
                }
                // no plan .. not complete
                if ((this.plannedPath == string.Empty) && !this.levelCompleted) {
                    this.plannedPath = defaultPaths[this.playerPosX + (3 * this.playerPosY)];
                    COMPACT_LOGGER.Info("New to this level. Making plan: " + this.plannedPath);
                }

                string nextStep = string.Empty;
                // still the plan is empty and the level is done?
                if ((this.plannedPath == string.Empty) && this.levelCompleted) {
                    // need more levels? take random step away from stairs
                    bool upperLevels = (this.dungeonLevel <= 17) && (this.playerLevel <= this.dungeonLevel + 2);
                    bool middleLevels = (this.dungeonLevel <= 22) && (this.playerLevel <= this.dungeonLevel + 1);
                    bool lowerLevels = (this.dungeonLevel <= 24) && (this.playerLevel <= this.dungeonLevel);
                    bool stillNeedLevels = upperLevels || middleLevels || lowerLevels;
                    if (stillNeedLevels) {
                        switch (this.directions.First(x => x != DirectionType.DOWN)) {
                            case DirectionType.EAST:
                                nextStep = "e";
                                break;
                            case DirectionType.NORTH:
                                nextStep = "n";
                                break;
                            case DirectionType.SOUTH:
                                nextStep = "s";
                                break;
                            case DirectionType.WEST:
                                nextStep = "w";
                                break;
                        }
                    }
                    else {
                        nextStep = "d";
                        this.levelCompleted = false;
                        this.stairPositionX = -1;
                        this.stairPositionY = -1;
                        COMPACT_LOGGER.Info("Been there, done that. Going downstairs.");
                    }
                }
                // follow the path
                else {
                    nextStep = this.plannedPath.Substring(0, 1);
                    this.plannedPath = this.plannedPath.Substring(1, this.plannedPath.Length - 1);
                    if (this.plannedPath == string.Empty) {
                        this.levelCompleted = true;
                    }
                    COMPACT_LOGGER.Info("Taking another step to '" + nextStep + "'. That leaves '" + this.plannedPath + "' as future path." + (this.levelCompleted ? " This level is therefore done." : ""));
                }
                this.webClient.DownloadStringAsync(new Uri(dungeonUrl + "&m=" + nextStep));
            }

            public void Run() {
                COMPACT_LOGGER.Info("Runner started.");
                COMPACT_LOGGER.Info(patternAll);
                this.running = true;
                using (this.webClient = new WebClient()) {
                    this.webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ProcessFeedbackStringFromWebClient);
                    this.webClient.DownloadStringAsync(new Uri(dungeonUrl));
                }
            }

            public void Stop() {
                this.running = false;
            }

        }

        static void Main(string[] args) {
            COMPACT_LOGGER.Info("Starting...");
            DungeonRunner dungeonRunner = new DungeonRunner();
            dungeonRunner.Run();
            Console.ReadKey(true);
            dungeonRunner.Stop();
        }

    }
}
