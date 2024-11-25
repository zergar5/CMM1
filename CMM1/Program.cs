using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using SharpMath.EquationsSystem.Solver;
using SharpMath.EquationsSystem.Preconditions;
using SharpMath.FiniteElement._2D;
using SharpMath.Geometry._2D;
using SharpMath.Geometry.Splitting;
using SharpMath.Splines;
using System.Globalization;
using CourseProjectSasha;
using CourseProjectSasha.Core.GridComponents;
using Element = SharpMath.FiniteElement._2D.Element;
using System;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

void ConfigureServices(IServiceCollection services)
{
    IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    services.AddScoped<GaussZeidelConfig>(provider =>
    {
        provider.GetService<IConfiguration>();
        var gaussZeidelConfig = configuration
            .GetSection("CMM1")
            .GetSection("GaussZeidel")
            .Get<GaussZeidelConfig>();

        return gaussZeidelConfig!;
    });

    services.AddSingleton(configuration);

    services.AddTransient<ISplineCreator<ISpline2D<Point>, Point, Element>, SmoothingSpline2DCreator>();
    services.AddTransient<SmoothingSpline2DCreator>();

    services.AddScoped<GaussZeidelSolver>();


    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .CreateLogger();
    services.AddLogging(loggingBuilder =>
        loggingBuilder.AddSerilog(dispose: true));
}

void RunTest()
{
    var services = new ServiceCollection();
    ConfigureServices(services);
    var provider = services.BuildServiceProvider();

    const int splineGridSplit = 4;

    var splineGrid = new GridBuilder()
        .SetXAxis(new AxisSplitParameter(
            [0, 10],
            new UniformSplitter(splineGridSplit)
        ))
        .SetYAxis(new AxisSplitParameter(
            [0, 10],
            new UniformSplitter(splineGridSplit)
        ))
        .Build();

    var splineCreator = provider.GetService<SmoothingSpline2DCreator>();
    splineCreator.Allocate(splineGrid);

    var tests = new Tests();

    var femPoints = tests.GetPoints(8);
    var inputPoints = tests.GetPoints(8);

    //var derivativesByX = tests.GetDerivativeByXValues(inputGrid.Nodes);
    //var derivativesByY = tests.GetDerivativeByYValues(inputGrid.Nodes);

    var points = new Point[101];

    for (var i = 0; i < points.Length; i++)
    {
        points[i] = new Point(i / 10d, i / 10d);
    }

    var funcValues = tests.GetFuncValues(inputPoints);

    var weights = new double[funcValues.Length];

    var random = new Random();

    for (var i = 0; i < inputPoints.Length; i++)
    {
        if (!femPoints.Any(p => Math.Abs(p.X - inputPoints[i].X) <= 1e-16 && Math.Abs(p.Y - inputPoints[i].Y) <= 1e-16))
        {
            //funcValues[i].Value *= random.NextDouble() * (2 - 1e-2) + 1e-2;
            weights[i] = 1d;
        }
        else
        {
            weights[i] = 1d;
        }
    }

    var spline = splineCreator.CreateSpline(funcValues, weights, 0);

    Console.WriteLine("FEM solution");

    var femSolution = tests.GetFuncValues(points);

    for (var i = 0; i < points.Length; i++)
    {
        var point = points[i];
        Console.WriteLine($"{point.X:F8} {point.Y:F8} {femSolution[i].Value:E8}");
    }

    Console.WriteLine("Spline solution");

    foreach (var point in points)
    {
        Console.WriteLine($"{point.X:F8} {point.Y:F8} {spline.Calculate(point):E8}");
    }

    Console.WriteLine("True solution");
    Func<Point, double, double> u = (p, t) => Math.Sin(p.X) * Math.Cos(p.Y);

    foreach (var point in points)
    {
        Console.WriteLine($"{point.X:F8} {point.Y:F8} {u(point, 1):E8}");
    }
}

RunTest();