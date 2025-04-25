using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUSchedulingSimulator
{
    
    public class RoundRobin : ISchedulingAlgorithm
    {
        private readonly int _quantum;

        public RoundRobin(int quantum)
        {
            _quantum = quantum;
        }

        public string Name => $"Round Robin (q={_quantum})";

        public SchedulingResult Execute(List<Process> processes)
        {
            var result = new SchedulingResult
            {
                AlgorithmName = Name,
                ExecutionTimeline = new List<ExecutionEvent>(),
                CompletedProcesses = new List<Process>()
            };

            // Create a copy of the processes to manipulate
            var processList = processes.ToList();
            
            // Ready queue for processes
            Queue<Process> readyQueue = new Queue<Process>();
            
            int currentTime = 0;
            int totalIdleTime = 0;
            int completedCount = 0;
            
            // Sort processes by arrival time
            processList = processList.OrderBy(p => p.ArrivalTime).ToList();
            
            // Continue until all processes are completed
            while (completedCount < processes.Count)
            {
                // Check for newly arrived processes
                var newArrivals = processList
                    .Where(p => p.ArrivalTime <= currentTime && !p.IsCompleted && !readyQueue.Contains(p))
                    .ToList();
                
                foreach (var process in newArrivals)
                {
                    readyQueue.Enqueue(process);
                }
                
                // If ready queue is empty, move time to next arrival
                if (readyQueue.Count == 0)
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
                        continue;
                    }
                    else
                    {
                        break; // No more processes to execute
                    }
                }
                
                // Get the next process from ready queue
                var currentProcess = readyQueue.Dequeue();
                
                // Set response time if first execution
                if (currentProcess.ResponseTime == -1)
                {
                    currentProcess.ResponseTime = currentTime - currentProcess.ArrivalTime;
                }
                
                // Calculate execution time for this quantum
                int execTime = Math.Min(_quantum, currentProcess.RemainingTime);
                
                // Record execution
                result.ExecutionTimeline.Add(new ExecutionEvent
                {
                    ProcessId = currentProcess.Id,
                    StartTime = currentTime,
                    EndTime = currentTime + execTime
                });
                
                // Update remaining time
                currentProcess.RemainingTime -= execTime;
                currentTime += execTime;
                
                // Check for completion
                if (currentProcess.IsCompleted)
                {
                    currentProcess.CompletionTime = currentTime;
                    currentProcess.TurnaroundTime = currentProcess.CompletionTime - currentProcess.ArrivalTime;
                    currentProcess.WaitingTime = currentProcess.TurnaroundTime - currentProcess.BurstTime;
                    
                    result.CompletedProcesses.Add(currentProcess);
                    completedCount++;
                }
                else
                {
                    // Check for any new arrivals before re-queuing
                    newArrivals = processList
                        .Where(p => p.ArrivalTime <= currentTime && !p.IsCompleted && 
                               !readyQueue.Contains(p) && p.Id != currentProcess.Id)
                        .ToList();
                    
                    foreach (var process in newArrivals)
                    {
                        readyQueue.Enqueue(process);
                    }
                    
                    // Put back in queue
                    readyQueue.Enqueue(currentProcess);
                }
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

    // Priority Scheduling Algorithm (Non-preemptive)
    public class PriorityScheduling : ISchedulingAlgorithm
    {
        public string Name => "Priority Scheduling";

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

            // Continue until all processes are completed
            while (completedCount < processes.Count)
            {
                // Find all arrived processes that are not completed
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

                // Finding the process with highest priority 
                var selectedProcess = availableProcesses
                    .OrderBy(p => p.Priority)
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