namespace DiceRoller_v2
{
    // Abstract Framework
    [Serializable]
    public abstract class RollableObject
    {
        // Properties
        public string Name { get; private set; }
        public int Modifier { get; private set; }

        // Constructor
        public RollableObject(string name, int modifier)
        {
            Name = name;
            Modifier = modifier;
        }

        // Methods
        public void Rename(string name) { Name = name; }
        public void SetModifier(int modifier){ Modifier = modifier; }
        public override string ToString() { return Name + " (" + (Modifier >= 0 ? "+" + Modifier.ToString() : Modifier.ToString()) + ")"; }
    }

    // Die Class
    public class Die : RollableObject
    {
        // Properties
        public int[] Values { get; private set; }
        public Random Rnd { get; private set; }

        // Constructors
        // Constructs a die with custom face values
        public Die(string name, int modifier, params int[] values) : base(name, modifier)
        {
            Values = values;
            Rnd = new Random();
        }

        // Constructs a die with n faces, or face values 1, 2, ..., n
        public Die(string name, int modifier, int faces) : base(name, modifier)
        {
            Values = new int[faces];
            for (int i = 0; i < faces; i++)
            {
                Values[i] = i + 1;
            }
            Rnd = new Random();
        }

        // Methods
        // Add a value to the die, and a face for it
        public void AddFace(int val)
        {
            int[] temp = new int[Values.Length + 1];
            Array.Copy(Values, temp, Values.Length);
            temp[Values.Length] = val;

            Values = temp;
        }

        // Remove a face and its value from the die
        public int RemoveFace(int val)
        {
            // Returns 0 on success
            // Error Codes:
            //      -1: Provided value is last face
            //      -2: Provided value doesn't exist

            // Error handling
            if (Values.Length == 1)
                return -1;

            if (!Values.Contains(val))
                return -2;

            // Remove the face
            int[] temp = new int[Values.Length - 1];
            
            int index = 0;
            while (index < Values.Length)
            {
                if (Values[index] != val)
                    temp[index] = Values[index];
                else
                {
                    Array.Copy(Values, index + 1, temp, index, Values.Length - (index + 1));
                    Values = temp;
                    break;
                }

                index++;
            }

            return 0;
        }

        // Return a random face value
        public int Roll()
        {
            return Values[Rnd.Next(Values.Length)] + Modifier;
        }

        // Return a stringified representation
        public override string ToString()
        {
            string str = Name;    
            
            // Modifier
            str += " (" + (Modifier >= 0 ? "+" + Modifier.ToString() : Modifier.ToString()) + ")";       // Add Modifier - ternary includes + symbol for positive values
            
            // Values
            int printCap = Math.Min(Values.Length, 25);
            for (int i = 0; i < printCap; i++)                                                           // Add Values     
            {
                str += " " + Values[i].ToString();
            }
            if (Values.Length > 25)
                str += "... " + Values[Values.Length - 1].ToString();

            return str;
        }
    }

    // Dice Collection Class
    public class DiceCollection : RollableObject
    {
        // Properties
        public PointerNode Contents { get; private set; }

        // Constructor
        public DiceCollection(string name, int modifier, PointerNode contents = null) : base(name, modifier) 
        {
            Contents = new PointerNode(contents, null);
        }

