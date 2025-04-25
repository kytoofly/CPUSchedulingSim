using System;
using System.Collections.Generic;

namespace CPUSchedulingSimulator
{
    // The process class representing a process to be scheduled
    public class Process
    {
        
        public int Id { get; set; }
        public int ArrivalTime { get; set; }
        public int BurstTime { get; set; }
        public int Priority { get; set; }
        
        
        public int WaitingTime { get; set; }
        public int TurnaroundTime { get; set; }
        public int CompletionTime { get; set; }
        public int ResponseTime { get; set; } = -1; 
        
        
        public int RemainingTime { get; set; }
        public bool IsCompleted => RemainingTime <= 0;

        // Cloning method for creating deep copies
        public Process Clone()
        {
            return new Process
            {
                Id = this.Id,
                ArrivalTime = this.ArrivalTime,
                BurstTime = this.BurstTime,
                Priority = this.Priority,
                RemainingTime = this.BurstTime,
                WaitingTime = 0,
                TurnaroundTime = 0,
                CompletionTime = 0,
                ResponseTime = -1
            };
        }
    }

    // Records an execution period for a process
    public class ExecutionEvent
    {
        public int ProcessId { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        
        
        public bool IsIdle => ProcessId == -1;
    }

    // Contains the results of a scheduling algorithm simulation
    public class SchedulingResult
    {
        public string AlgorithmName { get; set; }
        public double AverageWaitingTime { get; set; }
        public double AverageTurnaroundTime { get; set; }
        public double CPUUtilization { get; set; }
        public double Throughput { get; set; }
        public double? AverageResponseTime { get; set; }
        public List<ExecutionEvent> ExecutionTimeline { get; set; } = new List<ExecutionEvent>();
        public List<Process> CompletedProcesses { get; set; } = new List<Process>();
    }

    // Interface for all scheduling algorithms
    public interface ISchedulingAlgorithm
    {
        string Name { get; }
        SchedulingResult Execute(List<Process> processes);
    }
}