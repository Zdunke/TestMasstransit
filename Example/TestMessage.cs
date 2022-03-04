using System;
using System.Collections.Generic;

namespace Example
{
    public class TestMessage
    {
        public int Counter { get; set; }

        public DateTime Timestamp { get; set; }

        //public Guid A1 { get; set; } = Guid.NewGuid();

        //public Guid A2 { get; set; } = Guid.NewGuid();

        //public Guid A3 { get; set; } = Guid.NewGuid();

        //public Guid A4 { get; set; } = Guid.NewGuid();

        public override string ToString()
        {
            return $"[{Counter}] [{Timestamp}]";
        }
    }
}