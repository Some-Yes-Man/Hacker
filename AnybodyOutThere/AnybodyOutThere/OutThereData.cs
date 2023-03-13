namespace AnybodyOutThere {
    internal class OutThereData {
        public const int COUNT_RESULT = 12;

        public int X { get; set; }
        public int Y { get; set; }
        public int[] Data { get; private set; }

        public OutThereData() {
            this.Data = new int[COUNT_RESULT];
        }
    }
}
