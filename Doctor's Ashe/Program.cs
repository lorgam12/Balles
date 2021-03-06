﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Font = SharpDX.Direct3D9.Font;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Ashe
{
    internal class Program
    {
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Item Botrk;
        public static Item Bil;
        public static Font Thm;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Menu Menu, ComboMenu, JungleClearMenu, HarassMenu, LaneClearMenu, Misc, Items, Skin;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

// Menu

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Ashe")) return;
            Chat.Print("Doctor's Ashe Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q, 600);
            W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Linear, 0, int.MaxValue, 60);
            W.AllowedCollisionCount = 0;
            E = new Spell.Skillshot(SpellSlot.E, 10000, SkillShotType.Linear);
            E.AllowedCollisionCount = int.MaxValue;
            R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 250, 1600, 100);
            R.AllowedCollisionCount = -1;
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Menu = MainMenu.AddMenu("Doctor's Ashe", "Ashe");
            Menu.AddGroupLabel("Doctor7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Combo"));
            ComboMenu.Add("KeepCombo", new CheckBox("Kepp Mana For [R]"));
            ComboMenu.AddGroupLabel("KillSteal Settings");
            ComboMenu.Add("RAoe", new CheckBox("Use [R] Aoe"));
            ComboMenu.Add("minRAoe", new Slider("Use [R] Aoe If Hit x Enemies", 2, 1, 5));
            ComboMenu.Add("ComboSL", new CheckBox("Use [R] On Selected Target", false));
            ComboMenu.Add("RKs", new CheckBox("Automatic [R] KillSteal"));
            ComboMenu.Add("WKs", new CheckBox("Automatic [W] KillSteal"));
            ComboMenu.Add("RKb", new KeyBind(" Semi [R] KillSteal", false, KeyBind.BindTypes.HoldActive, 'T'));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("KeepHarass", new CheckBox("Keep Mana For [R]", false));
            HarassMenu.Add("manaHarass", new Slider("Mana Harass", 50, 0, 100));

            LaneClearMenu = Menu.AddSubMenu("Laneclear Settings", "Clear");
            LaneClearMenu.AddGroupLabel("Laneclear Settings");
            LaneClearMenu.Add("ClearQ", new CheckBox("Use [Q] Laneclear", false));
            LaneClearMenu.Add("ClearW", new CheckBox("Use [W] Laneclear", false));
            LaneClearMenu.Add("manaFarm", new Slider("Mana LaneClear", 60, 0, 100));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("jungleQ", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("jungleW", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("manaJung", new Slider("Mana JungleClear", 20, 0, 100));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Misc = Menu.AddSubMenu("Misc Settings", "Draw");
            Misc.AddGroupLabel("Anti Gapcloser");
            Misc.Add("antiGap", new CheckBox("Anti Gapcloser", false));
            Misc.Add("inter", new CheckBox("Use [R] Interupt"));
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings", false));
            Misc.Add("DrawE", new CheckBox("Draw [E]"));
            Misc.Add("DrawW", new CheckBox("Draw [W]", false));
            Misc.Add("Notifications", new CheckBox("Notifications Can Kill With [R]"));

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 6, "1", "2", "3", "4", "5", "6", "7", "8"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interupt;
            Orbwalker.OnPostAttack += ResetAttack;

        }

        // Game OnTick

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                RLogic();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            KillSteal();
            Item();
            if (_Player.SkinId != Skin["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }

// Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue) return;
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Misc["Notifications"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (target.IsValidTarget(W.Range) && Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.AttackShield)
                {
                    DrawFont(Thm, "[R] Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }
        }

        public static bool QReady
        {
            get { return Player.Instance.HasBuff("AsheQCastReady"); }
        }

// Flee Mode

        private static void Flee()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (W.IsReady() && target.IsValidTarget(W.Range))
            {
                W.Cast(target);
            }
        }

// Skin Changer

        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<ComboBox>().CurrentValue;
        }

        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

// Interrupt

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = Misc["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
            if (Inter && R.IsReady() && i.DangerLevel == DangerLevel.High && W.IsInRange(sender))
            {
                R.Cast(sender);
            }
        }

//Harass Mode

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var Keep = HarassMenu["KeepHarass"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["manaHarass"].Cast<Slider>().CurrentValue;
            if (_Player.ManaPercent < mana) return;
            if (target != null)
            {
                if (useQ && target.IsValidTarget(Q.Range) && QReady)
                {
                    if (Keep && R.IsReady())
                    {
                        if (Player.Instance.Mana > Q.Handle.SData.Mana + R.Handle.SData.Mana)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        Q.Cast();
                    }
                }
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (R.IsReady())
                    {
                        if (Player.Instance.Mana > W.Handle.SData.Mana + R.Handle.SData.Mana)
                        {
                            W.Cast(target);
                        }
                    }
                    else
                    {
                        W.Cast(target);
                    }
                }
            }
        }

//Combo Mode

        private static void Combo()
        {
            var targetS = TargetSelector.SelectedTarget;
            var useSL = ComboMenu["ComboSL"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var Keep = ComboMenu["KeepCombo"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(W.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useW && W.IsReady() && target.IsValidTarget(W.Range) && Player.Instance.Mana > W.Handle.SData.Mana + R.Handle.SData.Mana)
                {
                    if (Keep && R.IsReady())
                    {
                        if (Player.Instance.Mana > W.Handle.SData.Mana + R.Handle.SData.Mana)
                        {
                            W.Cast(target);
                        }
                    }
                    else
                    {
                        W.Cast(target);
                    }
                }
                if (useR && R.IsReady() && target.IsValidTarget(W.Range) && _Player.HealthPercent <= 70)
                {
                    R.Cast(target);
                }
            }
            if (useSL && targetS != null)
            {
                if (R.IsReady() && targetS.IsValidTarget(1500))
                {
                    R.Cast(targetS);
                }
            }
        }

//Use Q ResetAttack

        private static void ResetAttack(AttackableUnit target, EventArgs args)
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var Keep = ComboMenu["KeepCombo"].Cast<CheckBox>().CurrentValue;
            if (useQ && QReady && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target.IsValidTarget(650))
            {
                if (Keep && R.IsReady())
                {
                    if (Player.Instance.Mana > Q.Handle.SData.Mana + R.Handle.SData.Mana)
                    {
                        Q.Cast();
                    }
                }
                else
                {
                    Q.Cast();
                }
            }
        }

//LaneClear Mode

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["ClearQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["ClearW"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["manaFarm"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (_Player.ManaPercent < mana) return;
            foreach (var minion in minions)
            {
                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && minions.Count() >= 3)
                {
                    W.Cast(minion);
                }
                if (useQ && minion.IsValidTarget(Q.Range) && QReady && minions.Count() >= 3)
                {
                    Q.Cast();
                }
            }
        }

// JungleClear Mode

        private static void JungleClear()
        {
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(W.Range));
            var useQ = JungleClearMenu["jungleQ"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["jungleW"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["manaJung"].Cast<Slider>().CurrentValue;
            if (_Player.ManaPercent < mana) return;
            if (monster != null)
            {
                if (useQ && QReady && monster.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && monster.IsValidTarget(W.Range))
                {
                    W.Cast(monster);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

// KillSteal

        private static void KillSteal()
        {
            var target = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(2000) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie);
            var RKill = ComboMenu["RKs"].Cast<CheckBox>().CurrentValue;
            var WKill = ComboMenu["WKs"].Cast<CheckBox>().CurrentValue;
            var RKey = ComboMenu["RKb"].Cast<KeyBind>().CurrentValue;
            foreach (var target2 in target)
            {
                if (RKill && R.IsReady())
                {
                    if (target2.Health + target2.AttackShield < Player.Instance.GetSpellDamage(target2, SpellSlot.R) && target2.IsValidTarget(2000) && !target2.IsInRange(Player.Instance, 1000))
                    {
                        R.Cast(target2);
                    }
                }
                if (RKey && R.IsReady())
                {
                    if (target2.Health + target2.AttackShield < Player.Instance.GetSpellDamage(target2, SpellSlot.R) && target2.IsValidTarget(2000))
                    {
                        R.Cast(target2);
                    }
                }
                if (WKill && W.IsReady())
                {
                    if (target2.Health + target2.AttackShield < Player.Instance.GetSpellDamage(target2, SpellSlot.W) && target2.IsValidTarget(W.Range))
                    {
                        W.Cast(target2);
                    }
                }
            }
        }

// AntiGap

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (Misc["antiGap"].Cast<CheckBox>().CurrentValue && args.Sender.Distance(_Player) < 325)
            {
                R.Cast(args.Sender);
            }
        }

// Use Items

        public static void Item()
        {
            var item = Items["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = Items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Items["ihpp"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(450, DamageType.Physical);
            if (target != null)
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && target.IsValidTarget(450))
                {
                    Bil.Cast(target);
                }
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(450)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
            }
        }

//R Logic

        private static void RLogic()
        {
            var target2 = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(2000) && !e.IsDead);
            var RAoe = ComboMenu["RAoe"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["minRAoe"].Cast<Slider>().CurrentValue;
            foreach (var target in target2)
            {
                if (RAoe && R.IsReady() && target.IsValidTarget(2000))
                {
                    var RPred = R.GetPrediction(target);
                    if (RPred.CastPosition.CountEnemiesInRange(300) >= MinR && RPred.HitChance >= HitChance.High)
                    {
                        R.Cast(RPred.CastPosition);
                    }
		    	}
            }
        }
    }
}
