using System;
using System.Collections.Generic;

namespace NoFullACK {

    class Program {
        public static long Ackermann(long m, long n) {
            if (m == 0) {
                return n + 1;
            }
            if ((m > 0) && (n == 0)) {
                return Ackermann(m - 1, 1);
            }
            if ((m > 0) && (n > 0)) {
                return Ackermann(m - 1, Ackermann(m, n - 1));
            }
            return 0;
        }

        public static long AckermannImproved(long m, long n) {
            while (m != 0) {
                if (n == 0) {
                    n = 1;
                }
                else {
                    n = AckermannImproved(m, n - 1);
                }
                m = m - 1;
            }
            return n + 1;
        }

        public static long AckermannIterative(long m, long n) {
            Stack<long> stack = new Stack<long>();
            stack.Push(m);
            while (stack.Count != 0) {
                m = stack.Pop();
                if (m == 0)
                    n = n + 1;
                else if (n == 0) {
                    stack.Push(m - 1);
                    n = 1;
                }
                else {
                    stack.Push(m - 1);
                    stack.Push(m);
                    --n;
                }
            }
            return n;
        }

        static void Main(string[] args) {
            Bauer ackerer = new Bauer();
            ackerer.Run();
            Console.ReadKey(true);
        }

        class Bauer {
            Dictionary<Tuple<long, long>, long> precomputedValues = new Dictionary<Tuple<long, long>, long>();

            public long AckermannInformed(long m, long n) {
                if (m == 0) {
                    return n + 1;
                }
                long a = -1;
                long b = -1;
                if ((m > 0) && (n == 0)) {
                    if (!this.precomputedValues.TryGetValue(new Tuple<long, long>(m - 1, 1), out a)) {
                        a = AckermannInformed(m - 1, 1);
                        this.precomputedValues.Add(new Tuple<long, long>(m - 1, 1), a);
                    }
                    return a;
                }
                if (!this.precomputedValues.TryGetValue(new Tuple<long, long>(m, n - 1), out a)) {
                    a = AckermannInformed(m, n - 1);
                    this.precomputedValues.Add(new Tuple<long, long>(m, n - 1), a);
                }
                if (!this.precomputedValues.TryGetValue(new Tuple<long, long>(m - 1, a), out b)) {
                    b = AckermannInformed(m - 1, a);
                    this.precomputedValues.Add(new Tuple<long, long>(m - 1, a), b);
                }
                return b;
            }

            public void Run() {
                for (int i = 0; i <= 3; i++) {
                    for (int j = 0; j <= 15; j++) {
                        Console.WriteLine(AckermannInformed(i, j));
                    }
                }
            }
        }
    }
}
