using System.Text.Json.Serialization;

namespace DiceRoller_v2
{
    // UserObjectsList class
    public class UserObjectsList
    {
        // Properties
        private ObjectNode Root { get; set; }

        // Constructor
        public UserObjectsList()
        {
            Root = new ObjectNode();
        }

        // Support Methods
        // Return a PointerNode that points to the ObjectNode which has a .Next that points to the ObjectNode with the given object name
        // Return null on fail to find
        private PointerNode FindPrecursor(string name)
        {
            PointerNode cur = new PointerNode(null, Root);

            while (cur.Object.Next != null)
            {
                if ((cur.Object.Next.Die != null && cur.Object.Next.Die.Name == name) || (cur.Object.Next.Collection != null && cur.Object.Next.Collection.Name == name))
                    return cur;

                cur.Object = cur.Object.Next;
            }

            return null;
        }

        // Methods
        public int Add(RollableObject obj)
        {
            // Returns 0 on success
            // Error Codes:
            //      -1: Provided object is null
            //      -2: Provided object already exists (based on name)

            // Error Handling
            if (obj == null)
                return -1;

            // Duplicate handling
            if (Contains(obj.Name) != null)
                return -2;

            PointerNode cur = new PointerNode(null, Root);
            while (cur.Object.Next != null)
                cur.Object = cur.Object.Next;

            // Add the object
            if (obj is Die)
                cur.Object.Next = new ObjectNode(null, obj as Die, null);
            else
                cur.Object.Next = new ObjectNode(null, null, obj as DiceCollection);

            return 0;
        }

        public int Remove(RollableObject obj)
        {
            // Returns 0 on success
            // Error Codes:
            //      -1: Provided object is null
            //      -2: Provided object does not exist

            // Error Handling
            if (obj == null)
                return -1;

            // Find the precursor node
            PointerNode cur = FindPrecursor(obj.Name);
            if (cur == null)
                return -2;

            // Remove references in DiceCollections
            InternalRemove(obj.Name);

            // Remove
            cur.Object.Next = cur.Object.Next.Next;

            return 0;
        }
        private void InternalRemove(string name)
        {
            PointerNode cur = new PointerNode(null, Root);

            // Loop through all ObjectNodes
            while (cur.Object != null)
            {
                if (cur.Object.Collection != null)
                    while (cur.Object.Collection.Remove(name)) { } // If DiceCollection.Remove returns true, call it again. Catches DiceCollections that contian >1 reference to an object

                cur.Object = cur.Object.Next;
            }
        }

        public PointerNode Contains(string name)
        {
            // found is a PointerNode to the ObjectNode which has a .Next that points to the given name
            PointerNode found = FindPrecursor(name);

            // If found is not null, the object was found - so return a PointerNode to the next ObjectNode
            return found != null ? new PointerNode(null, found.Object.Next) : null;
        }

        public override string ToString()
        {
            PointerNode cur = new PointerNode(null, Root);
            string list = "";

            while (cur.Object != null)
            {
                if (cur.Object.Die != null)
                    list += "\n" + cur.Object.Die.ToString();
                else if (cur.Object.Collection != null)
                    list += "\n" + cur.Object.Collection.ToString();

                cur.Object = cur.Object.Next;
            }

            return list;
        }

        // Get a Serializable format for the UserObjectsList
        public SavedUserObjectsListFormat PreSerialize()
        {
            return new SavedUserObjectsListFormat(new PointerNode(null, Root));
        }
    }

    // Class structure for abstracting, saving, loading, and reconstructing UserObjectLists
    [Serializable]
    public class SavedUserObjectsListFormat
    {
        // RollableObjects abstractions to primitive data types
        public class DieFormat
        {
            // Properties
            public string Name { get; set; }
            public int Modifier { get; set; }
            public int[] Faces { get; set; }

            // Serializing Constructor
            public DieFormat(Die die)
            {
                Name = die.Name;
                Modifier = die.Modifier;
                Faces = die.Values;
            }

