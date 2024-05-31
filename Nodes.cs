namespace DiceRoller_v2
{
    // Object Node Class
    public class ObjectNode
    {
        // Properties
        public ObjectNode Next { get; set; }
        public Die Die { get; private set; }
        public DiceCollection Collection { get; private set; }

        // Constructor
        public ObjectNode(ObjectNode next = null, Die die = null, DiceCollection collection = null)
        {
            Next = next;
            Die = die;
            Collection = collection;
        }
    }

    // Pointer Node Class
    public class PointerNode
    {
        // Properties
        public PointerNode Next { get; set;}
        public ObjectNode Object { get; set; }

        // Constructor
        public PointerNode(PointerNode next = null, ObjectNode Object = null)
        {
            Next = next;
            this.Object = Object;
        }
    }
}
