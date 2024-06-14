using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FmiWrapper_Net;
using fmuSimulator.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace fmuSimulator.Views;

public partial class MainWindow :  Window
{
    string modelIdentifier = "WP4_dec23_Components_BaseClasses_C0R0_kW_alpha0125_dt_cosim_me";
    string guid = "{2471d66b-0e1d-4bb8-bda7-b6dc9b4bde45}";
   
    // Start Value Reference
    uint server_temp = 335544321;
    uint cooling_demand = 352321536;
    uint server_temp_in = 16777219;
    uint server_temp_out = 335544320;
    uint total_cooling = 352321537;
    
    public MainWindow()
    {
        InitializeComponent();
    }
    
    public void OnSimulation(object source, RoutedEventArgs args)
    {
        SimulateFMU();
        // Debug.WriteLine($"Click! Celsius={celsius.Text}");
        // if (Double.TryParse(celsius.Text, out double C))
        // {
        //     var F = C * (9d / 5d) + 32;
        //     fahrenheit.Text = F.ToString("0.0");
        // }
        // else
        // {
        //     celsius.Text = "0";
        //     fahrenheit.Text = "0";
        // }
    }

    private void SimulateFMU()
    {
            using (var fmu = new FmuInstance(modelIdentifier + ".dll"))
            {
                // Initialize
                if (Double.TryParse(StopTime.Text, out double stopTime) && 
                    Double.TryParse(Interval.Text, out double interval) && 
                    Double.TryParse(CoolingDemand.Text, out double coolingDemand) && 
                    Double.TryParse(TotalCooling.Text, out double totalCooling) &&
                    Double.TryParse(ServerTemperature.Text, out double serverTemperature) && 
                    Double.TryParse(ServerTemperatureInput.Text, out double serverTemperatureInput) )
                {
                    // Setup
                    fmu.Log += Fmu_Log;
                    fmu.StepFinished += Fmu_StepFinished;
                
                    // Instantiate (Resource Path must start with "file:")
                    fmu.Instantiate(modelIdentifier + "_Instance", 
                        Fmi2Type.fmi2CoSimulation, 
                        guid, 
                        "",
                        false, 
                        loggingOn : true);
                    Debug.WriteLine("Types platform: " + fmu.GetTypesPlatform());
                    Debug.WriteLine("Version: " + fmu.GetVersion());
                    
                    
                    // set start value
                    fmu.SetReal( new uint[]{ cooling_demand }, new double []{ coolingDemand }); 
                    fmu.SetReal( new uint[]{ total_cooling }, new double []{ totalCooling });
                    fmu.SetReal( new uint[]{ server_temp_in }, new double []{ serverTemperatureInput });
                    fmu.SetReal( new uint[]{ server_temp }, new double []{ serverTemperature });
                    double startTime = 0;
                    
                    // initialize
                    fmu.SetupExperiment(false, 0, startTime, true,  stopTime);
                    fmu.EnterInitializationMode();
                    fmu.ExitInitializationMode();
                    fmu.SetTime(startTime);
                
                    // Simulate
                    double time = startTime;
                    double[] server_temp_out_Val = new double[1];
                    double[] server_temp_in_Val = new double[1];
                    List<double> result = new List<double>();
                    
                    while (time < stopTime)
                    {
                        Fmi2Status status = fmu.DoStep(time, interval, false);

                        if (status == Fmi2Status.fmi2OK)
                        {
                            fmu.GetReal(new uint[]{ server_temp_out }, server_temp_out_Val);
                            server_temp_in_Val[0] = server_temp_out_Val[0];
                            fmu.SetReal(new uint[]{ server_temp_in }, server_temp_in_Val);
                            result.Add(server_temp_out_Val[0]);
                        }
                        else
                        {
                            Debug.WriteLine("Error: " + status);
                        }

                        Debug.WriteLine("time: " + time + " server_temp_out: " + server_temp_out_Val[0]);
                        time += interval;
                    }
                    
                    fmu.Terminate();
                    
                    var viewModel = this.DataContext as MainWindowViewModel;
                    if (viewModel != null)
                    {
                        // Set the Series property with a new array of ISeries
                        viewModel.Series = new ISeries[]
                        {
                            new LineSeries<double>
                            {
                                Values = result,
                                GeometrySize = 0.1,
                            }
                        };
                    }
                }
                
                
            }
    }
    
    private static void Fmu_Log(string instanceName, Fmi2Status status, string category, string message)
    {
        Debug.WriteLine("Instance name: " + instanceName + ", status: " + status.ToString("g") + ", category: " + category + ", message: " + message);
    }

    private static void Fmu_StepFinished(Fmi2Status status)
    {
        Debug.WriteLine("Step finished " + status.ToString("g"));
    }
    
    
}