            // Deserializing Constructor
            [JsonConstructor]
            public DieFormat(string name, int modifier, int[] faces)
            {
                Name = name;
                Modifier = modifier;
                Faces = faces;
            }
        }
        public class DiceCollectionFormat
        {
            // Properties
            public string Name { get; set; }
            public int Modifier { get; set; }
            public string[] Contents { get; set; }

            // Serializing Constructor
            public DiceCollectionFormat(DiceCollection collection)
            {
                Name = collection.Name;
                Modifier = collection.Modifier;

                // Loop through all objects recording names
                List<string> containedNames = new List<string>();
                PointerNode cur = collection.Contents.Next;

                while (cur != null)
                {
                    containedNames.Add(cur.Object.Die != null ? cur.Object.Die.Name : cur.Object.Collection.Name);
                    cur = cur.Next;
                }

                Contents = containedNames.ToArray();
            }

            // Deserializing Constructor
            [JsonConstructor]
            public DiceCollectionFormat(string name, int modifier, string[] contents)
            {
                Name = name;
                Modifier = modifier;
                Contents = contents;
            }
        }

        // Properties
        public List<DieFormat> Dice { get; set; }
        public List<DiceCollectionFormat> DiceCollections { get; set; }

        // Serializing Constructor
        public SavedUserObjectsListFormat(PointerNode rootPointer)
        {
            // Loop through objects list and sort objects
            // Ignore first object, since it is the root
            // Store the name of each object in a DiceCollection with it

            Dice = new List<DieFormat>();
            DiceCollections = new List<DiceCollectionFormat>();
            PointerNode cur = rootPointer;

            while (cur.Object != null)
            {
                if (cur.Object.Die != null)
                    Dice.Add(new DieFormat(cur.Object.Die));
                else if (cur.Object.Collection != null)
                    DiceCollections.Add(new DiceCollectionFormat(cur.Object.Collection));

                cur.Object = cur.Object.Next;
            }
        }

        // Deserializing Constructor
        [JsonConstructor]
        public SavedUserObjectsListFormat(List<DieFormat> dice, List<DiceCollectionFormat> diceCollections)
        {
            Dice = dice;
            DiceCollections = diceCollections;
        }

        // Methods
        // Construct a fully functional UserObjectsList from a deserialized SavedUserObjectsListFormat
        public UserObjectsList ConstructObjectList()
        {
            UserObjectsList nodeList = new UserObjectsList();

            // Add Dice to list
            while (Dice.Count > 0)
            {
                // Pop first DieFormat
                DieFormat dieFormat = Dice.First();
                Dice.RemoveAt(0);

                // Create Die from DieFormat then add it to nodeList
                Die die = new Die(dieFormat.Name, dieFormat.Modifier, dieFormat.Faces);
                nodeList.Add(die);
            }

            // Add to nodeList
            // Because a DiceCollection could contain a collection created at a later date in the UserObjectList, DiceCollections are added recursively
            AddDiceCollectionsToDeserializedList(DiceCollections, ref nodeList);

            return nodeList;
        }
        private void AddDiceCollectionsToDeserializedList(List<DiceCollectionFormat> DiceCollections, ref UserObjectsList nodeList)
        {
            // Because a DiceCollection could contain a collection created at a later date in the UserObjectList, DiceCollections are added recursively
            // After each DiceCOllection is created, it calls this method to create the next. Once the method returns, it adds all contents

            // Pop first DiceCollectionFormat
            DiceCollectionFormat collectionFormat = DiceCollections.First();
            DiceCollections.RemoveAt(0);

            // Create DiceCollection from DiceCollectionFormat then add it to nodeList with no contents
            // Store the associated list of contents
            DiceCollection collection = new DiceCollection(collectionFormat.Name, collectionFormat.Modifier);
            nodeList.Add(collection); 

            // Make recursive call
            if (DiceCollections.Count > 0)
                AddDiceCollectionsToDeserializedList(DiceCollections, ref nodeList);

            // Add all DiceCollection contents
            for (int i = 0; i < collectionFormat.Contents.Length; i++)
            {
                collection.Add(nodeList.Contains(collectionFormat.Contents[i]));
            }
        }
    }
}