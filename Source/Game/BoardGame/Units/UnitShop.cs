using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Hex.UI;
using Engine.UI;

namespace Hex.BoardGame
{
    public class UnitShop
    {
        private static int refreshCost = 2;
        private static int capacity = 3;


        public struct ShopEntry
        {
            public readonly Type unitType;
            public readonly int cost;
            private bool sold;
            public bool Sold { get { return sold; } }

            public ShopEntry(Type unitType)
            {
                this.unitType = unitType;
                cost = (int)unitType.GetField("Cost").GetRawConstantValue();
                sold = false;
            }

            public void Sell()
            {
                sold = true;
            }
        }
        private ShopEntry[] unitsForSale = new ShopEntry[capacity];

        private UnitShopBox display;

        private Player owner;

        public UnitShop(GraphicsDevice graphicsDevice, Player owner)
        {
            this.owner = owner;

            //generate the ui elements.
            Vector2 pos = WindowTools.PaddingToPixelCoordinate(1f, 0.5f, -10, 0);
            display = new UnitShopBox(this, graphicsDevice, capacity, pos);
            //anchor to the middle right as displayed on the right hand side of the screen.
            display.SetAnchorPoint(1f, 0.5f);
            FillShop();
        }

        public void SellEntry(object sender, ShopButtonArgs args)
        {
            ShopEntry entry = unitsForSale[args.entryNumber];
            //if the entry in question is not already sold, and the owners bench has a free slot
            if (!entry.Sold && !owner.IsBenchFull() && owner.Money >= entry.cost)
            {
                //sell it (note entry is NOT ref)
                unitsForSale[args.entryNumber].Sell();

                //get the cost of the unit and reduce the players money by that amount.
                owner.Money -= entry.cost;
                
                //add the unit to the players bench

                //Instantiate a new unit by invoking the constructor from the units type:
                var constructors = unitsForSale[args.entryNumber].unitType.GetConstructors();
                Unit unit = (Unit)constructors.First().Invoke(new object[] { owner.Id });
                unit.AddToBench(owner);
            }
        }

        private void FillShop()
        {
            //using standard random here at the moment, in future could do something like stages with weighted pools.

            //get the unit pool and choose a random unit type from it until the shop capacity is reached.
            Type[] unitPool = UnitPool.GetUnitPool();
            Random rand = new Random();
            for (int i = 0; i < capacity; i++)
            {
                Type unitType = unitPool[rand.Next(0, unitPool.Length)];

                //create a respective shop entry.
                unitsForSale[i] = new ShopEntry(unitType);
            }
        }

        public void Reset()
        {
            unitsForSale = new ShopEntry[capacity];
            FillShop();
        }

        public void Refresh()
        {
            //Re-generate shop entries, at a cost to the player.
            if (owner.Money >= refreshCost)
            {
                Reset();
                owner.Money -= refreshCost;
            }

        }

        public void Update(GameTime gameTime)
        {
            display.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, Dictionary<Type, Texture2D> unitSprites, Texture2D moneyIcon, SpriteFont font)
        {
            display.Draw(spriteBatch, owner, unitsForSale, unitSprites, moneyIcon, font);
        }
    }
}
