using System;
using System.Collections.Generic;

namespace Engine.Pools
{
    //The purpose of this class is to take in a set of items, which probabilites cumutively
    // adding to 100 (though not restricted to 100), and allow a item to be picked
    // from them randomly, skewed by their probability. 
    // (i.e. an item with probability 20% has a 20% change of being picked randomly)
    public class WeightedPool<Type>
    {
        private List<Item> items = new List<Item>();
        //keep track of the total weight - important when getting an item
        private float totalWeight = 0;
        //random object used to generate random numbers.
        private Random random = new Random();

        //Container to store item - probability pair
        // easier to store them this way than in a dictionary, makes search more simple.
        private struct Item
        {
            public readonly Type obj;
            public readonly float weight;
                
            public Item(Type obj, float weight)
            {
                this.obj = obj;
                this.weight = weight;
            }
        }

        public WeightedPool()
        {

        }

        public void Add(Type item, float probability)
        {
            //update total weight for grabbing from probabilities
            totalWeight += probability;
            items.Add(new Item(item, totalWeight));
        }

        //generate a random weight, return item which has a weight not covered by generated num.
        public Type GetRandom()
        {
            float num = (float)random.NextDouble() * totalWeight;
            foreach (Item item in items)
            {
                if (item.weight >= num)
                {
                    return item.obj;
                }
            }
            return default; // if nothing found.
        }
    }
}
