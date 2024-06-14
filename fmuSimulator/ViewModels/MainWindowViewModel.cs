namespace fmuSimulator.ViewModels;
using SkiaSharp;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using System.ComponentModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia, Hi!";
#pragma warning restore CA1822 // Mark members as static
    
    private ISeries[] _series;
    
    public ISeries[] Series
    {
        get => _series;
        set
        {
            _series = value;
            OnPropertyChanged(nameof(Series));
        }
    }
    
    public MainWindowViewModel()
    {
        // Initialize with default values or load data here
        Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = new double[] { },
                GeometrySize = 0.1,
            }
        };
    }
    
    public Axis[] XAxes { get; set; }
        = new Axis[]
        {
            new Axis
            {
                Name = "Time",
                NamePaint = new SolidColorPaint(SKColors.White), 

                LabelsPaint = new SolidColorPaint(SKColors.White), 
                TextSize = 10,
                // SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }  
            }
        };
    
    public Axis[] YAxes { get; set; }
        = new Axis[]
        {
            new Axis
            {
                Name = "Temperature",
                NamePaint = new SolidColorPaint(SKColors.White), 
                LabelsPaint = new SolidColorPaint(SKColors.White),
                
                TextSize = 12,

                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
                { 
                    StrokeThickness = 2, 
                    PathEffect = new DashEffect(new float[] { 3, 3 }) 
                } 
            }
        };
    
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
}