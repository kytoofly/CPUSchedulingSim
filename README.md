# CPU Scheduling Simulator

A comprehensive C# application for simulating and comparing various CPU scheduling algorithms.

## Overview

This project implements a CPU scheduler simulator that allows for the evaluation of different scheduling algorithms based on standard performance metrics. The simulator is designed to run various test scenarios to compare how different scheduling strategies perform under different workload conditions.

## Features

- **Multiple Scheduling Algorithms**:
  - First Come First Serve (FCFS)
  - Shortest Job First (SJF)
  - Round Robin (with configurable time quantum)
  - Priority Scheduling
  - Shortest Remaining Time First (SRTF)
  - Highest Response Ratio Next (HRRN)

- **Performance Metrics**:
  - Average Waiting Time (AWT)
  - Average Turnaround Time (ATT)
  - CPU Utilization (%)
  - Throughput (Processes per unit time)
  - Response Time (RT)

- **Test Scenarios**:
  - Standard test case with predefined processes
  - Workloads with many short processes
  - Workloads with few long processes
  - Mixed workloads with varying process lengths
  - Processes arriving simultaneously
  - Processes with varying priorities

- **Visualization**:
  - Text-based visualization of performance comparisons
  - Detailed execution timelines
  - Comparative analysis across different workload types

## Project Structure

- `Process.cs` - Data model for CPU processes
- `BasicScheduling.cs` - Implementation of basic scheduling algorithms (FCFS, SJF)
- `IntermediateScheduling.cs` - Implementation of intermediate algorithms (RR, Priority)
- `AdvScheduling.cs` - Implementation of advanced algorithms (SRTF, HRRN)
- `TestGenerator.cs` - Generator for different test workload scenarios
- `Program.cs` - Main program that runs simulations and displays results

## Getting Started

### Prerequisites

- .NET SDK (version 6.0 or higher)
- Visual Studio or Visual Studio Code

### Running the Simulator

1. Clone the repository:
   ```
   git clone https://github.com/kytoofify/CPUSchedulingSim.git
   ```

2. Navigate to the project directory:
   ```
   cd CPUSchedulingSim
   ```

3. Build the project:
   ```
   dotnet build
   ```

4. Run the simulator:
   ```
   dotnet run
   ```

## Sample Output

The simulator provides detailed output for each algorithm, including:
- Average Waiting Time
- Average Turnaround Time
- CPU Utilization
- Throughput
- Process execution timeline

It also generates overall comparisons and recommendations based on different workload characteristics.

## Extending the Simulator

You can extend the simulator by:
1. Adding new scheduling algorithms by implementing the `ISchedulingAlgorithm` interface
2. Creating new test scenarios in the `TestGenerator` class
3. Modifying the performance metrics calculation in the `Execute` methods

## Use Cases

- Educational tool for teaching CPU scheduling concepts
- Performance analysis of scheduling algorithms
- Exploring the trade-offs between different scheduling strategies
- Understanding how workload characteristics affect scheduling performance

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Based on operating systems scheduling concepts
- Inspired by standard CPU scheduling algorithms

## Contact

For questions or contributions, please open an issue on GitHub.
