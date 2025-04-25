using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUSchedulingSimulator
{
    
    public static class TestGenerator
    {
        private static Random random = new Random(42); // Fixed seed for reproducibility
        
        
        public static List<Process> StandardTestCase()
        {
            return new List<Process>
            {
                new Process { Id = 1, ArrivalTime = 0, BurstTime = 7, Priority = 2, RemainingTime = 7 },
                new Process { Id = 2, ArrivalTime = 2, BurstTime = 4, Priority = 1, RemainingTime = 4 },
                new Process { Id = 3, ArrivalTime = 4, BurstTime = 1, Priority = 3, RemainingTime = 1 },
                new Process { Id = 4, ArrivalTime = 5, BurstTime = 4, Priority = 2, RemainingTime = 4 },
                new Process { Id = 5, ArrivalTime = 8, BurstTime = 2, Priority = 1, RemainingTime = 2 }
            };
        }
        
        
        public static List<Process> ShortProcessesTestCase()
        {
            List<Process> processes = new List<Process>();
            
            for (int i = 1; i <= 10; i++)
            {
                var process = new Process
                {
                    Id = i,
                    ArrivalTime = i - 1,
                    BurstTime = random.Next(1, 3), // Short burst times (1-2)
                    Priority = random.Next(1, 5),
                };
                process.RemainingTime = process.BurstTime;
                processes.Add(process);
            }
            
            return processes;
        }
        
        // Scenario with long processes
        public static List<Process> LongProcessesTestCase()
        {
            List<Process> processes = new List<Process>();
            
            for (int i = 1; i <= 5; i++) 
            {
                var process = new Process
                {
                    Id = i,
                    ArrivalTime = i * 2,
                    BurstTime = random.Next(10, 20), // Long burst times (10-19)
                    Priority = random.Next(1, 5),
                };
                process.RemainingTime = process.BurstTime;
                processes.Add(process);
            }
            
            return processes;
        }
        
        // Scenario with varying burst times
        public static List<Process> MixedProcessesTestCase()
        {
            List<Process> processes = new List<Process>();
            
            for (int i = 1; i <= 8; i++)
            {
                int burstTime;
                
                
                if (i <= 3)
                    burstTime = random.Next(1, 4); // Short
                else if (i <= 6)
                    burstTime = random.Next(5, 10); // Medium
                else
                    burstTime = random.Next(11, 15); // Long
                
                var process = new Process
                {
                    Id = i,
                    ArrivalTime = random.Next(0, 10),
                    BurstTime = burstTime,
                    Priority = random.Next(1, 5),
                };
                process.RemainingTime = process.BurstTime;
                processes.Add(process);
            }
            
            // Sort by arrival time for clarity
            return processes.OrderBy(p => p.ArrivalTime).ToList();
        }
        
        // Scenario with processes arriving simultaneously
        public static List<Process> SimultaneousArrivalTestCase()
        {
            List<Process> processes = new List<Process>();
            
            for (int i = 1; i <= 5; i++)
            {
                var process = new Process
                {
                    Id = i,
                    ArrivalTime = 0, 
                    BurstTime = random.Next(1, 10),
                    Priority = random.Next(1, 5),
                };
                process.RemainingTime = process.BurstTime;
                processes.Add(process);
            }
            
            return processes;
        }
        
        // Scenario with high priority variation
        public static List<Process> PriorityVariationTestCase()
        {
            List<Process> processes = new List<Process>();
            
            for (int i = 1; i <= 8; i++)
            {
                var process = new Process
                {
                    Id = i,
                    ArrivalTime = i,
                    BurstTime = random.Next(3, 8),
                    Priority = i, 
                };
                process.RemainingTime = process.BurstTime;
                processes.Add(process);
            }
            
            return processes;
        }
    }
}