using System;
using System.Collections.Generic;
using System.Text;

namespace ShreddedAndScrambled {
    public class BlackListEntry {

        public int Id { get; set; }
        public Direction Edge { get; set; }
        public int BlackListedId { get; set; }

    }
}
