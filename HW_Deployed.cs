using System;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;

//I used Eboreg's constriction as a base

namespace XRL.World.Parts.Effects
{
    [Serializable]
    internal class HW_Deployed : Effect
    {
        public int HW_AccuracyBonus;

        public HW_Deployed()
        {
            base.DisplayName = "Deployed";
            this.Duration = 1;
            this.HW_AccuracyBonus = 10;
        }

        public HW_Deployed(int FeedMeYourNumber)
        {
            base.DisplayName = "Deployed";
            this.Duration = 1;
            this.HW_AccuracyBonus = FeedMeYourNumber;
        }

        public override bool Apply(GameObject Object)
        {
            return true;
        }

        public override void Remove(GameObject Object)
        {
            base.Remove(Object);
        }

        public override void Register(GameObject Object)
        {
            Object.Statistics["DV"].Penalty += 5;
            Object.ModIntProperty("IncomingAimModifier", 10);
            Object.ModIntProperty("MissileWeaponAccuracyBonus", -this.HW_AccuracyBonus);//I'm not sure if the damn thing works.
            Object.RegisterEffectEvent(this, "BeginMove");
            base.Register(Object);
        }

        public override void Unregister(GameObject Object)
        {
            Object.Statistics["DV"].Penalty -= 5;
            Object.ModIntProperty("IncomingAimModifier", -10);
            Object.ModIntProperty("MissileWeaponAccuracyBonus", this.HW_AccuracyBonus);
            Object.UnregisterEffectEvent(this, "BeginMove");
            base.Unregister(Object);
        }

        public override bool Render(RenderEvent E)
        {
            int num = XRLCore.CurrentFrame % 120;
            if (num > 35 && num < 45)
            {
                E.Tile = null;
                E.RenderString = "\x001F";
                E.ColorString = "&g";
            }
            return true;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginMove")
            {
                return false;
            }
            return base.FireEvent(E);
        }
    }
}