        // Methods
        public int Add(PointerNode item)
        {
            // Returns 0 on success
            // Error Codes:
            //      -1: Provided item is null
            //      -2: Provided item is DiceCollection which contains this DiceCollection
            //      -3: Provided item is this DiceCollection

            // Error handling
            if (item == null)
                return -1;

            PointerNode cur;

            if (item.Object.Collection != null)
            {
                // Disallow putting two collections inside each other
                cur = item.Object.Collection.Contents;

                while (cur != null)
                {
                    if (cur.Object != null && cur.Object.Collection != null && cur.Object.Collection.Contains(this))
                        return -2;

                    cur = cur.Next;
                }

                // Disallow putting a collection in itself
                if (item.Object.Collection.Name == Name)
                    return -3;
            }

            // Add the item
            cur = Contents;
            while (cur.Next != null)
                cur = cur.Next;

            cur.Next = item;

            return 0;
        }
        public bool Remove(string item) 
        {
            // Handle empty DiceCollection
            if (Contents.Next == null)
                return false;

            // Attempt to remove
            PointerNode cur = Contents.Next;
            PointerNode prev = Contents;

            while (cur != null)
            {
                if ((cur.Object.Die != null && cur.Object.Die.Name == item) || (cur.Object.Collection != null && cur.Object.Collection.Name == item))
                {
                    prev.Next = cur.Next;
                    return true;
                }

                cur = cur.Next;
                prev = prev.Next;
            }

            // Provided item was not found
            return false;
        }
        public bool Contains(RollableObject obj) 
        {
            // Error handling
            if (obj == null)
                return false;

            PointerNode cur = Contents.Next;
            while (cur != null)
            {
                // Check if this DiceCollection contains a Die that is the provided RollableObject
                if (cur.Object.Die != null && cur.Object.Die.Name == obj.Name)
                    return true;

                // Check if this DiceCollection contains a DiceCollection that is or contains the provided RollableObject
                else if (cur.Object.Collection != null && (cur.Object.Collection.Name == obj.Name || cur.Object.Collection.Contains(obj)))
                    return true;

                cur = cur.Next;
            }

            return false;
        }

        public int Roll() 
        {
            (string, int, bool)[] results = RollPrecisely();
            int total = 0;

            for (int i = 0; i < results.Length; i++)
            {
                // Build the total out of non-subtotal values
                if (results[i].Item3 == false)
                    total += results[i].Item2;
            }

            return total;
        }
        public (string, int, bool)[] RollPrecisely(bool outermostIteration = true) 
        {
            PointerNode cur = Contents.Next;
            List<(string, int, bool)> results = new List<(string, int, bool)>();
            int subtotal = Modifier;

            // Add this DiceCollection's modifier
            results.Add((Name, Modifier, false));

            // Loop through all contents
            while (cur != null)
            {
                // If current content has a Die, roll it and add the result
                if (cur.Object.Die != null)
                {
                    int roll = cur.Object.Die.Roll();
                    results.Add((cur.Object.Die.Name, roll, false));
                    subtotal += roll;
                }

                // If the current content has a DiceCollection, roll it and add the result
                if (cur.Object.Collection != null)
                {
                    foreach ((string, int, bool) result in cur.Object.Collection.RollPrecisely(false))
                    {
                        // Add each result to the main list, do not use subtotals to calculate subtotals
                        results.Add(result);
                        if (result.Item3 == false)
                        {
                            subtotal += result.Item2;
                        }
                    }
                }

                cur = cur.Next;
            }

            // Add subtotal for this bag
            results.Add((Name + (outermostIteration ? " Total" : " Subtotal"), subtotal, true));

            return results.ToArray();
        }
        public override string ToString() 
        {
            return InternalToString(1);
        }
        private string InternalToString(int tabcount)
        {
            string str = Name;
            Queue<DiceCollection> collections = new Queue<DiceCollection>();
            PointerNode cur = Contents.Next;

            // Modifier
            str += " (" + (Modifier >= 0 ? "+" + Modifier.ToString() : Modifier.ToString()) + "):\n";

            // Collections
            while (cur != null)
            {
                // Contents
                if (cur.Object.Collection != null)
                    collections.Enqueue(cur.Object.Collection);
                if (cur.Object.Die != null)
                {
                    // Indentation
                    for (int i = 0; i < tabcount; i++)
                        str += "\t";

                    str += cur.Object.Die.ToString() + "\n";
                }

                cur = cur.Next;
            }

            // Add all the loose dice in the DiceCollection to the string
            while (collections.Count > 0)
            {
                // Indentation
                for (int i = 0; i < tabcount; i++)
                    str += "\t";

                // Contents
                str += collections.Dequeue().InternalToString(tabcount + 1);
            }

            return str;
        }
    }
}
