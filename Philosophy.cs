using System;
using System.Threading;

namespace DinnerPhilisohy2
{
    public enum SolutionType { Naive, OrderedResources, TryEnter, Waiter }
    public class Philosophy
    {
        private readonly string numberFill;
        private readonly object leftFork;
        private readonly object rightFork;
        private readonly int leftForkId;
        private readonly int rightForkId;
        
        private readonly Random rdn;
        private static int totalStatistics = 0;
        private static int retreatsCount = 0;

        public Philosophy(string numberFill, object leftFork, object rightFork, int leftForkId, int rightForkId)
        {
            this.numberFill = numberFill;
            this.leftFork = leftFork;
            this.rightFork = rightFork;
            this.leftForkId = leftForkId;
            this.rightForkId = rightForkId;
            this.rdn = new Random(numberFill.GetHashCode());
        }

        public void Start(SolutionType solution)
        {
            for (int i = 0; i < 5; i++)
            {
                int time = rdn.Next(100, 301);
                Console.WriteLine($" {numberFill},  думает ({time}) ms");
                Thread.Sleep(time);

                Eat(solution);
            }
            Console.WriteLine();
        }
        private void Eat(SolutionType solution)
        {
            int time = rdn.Next(100, 301);
            switch (solution)
            {
                case SolutionType.Naive:
                    EatNaive(time);
                    break;
                case SolutionType.OrderedResources:
                    EatOrdered(time);
                    break;
                case SolutionType.TryEnter:
                    EatTryEnter(time);
                    break;
                case SolutionType.Waiter:
                    EatWaiter(time);
                    break;
            }

            Interlocked.Increment(ref totalStatistics);
        }

        //1
        private void EatNaive(int time)
        {
            Console.WriteLine($" {numberFill}: хочет есть");
            lock (leftFork)
            {
                Console.WriteLine($"  \t{numberFill}: взял первую вилку");
                Thread.Sleep(50);
                lock (rightFork)
                {
                    Console.WriteLine($"  \t{numberFill}: взял вторую вилку");
                    Thread.Sleep(time);
                    Console.WriteLine($"  \t{numberFill}: закончил есть");
                }
                Console.WriteLine($"  \t{numberFill}: положил вторую вилку");
            }
            Console.WriteLine($"  \t{numberFill}: положил первую вилку");
        }
        //2
        private void EatOrdered(int time)
        {
            object first = (leftForkId < rightForkId) ? leftFork : rightFork;
            object second = (leftForkId < rightForkId) ? rightFork : leftFork;

            Console.WriteLine($" {numberFill}: хочет есть");
            lock (first)
            {
                Console.WriteLine($"  \t{numberFill}: взял первую вилку");
                lock (second)
                {
                    Console.WriteLine($"  \t{numberFill}: взял вторую вилку");
                    Thread.Sleep(time);
                    Console.WriteLine($"  \t{numberFill}: закончил есть");
                }
                Console.WriteLine($"  \t{numberFill}: положил вторую вилку");
            }
            Console.WriteLine($"  \t{numberFill}: положил первую вилку");
        }
        //3
        private void EatTryEnter(int time)
        {
            Console.WriteLine($" {numberFill}: хочет есть");
            while (true)
            {
                if (Monitor.TryEnter(leftFork, 100))
                {
                    if (Monitor.TryEnter(rightFork, 100))
                    {
                        try
                        {
                            Console.WriteLine($"  \t{numberFill}: взял первую вилку");
                            Console.WriteLine($"  \t{numberFill}: взял вторую вилку");
                            Thread.Sleep(time);
                            Console.WriteLine($"  \t{numberFill}: закончил есть");
                        }
                        finally
                        {
                            Monitor.Exit(rightFork);
                            Monitor.Exit(leftFork);
                        }
                        Console.WriteLine($"  \t{numberFill}: положил вторую вилку");
                        Console.WriteLine($"  \t{numberFill}: положил первую вилку");
                        return;
                    }
                    else
                    {                        
                        Monitor.Exit(leftFork);
                        Interlocked.Increment(ref retreatsCount);
                    }
                }                               
                int delay = rdn.Next(50, 200);
                Thread.Sleep(delay);
            }
        }
        private static readonly object waitLock = new object();
        private static int seatedPhilosophers = 0;

        private void EatWaiter(int time)
        {            
            lock (waitLock)
            {
                while (seatedPhilosophers >= 4)
                {
                    Monitor.Wait(waitLock);
                }
                seatedPhilosophers++;
            }

            try
            {
                Console.WriteLine($"  \t{numberFill}: сел за стол (место занято)");
                Console.WriteLine($"  \t{numberFill}: хочет есть");

                lock (leftFork)
                {
                    Console.WriteLine($"  \t{numberFill}: взял первую вилку");
                    lock (rightFork)
                    {
                        Console.WriteLine($"  \t{numberFill}: взял вторую вилку");
                        Thread.Sleep(time);
                        Console.WriteLine($"  \t{numberFill}: закончил есть");
                    }
                    Console.WriteLine($"  \t{numberFill}: положил вторую вилку");
                }
                Console.WriteLine($"  \t{numberFill}: положил первую вилку");
            }
            finally
            {                
                lock (waitLock)
                {
                    seatedPhilosophers--;
                    Monitor.PulseAll(waitLock);
                }
                Console.WriteLine($"  \t{numberFill}: встал из-за стола");
            }
        }
        public static int GetTotalStatistic()
        {
            return totalStatistics;
        }
        public static int GetRetreats()
        {
            return retreatsCount;
        } 
        public static void ResetStats()
        {
            totalStatistics = 0;
            retreatsCount = 0;
            seatedPhilosophers = 0;
        }





    }
}
