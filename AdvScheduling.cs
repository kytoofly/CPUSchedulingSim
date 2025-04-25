using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUSchedulingSimulator
{
    
    public class ShortestRemainingTimeFirst : ISchedulingAlgorithm
    {
        public string Name => "Shortest Remaining Time First";

        public SchedulingResult Execute(List<Process> processes)
        {
            var result = new SchedulingResult
            {
                AlgorithmName = Name,
                ExecutionTimeline = new List<ExecutionEvent>(),
                CompletedProcesses = new List<Process>()
            };

            // Making a working copy of processes
            var processList = processes.ToList();
            
            int currentTime = 0;
            int totalIdleTime = 0;
            int completedCount = 0;
            int? currentProcessId = null;
            int executionStartTime = 0;

           
            while (completedCount < processes.Count)
            {
                // Finding all arrived processes that are not completed
                var availableProcesses = processList
                    .Where(p => p.ArrivalTime <= currentTime && !p.IsCompleted)
                    .ToList();

                
                if (availableProcesses.Count == 0)
                {
                    var nextArrival = processList
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

                // Finding the process with shortest remaining time
                var selectedProcess = availableProcesses
                    .OrderBy(p => p.RemainingTime)
                    .First();

                
                if (currentProcessId != selectedProcess.Id)
                {
                    // If executing a process, record the execution time
                    if (currentProcessId.HasValue && currentProcessId.Value != -1)
                    {
                        result.ExecutionTimeline.Add(new ExecutionEvent
                        {
                            ProcessId = currentProcessId.Value,
                            StartTime = executionStartTime,
                            EndTime = currentTime
                        });
                    }
                    
                    
                    if (selectedProcess.ResponseTime == -1)
                    {
                        selectedProcess.ResponseTime = currentTime - selectedProcess.ArrivalTime;
                    }
                    
                    
                    executionStartTime = currentTime;
                    currentProcessId = selectedProcess.Id;
                }
                
                // Determining how long this process will run
                int runUntil = currentTime + 1; 
                
                
                Process nextIncomingProcess = processList
                    .Where(p => p.ArrivalTime > currentTime && p.ArrivalTime < currentTime + selectedProcess.RemainingTime && !p.IsCompleted)
                    .OrderBy(p => p.ArrivalTime)
                    .FirstOrDefault();
                
                if (nextIncomingProcess != null)
                {
                    runUntil = nextIncomingProcess.ArrivalTime;
                }
                else
                {
                    
                    runUntil = currentTime + selectedProcess.RemainingTime;
                }
                
                // Updating the remaining time
                int executedTime = runUntil - currentTime;
                selectedProcess.RemainingTime -= executedTime;
                currentTime = runUntil;
                
                
                if (selectedProcess.IsCompleted)
                {
                    // Record final execution time for this process
                    result.ExecutionTimeline.Add(new ExecutionEvent
                    {
                        ProcessId = selectedProcess.Id,
                        StartTime = executionStartTime,
                        EndTime = currentTime
                    });
                    
                    
                    selectedProcess.CompletionTime = currentTime;
                    selectedProcess.TurnaroundTime = selectedProcess.CompletionTime - selectedProcess.ArrivalTime;
                    selectedProcess.WaitingTime = selectedProcess.TurnaroundTime - selectedProcess.BurstTime;
                    
                    // Add to completed processes
                    result.CompletedProcesses.Add(selectedProcess);
                    completedCount++;
                    
                    // Reset current process
                    currentProcessId = null;
                }
            }

            
            int totalWaitingTime = result.CompletedProcesses.Sum(p => p.WaitingTime);
            int totalTurnaroundTime = result.CompletedProcesses.Sum(p => p.TurnaroundTime);
            int totalResponseTime = result.CompletedProcesses.Sum(p => p.ResponseTime);
            
            result.AverageWaitingTime = (double)totalWaitingTime / processes.Count;
            result.AverageTurnaroundTime = (double)totalTurnaroundTime / processes.Count;
            result.AverageResponseTime = (double)totalResponseTime / processes.Count;
            
            // Calculating total simulation time
            int totalTime = currentTime;
            result.CPUUtilization = ((double)(totalTime - totalIdleTime) / totalTime) * 100;
            result.Throughput = (double)processes.Count / totalTime;

            return result;
        }
    }

    
    public class HighestResponseRatioNext : ISchedulingAlgorithm
    {
        public string Name => "Highest Response Ratio Next";

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
            int completedCount = 0;

        
            while (completedCount < processes.Count)
            {
                
                var availableProcesses = sortedProcesses
                    .Where(p => p.ArrivalTime <= currentTime && !p.IsCompleted)
                    .ToList();

                // If no processes available, advance time to next arrival
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

                // Calculate response ratio for each available process
                // Response Ratio = (Waiting Time + Burst Time) / Burst Time
                Process selectedProcess = null;
                double highestRatio = -1;
                
                foreach (var process in availableProcesses)
                {
                    int waitingTime = currentTime - process.ArrivalTime;
                    double responseRatio = (double)(waitingTime + process.BurstTime) / process.BurstTime;
                    
                    if (responseRatio > highestRatio)
                    {
                        highestRatio = responseRatio;
                        selectedProcess = process;
                    }
                }

                // Set response time if not set yet
                if (selectedProcess.ResponseTime == -1)
                {
                    selectedProcess.ResponseTime = currentTime - selectedProcess.ArrivalTime;
                }

                // Execute the process
                result.ExecutionTimeline.Add(new ExecutionEvent
                {
                    ProcessId = selectedProcess.Id,
                    StartTime = currentTime,
                    EndTime = currentTime + selectedProcess.BurstTime
                });

                // Update process stats
                selectedProcess.CompletionTime = currentTime + selectedProcess.BurstTime;
                selectedProcess.TurnaroundTime = selectedProcess.CompletionTime - selectedProcess.ArrivalTime;
                selectedProcess.WaitingTime = selectedProcess.TurnaroundTime - selectedProcess.BurstTime;
                selectedProcess.RemainingTime = 0; // Process is completed
                
                // Move time forward
                currentTime += selectedProcess.BurstTime;
                
                // Add to completed processes
                result.CompletedProcesses.Add(selectedProcess);
                completedCount++;
            }

            // Calculate metrics
            int totalWaitingTime = result.CompletedProcesses.Sum(p => p.WaitingTime);
            int totalTurnaroundTime = result.CompletedProcesses.Sum(p => p.TurnaroundTime);
            int totalResponseTime = result.CompletedProcesses.Sum(p => p.ResponseTime);
            
            result.AverageWaitingTime = (double)totalWaitingTime / processes.Count;
            result.AverageTurnaroundTime = (double)totalTurnaroundTime / processes.Count;
            result.AverageResponseTime = (double)totalResponseTime / processes.Count;
            
            // Calculate total simulation time
            int totalTime = currentTime;
            result.CPUUtilization = ((double)(totalTime - totalIdleTime) / totalTime) * 100;
            result.Throughput = (double)processes.Count / totalTime;

            return result;
        }
    }
}