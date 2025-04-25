using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUSchedulingSimulator
{
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CPU Scheduling Simulator");
            Console.WriteLine("========================");

            
            List<ISchedulingAlgorithm> algorithms = new List<ISchedulingAlgorithm>
            {
                new FirstComeFirstServe(),
                new ShortestJobFirst(),
                new RoundRobin(quantum: 2),
                new PriorityScheduling(),
                new ShortestRemainingTimeFirst(),
                new HighestResponseRatioNext()
            };

            // Defining` different test scenarios
            var testScenarios = new Dictionary<string, Func<List<Process>>>
            {
                { "Standard Case", () => TestGenerator.StandardTestCase() },
                { "Short Processes", () => TestGenerator.ShortProcessesTestCase() },
                { "Long Processes", () => TestGenerator.LongProcessesTestCase() },
                { "Mixed Processes", () => TestGenerator.MixedProcessesTestCase() },
                { "Simultaneous Arrival", () => TestGenerator.SimultaneousArrivalTestCase() },
                { "Priority Variation", () => TestGenerator.PriorityVariationTestCase() }
            };

            // Collecting overall results for final comparison
            var scenarioResults = new Dictionary<string, List<SchedulingResult>>();

            
            foreach (var scenario in testScenarios)
            {
                Console.WriteLine("\n\n");
                Console.WriteLine($"TEST SCENARIO: {scenario.Key}");
                Console.WriteLine("".PadLeft(scenario.Key.Length + 14, '='));
                
                
                var processes = scenario.Value();
                
                // Displaying process details
                Console.WriteLine("\nProcess Details:");
                Console.WriteLine("ID\tArrival\tBurst\tPriority");
                foreach (var process in processes)
                {
                    Console.WriteLine($"{process.Id}\t{process.ArrivalTime}\t{process.BurstTime}\t{process.Priority}");
                }
                
                
                List<SchedulingResult> results = new List<SchedulingResult>();
                
                foreach (var algorithm in algorithms)
                {
                    
                    var processCopy = processes.Select(p => p.Clone()).ToList();
                    
                    
                    Console.WriteLine($"\nRunning {algorithm.Name} algorithm...");
                    var result = algorithm.Execute(processCopy);
                    results.Add(result);
                    
                    
                    PrintAlgorithmResults(result);
                }
                
               
                CompareAlgorithms(scenario.Key, results);
                
               
                scenarioResults[scenario.Key] = results;
            }
            
            // Overall comparison across all scenarios
            OverallComparison(scenarioResults);
            
            // Generate summary report with visualizations
            GenerateSummaryReport(scenarioResults);
        }

        static void PrintAlgorithmResults(SchedulingResult result)
        {
            Console.WriteLine($"Algorithm: {result.AlgorithmName}");
            Console.WriteLine($"Average Waiting Time: {result.AverageWaitingTime:F2}");
            Console.WriteLine($"Average Turnaround Time: {result.AverageTurnaroundTime:F2}");
            Console.WriteLine($"CPU Utilization: {result.CPUUtilization:F2}%");
            Console.WriteLine($"Throughput: {result.Throughput:F2} processes/unit time");
            
            if (result.AverageResponseTime.HasValue)
                Console.WriteLine($"Average Response Time: {result.AverageResponseTime:F2}");

            Console.WriteLine("\nExecution Timeline:");
            foreach (var execution in result.ExecutionTimeline)
            {
                string processInfo = execution.IsIdle ? "IDLE" : $"Process {execution.ProcessId}";
                Console.WriteLine($"Time {execution.StartTime}-{execution.EndTime}: {processInfo}");
            }
        }

        static void CompareAlgorithms(string scenarioName, List<SchedulingResult> results)
        {
            Console.WriteLine($"\n\nALGORITHM COMPARISON FOR {scenarioName}");
            Console.WriteLine("".PadLeft(scenarioName.Length + 23, '-'));
            
            Console.WriteLine("\nAlgorithm\t\tAWT\tATT\tCPU Util\tThroughput\tART");
            Console.WriteLine("------------------------------------------------------------------------");
            
            foreach (var result in results)
            {
                Console.WriteLine($"{result.AlgorithmName,-20}\t{result.AverageWaitingTime,5:F2}\t{result.AverageTurnaroundTime,5:F2}\t" +
                                 $"{result.CPUUtilization,5:F2}%\t{result.Throughput,5:F2}\t" +
                                 $"{(result.AverageResponseTime.HasValue ? result.AverageResponseTime.Value.ToString("F2") : "N/A")}");
            }

            // Find the best algorithm for each metric
            var bestAWT = results.OrderBy(r => r.AverageWaitingTime).First();
            var bestATT = results.OrderBy(r => r.AverageTurnaroundTime).First();
            var bestUtil = results.OrderByDescending(r => r.CPUUtilization).First();
            var bestThruput = results.OrderByDescending(r => r.Throughput).First();
            
            Console.WriteLine("\nBEST ALGORITHMS FOR THIS SCENARIO:");
            Console.WriteLine($"Best Average Waiting Time: {bestAWT.AlgorithmName} ({bestAWT.AverageWaitingTime:F2})");
            Console.WriteLine($"Best Average Turnaround Time: {bestATT.AlgorithmName} ({bestATT.AverageTurnaroundTime:F2})");
            Console.WriteLine($"Best CPU Utilization: {bestUtil.AlgorithmName} ({bestUtil.CPUUtilization:F2}%)");
            Console.WriteLine($"Best Throughput: {bestThruput.AlgorithmName} ({bestThruput.Throughput:F2} proc/unit)");
        }
        
        static void OverallComparison(Dictionary<string, List<SchedulingResult>> scenarioResults)
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("OVERALL ALGORITHM COMPARISON");
            Console.WriteLine("===========================");
            
            // Calculate average performance metrics across all scenarios
            var algorithmNames = scenarioResults.First().Value.Select(r => r.AlgorithmName).ToList();
            
            var overallStats = new Dictionary<string, (double AvgAWT, double AvgATT, double AvgUtil, double AvgThruput, double? AvgART, int Wins)>();
            
            foreach (var name in algorithmNames)
            {
                overallStats[name] = (0, 0, 0, 0, 0, 0);
            }
            
            // Track which algorithm performed best in each scenario for each metric
            var metricWins = new Dictionary<string, int>();
            foreach (var name in algorithmNames)
            {
                metricWins[name] = 0;
            }
            
            // Calculate averages
            foreach (var scenario in scenarioResults)
            {
                foreach (var result in scenario.Value)
                {
                    var stats = overallStats[result.AlgorithmName];
                    overallStats[result.AlgorithmName] = (
                        stats.AvgAWT + result.AverageWaitingTime,
                        stats.AvgATT + result.AverageTurnaroundTime,
                        stats.AvgUtil + result.CPUUtilization,
                        stats.AvgThruput + result.Throughput,
                        stats.AvgART.HasValue ? stats.AvgART + result.AverageResponseTime : result.AverageResponseTime,
                        stats.Wins
                    );
                }
                
                // Count wins per scenario
                var bestAWT = scenario.Value.OrderBy(r => r.AverageWaitingTime).First();
                var bestATT = scenario.Value.OrderBy(r => r.AverageTurnaroundTime).First();
                var bestUtil = scenario.Value.OrderByDescending(r => r.CPUUtilization).First();
                var bestThruput = scenario.Value.OrderByDescending(r => r.Throughput).First();
                
                // Increment win counters
                var statsBestAWT = overallStats[bestAWT.AlgorithmName];
                overallStats[bestAWT.AlgorithmName] = (statsBestAWT.AvgAWT, statsBestAWT.AvgATT, statsBestAWT.AvgUtil, statsBestAWT.AvgThruput, statsBestAWT.AvgART, statsBestAWT.Wins + 1);
                
                if (bestATT.AlgorithmName != bestAWT.AlgorithmName)
                {
                    var statsBestATT = overallStats[bestATT.AlgorithmName];
                    overallStats[bestATT.AlgorithmName] = (statsBestATT.AvgAWT, statsBestATT.AvgATT, statsBestATT.AvgUtil, statsBestATT.AvgThruput, statsBestATT.AvgART, statsBestATT.Wins + 1);
                }
                
                if (bestUtil.AlgorithmName != bestAWT.AlgorithmName && bestUtil.AlgorithmName != bestATT.AlgorithmName)
                {
                    var statsBestUtil = overallStats[bestUtil.AlgorithmName];
                    overallStats[bestUtil.AlgorithmName] = (statsBestUtil.AvgAWT, statsBestUtil.AvgATT, statsBestUtil.AvgUtil, statsBestUtil.AvgThruput, statsBestUtil.AvgART, statsBestUtil.Wins + 1);
                }
                
                if (bestThruput.AlgorithmName != bestAWT.AlgorithmName && bestThruput.AlgorithmName != bestATT.AlgorithmName 
                    && bestThruput.AlgorithmName != bestUtil.AlgorithmName)
                {
                    var statsBestThruput = overallStats[bestThruput.AlgorithmName];
                    overallStats[bestThruput.AlgorithmName] = (statsBestThruput.AvgAWT, statsBestThruput.AvgATT, statsBestThruput.AvgUtil, statsBestThruput.AvgThruput, statsBestThruput.AvgART, statsBestThruput.Wins + 1);
                }
            }
            
            int scenarioCount = scenarioResults.Count;
            
            // Calculate averages
            foreach (var algorithm in overallStats.Keys.ToList())
            {
                var algorithmStats = overallStats[algorithm];
                overallStats[algorithm] = (
                    algorithmStats.AvgAWT / scenarioCount,
                    algorithmStats.AvgATT / scenarioCount,
                    algorithmStats.AvgUtil / scenarioCount,
                    algorithmStats.AvgThruput / scenarioCount,
                    algorithmStats.AvgART.HasValue ? algorithmStats.AvgART / scenarioCount : null,
                    algorithmStats.Wins
                );
            }
            
            // Displaying overall results
            Console.WriteLine("\nAlgorithm\t\tAvg AWT\tAvg ATT\tAvg CPU Util\tAvg Throughput\tWins");
            Console.WriteLine("-------------------------------------------------------------------------");
            
            foreach (var entry in overallStats.OrderByDescending(e => e.Value.Wins))
            {
                var stats = entry.Value;
                Console.WriteLine($"{entry.Key,-20}\t{stats.AvgAWT,6:F2}\t{stats.AvgATT,6:F2}\t" +
                                 $"{stats.AvgUtil,8:F2}%\t{stats.AvgThruput,8:F2}\t{stats.Wins}");
            }
            
            // Finding the overall best algorithms
            var bestOverallAWT = overallStats.OrderBy(s => s.Value.AvgAWT).First();
            var bestOverallATT = overallStats.OrderBy(s => s.Value.AvgATT).First();
            var bestOverallUtil = overallStats.OrderByDescending(s => s.Value.AvgUtil).First();
            var bestOverallThruput = overallStats.OrderByDescending(s => s.Value.AvgThruput).First();
            var mostWins = overallStats.OrderByDescending(s => s.Value.Wins).First();
            
            Console.WriteLine("\nOVERALL BEST ALGORITHMS:");
            Console.WriteLine($"Best Average Waiting Time: {bestOverallAWT.Key} ({bestOverallAWT.Value.AvgAWT:F2})");
            Console.WriteLine($"Best Average Turnaround Time: {bestOverallATT.Key} ({bestOverallATT.Value.AvgATT:F2})");
            Console.WriteLine($"Best CPU Utilization: {bestOverallUtil.Key} ({bestOverallUtil.Value.AvgUtil:F2}%)");
            Console.WriteLine($"Best Throughput: {bestOverallThruput.Key} ({bestOverallThruput.Value.AvgThruput:F2})");
            Console.WriteLine($"Most Wins Across Scenarios: {mostWins.Key} ({mostWins.Value.Wins} wins)");
            
            // Providing recommendations
            Console.WriteLine("\nRECOMMENDATIONS BASED ON WORKLOAD TYPE:");
            Console.WriteLine("For workloads with many short processes: " + 
                GetBestAlgorithmForScenario(scenarioResults["Short Processes"]));
            Console.WriteLine("For workloads with few long processes: " + 
                GetBestAlgorithmForScenario(scenarioResults["Long Processes"]));
            Console.WriteLine("For workloads with mixed process lengths: " + 
                GetBestAlgorithmForScenario(scenarioResults["Mixed Processes"]));
            Console.WriteLine("For workloads where processes arrive simultaneously: " + 
                GetBestAlgorithmForScenario(scenarioResults["Simultaneous Arrival"]));
            
            Console.WriteLine("\nCONCLUSION:");
            Console.WriteLine($"Based on overall performance across different scenarios, {mostWins.Key} appears to be the most versatile scheduling algorithm.");
            
            // Highlighting specific algorithm strengths
            if (mostWins.Key != bestOverallAWT.Key)
                Console.WriteLine($"However, if minimizing waiting time is critical, {bestOverallAWT.Key} would be preferable.");
            
            if (mostWins.Key != bestOverallThruput.Key)
                Console.WriteLine($"For maximizing throughput, {bestOverallThruput.Key} shows the best results.");
        }
        
        // Helper method to determine the best algorithm for a specific scenario
        static string GetBestAlgorithmForScenario(List<SchedulingResult> results)
        {
            // Simple scoring: lower AWT and ATT are better, higher CPU util and throughput are better
            // We'll normalize each metric to a 0-1 scale and combine them
            
            double minAWT = results.Min(r => r.AverageWaitingTime);
            double maxAWT = results.Max(r => r.AverageWaitingTime);
            double awtRange = maxAWT - minAWT;
            
            double minATT = results.Min(r => r.AverageTurnaroundTime);
            double maxATT = results.Max(r => r.AverageTurnaroundTime);
            double attRange = maxATT - minATT;
            
            double minUtil = results.Min(r => r.CPUUtilization);
            double maxUtil = results.Max(r => r.CPUUtilization);
            double utilRange = maxUtil - minUtil;
            
            double minThru = results.Min(r => r.Throughput);
            double maxThru = results.Max(r => r.Throughput);
            double thruRange = maxThru - minThru;
            
            var scores = new Dictionary<string, double>();
            
            foreach (var result in results)
            {
                // Normalize each metric (0 = worst, 1 = best)
                double awtScore = awtRange > 0 ? 1 - ((result.AverageWaitingTime - minAWT) / awtRange) : 1;
                double attScore = attRange > 0 ? 1 - ((result.AverageTurnaroundTime - minATT) / attRange) : 1;
                double utilScore = utilRange > 0 ? (result.CPUUtilization - minUtil) / utilRange : 1;
                double thruScore = thruRange > 0 ? (result.Throughput - minThru) / thruRange : 1;
                
                // Combined score (equal weights)
                double combinedScore = (awtScore + attScore + utilScore + thruScore) / 4.0;
                scores[result.AlgorithmName] = combinedScore;
            }
            
            // Find algorithm with highest score
            var bestAlgorithm = scores.OrderByDescending(s => s.Value).First();
            return $"{bestAlgorithm.Key} (Score: {bestAlgorithm.Value:F2})";
        }
        
        static void GenerateSummaryReport(Dictionary<string, List<SchedulingResult>> scenarioResults)
        {
            Console.WriteLine("\nGENERATING SUMMARY REPORT");
            Console.WriteLine("=========================");
            
            // Display text-based charts for overall algorithm performance
            var algorithmNames = scenarioResults.First().Value.Select(r => r.AlgorithmName).ToList();
            var overallStats = new Dictionary<string, (double AvgAWT, double AvgATT, double AvgUtil, double AvgThroughput)>();
            
            // Initialize metrics for each algorithm
            foreach (var name in algorithmNames)
            {
                overallStats[name] = (0, 0, 0, 0);
            }
            
            // Calculate average metrics across all scenarios
            foreach (var scenario in scenarioResults)
            {
                foreach (var result in scenario.Value)
                {
                    var current = overallStats[result.AlgorithmName];
                    overallStats[result.AlgorithmName] = (
                        current.AvgAWT + result.AverageWaitingTime,
                        current.AvgATT + result.AverageTurnaroundTime,
                        current.AvgUtil + result.CPUUtilization,
                        current.AvgThroughput + result.Throughput
                    );
                }
            }
            
            // Calculate averages
            int scenarioCount = scenarioResults.Count;
            foreach (var algorithm in overallStats.Keys.ToList())
            {
                var stats = overallStats[algorithm];
                overallStats[algorithm] = (
                    stats.AvgAWT / scenarioCount,
                    stats.AvgATT / scenarioCount,
                    stats.AvgUtil / scenarioCount,
                    stats.AvgThroughput / scenarioCount
                );
            }
            
            // Displaying text-based charts for each metric
            DisplayConsoleChart(overallStats.Select(kv => new { Name = kv.Key, Value = kv.Value.AvgAWT }).ToList(), 
                item => item.Value, "Overall Average Waiting Time", true);
            
            DisplayConsoleChart(overallStats.Select(kv => new { Name = kv.Key, Value = kv.Value.AvgATT }).ToList(), 
                item => item.Value, "Overall Average Turnaround Time", true);
            
            DisplayConsoleChart(overallStats.Select(kv => new { Name = kv.Key, Value = kv.Value.AvgUtil }).ToList(), 
                item => item.Value, "Overall CPU Utilization (%)", false);
            
            DisplayConsoleChart(overallStats.Select(kv => new { Name = kv.Key, Value = kv.Value.AvgThroughput }).ToList(), 
                item => item.Value, "Overall Throughput", false);
            
            // Calculating overall score 
            var scores = new Dictionary<string, double>();
            
            // Calculating max values for normalization
            double maxAWT = overallStats.Values.Max(v => v.AvgAWT);
            double maxATT = overallStats.Values.Max(v => v.AvgATT);
            double maxUtil = overallStats.Values.Max(v => v.AvgUtil);
            double maxThroughput = overallStats.Values.Max(v => v.AvgThroughput);
            
            foreach (var algorithm in overallStats.Keys)
            {
                var stats = overallStats[algorithm];
                
                // Normalize scores (0-1 scale) where higher is always better
                double awtScore = maxAWT > 0 ? 1 - (stats.AvgAWT / maxAWT) : 1;
                double attScore = maxATT > 0 ? 1 - (stats.AvgATT / maxATT) : 1;
                double utilScore = maxUtil > 0 ? stats.AvgUtil / maxUtil : 1;
                double throughputScore = maxThroughput > 0 ? stats.AvgThroughput / maxThroughput : 1;
                
                // Combined score with equal weights
                scores[algorithm] = (awtScore + attScore + utilScore + throughputScore) / 4.0;
            }
            
            // Display overall score chart
            Console.WriteLine("\nOVERALL ALGORITHM PERFORMANCE SCORE:");
            Console.WriteLine("----------------------------------");
            
            foreach (var entry in scores.OrderByDescending(kv => kv.Value))
            {
                int barLength = (int)(entry.Value * 40); // Scale to 40 chars max
                Console.Write($"{entry.Key,-20} | ");
                Console.Write(new string('█', barLength));
                Console.WriteLine($" {entry.Value:F2}");
            }
            
            // Overall recommendation
            var bestAlgorithm = scores.OrderByDescending(kv => kv.Value).First();
            Console.WriteLine($"\nOVERALL RECOMMENDATION: {bestAlgorithm.Key} (Score: {bestAlgorithm.Value:F2})");
        }

        // Helper method for text-based charts
        static void DisplayConsoleChart<T>(List<T> items, Func<T, double> valueSelector, string title, bool lowerIsBetter)
        {
            Console.WriteLine($"\n{title}:");
            Console.WriteLine("".PadLeft(title.Length + 1, '-'));
            
            // Find max value for scaling
            double maxValue = items.Max(item => valueSelector(item));
            double scale = 40.0 / (maxValue > 0 ? maxValue : 1); 
            
            
            IEnumerable<T> sortedItems;
            if (lowerIsBetter)
                sortedItems = items.OrderBy(item => valueSelector(item));
            else
                sortedItems = items.OrderByDescending(item => valueSelector(item));
            
            // Displaying the bar chart
            foreach (var item in sortedItems)
            {
                double value = valueSelector(item);
                int barLength = (int)(value * scale);
                
                // Getting the name property using reflection
                string name = "";
                var nameProperty = item.GetType().GetProperty("Name");
                if (nameProperty != null)
                    name = nameProperty.GetValue(item)?.ToString() ?? "";
                else
                    name = item.ToString();
                
                Console.Write($"{name,-20} | ");
                Console.Write(new string('█', barLength));
                Console.WriteLine($" {value:F2}");
            }
        }
    }
}