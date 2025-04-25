using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUSchedulingSimulator
{
    
    public class FirstComeFirstServe : ISchedulingAlgorithm
    {
        public string Name => "First Come First Serve";

        public SchedulingResult Execute(List<Process> processes)
        {
            var result = new SchedulingResult
            {
                AlgorithmName = Name,
                ExecutionTimeline = new List<ExecutionEvent>(),
                CompletedProcesses = new List<Process>()
            };

            
            var sortedProcesses = processes.OrderBy(p => p.ArrivalTime).ToList();
            
            int currentTime = 0;
            int totalIdleTime = 0;
            
            foreach (var process in sortedProcesses)
            {
                
                if (process.ArrivalTime > currentTime)
                {
                    result.ExecutionTimeline.Add(new ExecutionEvent
                    {
                        ProcessId = -1, 
                        StartTime = currentTime,
                        EndTime = process.ArrivalTime
                    });
                    
                    totalIdleTime += (process.ArrivalTime - currentTime);
                    currentTime = process.ArrivalTime;
                }
                
               
                process.ResponseTime = currentTime - process.ArrivalTime;
                
                // Execute the process
                result.ExecutionTimeline.Add(new ExecutionEvent
                {
                    ProcessId = process.Id,
                    StartTime = currentTime,
                    EndTime = currentTime + process.BurstTime
                });
                
               
                process.CompletionTime = currentTime + process.BurstTime;
                process.TurnaroundTime = process.CompletionTime - process.ArrivalTime;
                process.WaitingTime = process.TurnaroundTime - process.BurstTime;
                process.RemainingTime = 0; 
                
                
                currentTime += process.BurstTime;
                
                // Add to completed processes
                result.CompletedProcesses.Add(process);
            }

            
            int totalWaitingTime = result.CompletedProcesses.Sum(p => p.WaitingTime);
            int totalTurnaroundTime = result.CompletedProcesses.Sum(p => p.TurnaroundTime);
            int totalResponseTime = result.CompletedProcesses.Sum(p => p.ResponseTime);
            
            result.AverageWaitingTime = (double)totalWaitingTime / processes.Count;
            result.AverageTurnaroundTime = (double)totalTurnaroundTime / processes.Count;
            result.AverageResponseTime = (double)totalResponseTime / processes.Count;
            
            
            int totalTime = currentTime;
            result.CPUUtilization = ((double)(totalTime - totalIdleTime) / totalTime) * 100;
            result.Throughput = (double)processes.Count / totalTime;

            return result;
        }
    }

    
    public class ShortestJobFirst : ISchedulingAlgorithm
    {
        public string Name => "Shortest Job First";

        public SchedulingResult Execute(List<Process> processes)
        {
            var result = new SchedulingResult
            {
                AlgorithmName = Name,
                ExecutionTimeline = new List<ExecutionEvent>(),
                CompletedProcesses = new List<Process>()
            };

            // Sort processes by arrival time initially
            var sortedProcesses = processes.OrderBy(p => p.ArrivalTime).ToList();
            
            int currentTime = 0;
            int totalIdleTime = 0;
            int completedCount = 0;

            
            while (completedCount < processes.Count)
            {
                
                var availableProcesses = sortedProcesses
                    .Where(p => p.ArrivalTime <= currentTime && !p.IsCompleted)
                    .ToList();

                
                if (availableProcesses.Count == 0)
                {
                    var nextArrival = sortedProcesses
                        .Where(p => p.ArrivalTime > currentTime && !p.IsCompleted)
                        .OrderBy(p => p.ArrivalTime)
                        .FirstOrDefault();

                    if (nextArrival != null)
                    {
                        result.ExecutionTimeline.Add(new ExecutionEvent
                        {
                            ProcessId = -1, // Idle
                            StartTime = currentTime,
                            EndTime = nextArrival.ArrivalTime
                        });
                        
                        totalIdleTime += (nextArrival.ArrivalTime - currentTime);
                        currentTime = nextArrival.ArrivalTime;
                    }
                    continue;
                }

                
                var selectedProcess = availableProcesses
                    .OrderBy(p => p.BurstTime)
                    .First();

                
                if (selectedProcess.ResponseTime == -1)
                {
                    selectedProcess.ResponseTime = currentTime - selectedProcess.ArrivalTime;
                }

                
                result.ExecutionTimeline.Add(new ExecutionEvent
                {
                    ProcessId = selectedProcess.Id,
                    StartTime = currentTime,
                    EndTime = currentTime + selectedProcess.BurstTime
                });

                
                selectedProcess.CompletionTime = currentTime + selectedProcess.BurstTime;
                selectedProcess.TurnaroundTime = selectedProcess.CompletionTime - selectedProcess.ArrivalTime;
                selectedProcess.WaitingTime = selectedProcess.TurnaroundTime - selectedProcess.BurstTime;
                selectedProcess.RemainingTime = 0; 
                
                
                currentTime += selectedProcess.BurstTime;
                
                
                result.CompletedProcesses.Add(selectedProcess);
                completedCount++;
            }

            
            int totalWaitingTime = result.CompletedProcesses.Sum(p => p.WaitingTime);
            int totalTurnaroundTime = result.CompletedProcesses.Sum(p => p.TurnaroundTime);
            int totalResponseTime = result.CompletedProcesses.Sum(p => p.ResponseTime);
            
            result.AverageWaitingTime = (double)totalWaitingTime / processes.Count;
            result.AverageTurnaroundTime = (double)totalTurnaroundTime / processes.Count;
            result.AverageResponseTime = (double)totalResponseTime / processes.Count;
            
            
            int totalTime = currentTime;
            result.CPUUtilization = ((double)(totalTime - totalIdleTime) / totalTime) * 100;
            result.Throughput = (double)processes.Count / totalTime;

            return result;
        }
    }
}