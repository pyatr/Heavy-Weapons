using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.Core;

namespace XRL.World.Parts
{
    [Serializable]
    public class HW_Artillery : IPart
    {
        private const int GROUND_LEVEL = 10;
		
        public Guid FireHowitzerActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry FireHowitzerActivatedAbility;
        public bool CanFireAtUnexplored;
        public int SpreadRadius;

        public HW_Artillery() { }

        public override bool SameAs(IPart p) => base.SameAs(p);        

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "Equipped");
            Object.RegisterPartEvent((IPart)this, "Unequipped");
            Object.RegisterPartEvent((IPart)this, "CommandFireHowitzer");
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandFireHowitzer")
            {
                GameObject owner = ParentObject.pPhysics.Equipped;

                Cell currentCell = owner.GetCurrentCell();

                if (currentCell == null || currentCell.ParentZone == null)
                    return true;

                if (currentCell.ParentZone.IsWorldMap())
                {
                    if (owner.IsPlayer())
                        Popup.Show("You may not use this ability on the world map.", true);
                    return true;
                }

                if (currentCell.ParentZone.Z != GROUND_LEVEL)
                {
                    if (owner.IsPlayer())
                        Popup.Show("You can only use this ability at ground level.", true);
                    return true;
                }

                MagazineAmmoLoader magazine = ParentObject.GetPart("MagazineAmmoLoader") as MagazineAmmoLoader;
                GameObject shell = magazine.Ammo;
                if (shell == null)
                {
                    if (owner.IsPlayer())
                        Popup.Show("Your howitzer is not loaded.", true);
                    return true;
                }

                string ammoType = shell.Blueprint;
                string ammoTypeButDeeper = shell.GetPart<AmmoGeneric>().ProjectileObject;

                List<Cell> CellsWithinArea = new List<Cell>();
                if (CanFireAtUnexplored)
                    CellsWithinArea = ParentObject.pPhysics.Equipped.pPhysics.PickCircle(SpreadRadius, 80, false, AllowVis.Any);
                else
                    CellsWithinArea = ParentObject.pPhysics.Equipped.pPhysics.PickCircle(SpreadRadius, 80, false, AllowVis.OnlyExplored);

                if (CellsWithinArea.Count > 0)
                {
                    if (owner.pPhysics != null)
                        owner.pPhysics.PlayWorldSound(ParentObject.GetTag("MissileFireSound", (string)null), 0.5f, 0.0f, true, (Cell)null);
                    Cell randomCell;
                    //int i = 0;
                    //do
                    //{
                        //i++;
                    randomCell = CellsWithinArea.GetRandomElement();
                    //} while (randomCell.IsSolid() && i < 30);

                    GameObject GOexample = GameObjectFactory.Factory.CreateObject(ammoTypeButDeeper);
                    GameObject GO = null;
                    if (GOexample.HasPart("GasGrenade"))
                    {
                        GO = GameObjectFactory.Factory.CreateObject(ammoTypeButDeeper);
                        randomCell.AddObject(GO);
                        GasGrenade gr = GO.GetPart("GasGrenade") as GasGrenade;
                        GO.pPhysics.PlayWorldSound(ParentObject.GetTag("DetonatedSound", (string)null), 0.5f, 0.0f, true, (Cell)null);
                        gr.Detonate(randomCell, owner);
                    }
                    if (GOexample.HasPart("ExplodeOnHit"))
                    {
                        if (GO == null)
                            GO = GameObjectFactory.Factory.CreateObject(ammoTypeButDeeper);
                        randomCell.AddObject(GO);
                        ExplodeOnHit d = GO.GetPart("ExplodeOnHit") as ExplodeOnHit;
                        GO.pPhysics.PlayWorldSound(ParentObject.GetTag("DetonatedSound", (string)null), 0.5f, 0.0f, true, (Cell)null);
                        d.Detonate(owner);
                    }
                    if (GO != null)
                        GO.Destroy((string)null, true, false);
                    GOexample.Destroy((string)null, true, false);

                    magazine.Unload(owner);
                    List<GameObject> inventory = owner.GetPart<Inventory>().Objects;
                    foreach (GameObject ammo in inventory)
                    {
                        if (ammo.Blueprint == ammoType)
                        {
                            shell = ammo;
                            break;
                        }
                    }

                    Stacker stackerino = shell.GetPart("Stacker") as Stacker;
                    if (shell != null && stackerino != null)
                    {
                        if (stackerino.Number > 1)
                            shell.RemoveOne();
                        else
                            shell.Destroy();
                    }
                }
                return true;
            }

            if (E.ID == "Equipped")
            {
                GameObject go = E.GetParameter<GameObject>("EquippingObject");
                if (go.HasPart("HeavyWeapons"))
                {
                    go.RegisterPartEvent(this, "CommandFireHowitzer");
                    ActivatedAbilities pAA = go.GetPart<ActivatedAbilities>();
                    if (pAA != null)
                    {
                        this.FireHowitzerActivatedAbilityID = pAA.AddAbility("Fire howitzer at an area", "CommandFireHowitzer", "Items");
                        this.FireHowitzerActivatedAbility = pAA.AbilityByGuid[this.FireHowitzerActivatedAbilityID];
                    }
                }
                return true;
            }

            if (E.ID == "Unequipped")
            {
                GameObject go = E.GetParameter<GameObject>("UnequippingObject");
                if (go.HasPart("HeavyWeapons"))
                {
                    go.UnregisterPartEvent(this, "CommandFireHowitzer");
                    if (this.FireHowitzerActivatedAbilityID != Guid.Empty)
                    {
                        ActivatedAbilities pAA = go.GetPart<ActivatedAbilities>();
                        pAA.RemoveAbility(this.FireHowitzerActivatedAbilityID);
                        this.FireHowitzerActivatedAbilityID = Guid.Empty;
                    }
                    return true;
                }
            }
            return true;
        }
    }
}
/*=== Heavy Weapons Errors ===
\HW_AddPopulationsFromXML.cs(59,83): error CS0266: Cannot implicitly convert type 'int' to 'uint'. An explicit conversion exists (are you missing a cast?)
\HW_Artillery.cs(93,37): error CS1503: Argument 1: cannot convert from 'XRL.World.GameObject' to 'XRL.World.Cell'
== Warnings ==
None*/