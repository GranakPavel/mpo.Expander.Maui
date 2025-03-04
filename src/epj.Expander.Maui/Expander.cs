using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;
namespace epj.Expander.Maui;

#pragma warning disable CA1001
[ContentProperty(nameof(BodyContent))]
public class Expander : ContentView
{
    private static bool _animationsEnabled;

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private readonly double _fadeToOpacity = 0.6D;
    private readonly uint _fadeToExecutionTime = 200;

    private Grid HeaderGrid { get; } 
    private Grid BodyGrid { get; }
    private Border MyHeaderBorder { get; set; } 
    private Label MyHeaderText { get; set; }
    private Label MyHeaderIcon { get; }

    //propertyChanged: OnHeaderTextPropertyChanged
    public static readonly BindableProperty HeaderTextProperty = BindableProperty.Create(nameof(HeaderText), typeof(string), typeof(Expander), string.Empty, BindingMode.TwoWay);
    public static readonly BindableProperty HeaderBorderStyleProperty = BindableProperty.CreateAttached(nameof(HeaderBorderStyle), typeof(Style), typeof(Border), default(Style), BindingMode.OneWay);
    public static readonly BindableProperty HeaderTextStyleProperty = BindableProperty.CreateAttached(nameof(HeaderTextStyle), typeof(Style), typeof(Label), default(Style), BindingMode.OneWay);
    public static readonly BindableProperty HeaderIconStyleProperty = BindableProperty.CreateAttached(nameof(HeaderIconStyle), typeof(Style), typeof(Label), default(Style), BindingMode.OneWay);
    public static readonly BindableProperty HeaderIconDefaultRotationAngleProperty = BindableProperty.CreateAttached(nameof(HeaderIconDefaultRotationAngle), typeof(Double), typeof(Expander), 0D, BindingMode.TwoWay);
    public static readonly BindableProperty HeaderIconRotateAngleProperty = BindableProperty.CreateAttached(nameof(HeaderIconRotateAngle), typeof(Double), typeof(Expander), -90D, BindingMode.TwoWay);


    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(Expander), propertyChanged: OnIsExpandedPropertyChanged);
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Expander));
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(Expander));
    public static readonly BindableProperty HeaderContentProperty = BindableProperty.Create(nameof(HeaderContent), typeof(View), typeof(Expander), propertyChanged: OnHeaderContentPropertyChanged);
    public static readonly BindableProperty BodyContentProperty = BindableProperty.Create(nameof(BodyContent), typeof(View), typeof(Expander), propertyChanged: OnContentPropertyChanged);
    public static readonly BindableProperty ExpandEasingProperty = BindableProperty.Create(nameof(ExpandEasing), typeof(Easing), typeof(Expander), defaultValue: Easing.CubicIn);
    public static readonly BindableProperty CollapseEasingProperty = BindableProperty.Create(nameof(CollapseEasing), typeof(Easing), typeof(Expander), defaultValue: Easing.CubicOut);
    public static readonly BindableProperty ExpandDurationProperty = BindableProperty.Create(nameof(ExpandDuration), typeof(int), typeof(Expander), defaultValue: 250, propertyChanged: OnExpandDurationPropertyChanged);
    public static readonly BindableProperty CollapseDurationProperty = BindableProperty.Create(nameof(CollapseDuration), typeof(int), typeof(Expander), defaultValue: 250, propertyChanged: OnCollapseDurationPropertyChanged);
       
    /// <summary>
    /// Use to set the header content of the expander
    /// </summary>
    public View HeaderContent
    {
        get => (View)GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    /// <summary>
    /// Use to set the body content of the expander
    /// </summary>
    public View BodyContent
    {
        get => (View)GetValue(BodyContentProperty);
        set => SetValue(BodyContentProperty, value);
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
            {
                return;
            }
            _isExpanded = value;
            OnIsExpandedChanged();
        }
    }

    /// <summary>
    /// Animations are experimental at the moment. You need to call Expander.EnableAnimations() first before setting this to true.
    /// </summary>
    public bool Animated { get; set; }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public Easing ExpandEasing
    {
        get => (Easing)GetValue(ExpandEasingProperty);
        set => SetValue(ExpandEasingProperty, value);
    }

    public Easing CollapseEasing
    {
        get => (Easing)GetValue(CollapseEasingProperty);
        set => SetValue(CollapseEasingProperty, value);
    }

    private uint _expandDuration = 250;
    public uint ExpandDuration
    {
        get => _expandDuration;
        set
        {
            if (_expandDuration == value)
            {
                return;
            }
            _expandDuration = value;
            OnPropertyChanged();
        }
    }

    private uint _collapseDuration = 250;
    public uint CollapseDuration
    {
        get => _collapseDuration;
        set
        {
            if (_collapseDuration == value)
            {
                return;
            }
            _collapseDuration = value;
            OnPropertyChanged();
        }
    }

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public Style HeaderBorderStyle
    {
        get => (Style)GetValue(HeaderBorderStyleProperty);
        set => SetValue(HeaderBorderStyleProperty, value);
    }

    public Style HeaderTextStyle
    {
        get => (Style)GetValue(HeaderTextStyleProperty);
        set => SetValue(HeaderTextStyleProperty, value);
    }
    public Style HeaderIconStyle
    {
        get => (Style)GetValue(HeaderIconStyleProperty);
        set => SetValue(HeaderIconStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the header icon default image rotation angle.
    /// </summary>
    /// <value>
    /// 0D
    /// </value>
    public Double HeaderIconDefaultRotationAngle
    { 
        get => (Double)GetValue(HeaderIconDefaultRotationAngleProperty);
        set => SetValue(HeaderIconDefaultRotationAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the header icon rotate image angle.
    /// </summary>
    /// <value>
    /// -90D
    /// </value>
    public Double HeaderIconRotateAngle
    {
        get => (Double)GetValue(HeaderIconRotateAngleProperty);
        set => SetValue(HeaderIconRotateAngleProperty, value);
    }

    private static void OnHeaderTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((Label)bindable).Text = (string)newValue;
    }

    private static void OnIsExpandedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((Expander)bindable).IsExpanded = (bool)newValue;
    }

    private static void OnExpandDurationPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var expandDuration = (int)newValue;

        if (expandDuration <= 0)
        {
            var message = $"Value for {nameof(ExpandDuration)} must be larger than 0";
            throw new ArgumentOutOfRangeException(message);
        }

        ((Expander)bindable).ExpandDuration = (uint)expandDuration;
    }

    private static void OnCollapseDurationPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var collapseDuration = (int)newValue;

        if (collapseDuration <= 0)
        {
            var message = $"Value for {nameof(CollapseDuration)} must be larger than 0";
            throw new ArgumentOutOfRangeException(message);
        }

        ((Expander)bindable).CollapseDuration = (uint)collapseDuration;
    }

    private static void OnHeaderContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var expander = (Expander)bindable;

        if (expander.HeaderGrid is not { } grid || newValue is not View view)
        {
            return;
        }

        if (oldValue is View oldView)
        {
            grid.Remove(oldView);
        }

        grid.Add(view);
    }

    private static void OnContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var expander = (Expander)bindable;

        if (expander.BodyGrid is not { } grid || newValue is not View view)
        {
            return;
        }

        if (oldValue is View oldView)
        {
            grid.Remove(oldView);
        }

        grid.Add(view);
    }

    public event EventHandler<ExpandedEventArgs> IsExpandedChanged;
    public event EventHandler<ExpandedEventArgs> HeaderTapped;

    public Expander()
    {
        MyHeaderBorder = new Border()
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
        };

        MyHeaderText = new Label
        {
            Text = "Default text:"
        };

        MyHeaderIcon = new Label
        { 
            Rotation = HeaderIconDefaultRotationAngle 
        };

        HeaderGrid = new Grid
        {
            ZIndex = 500,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            RowDefinitions = { new RowDefinition { Height = GridLength.Star } },
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = new GridLength(30) } },
        };

        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += OnHeaderContentTapped;
        HeaderGrid.GestureRecognizers.Add(tapGestureRecognizer);
        MyHeaderBorder.GestureRecognizers.Add(tapGestureRecognizer);
        MyHeaderText.GestureRecognizers.Add(tapGestureRecognizer);
        MyHeaderIcon.GestureRecognizers.Add(tapGestureRecognizer);

        BodyGrid = new Grid
        {
            ZIndex = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Start,
            IsClippedToBounds = true
        };

        MyHeaderBorder.SetBinding(Border.StyleProperty, new Binding(nameof(HeaderBorderStyle), source: this));
        BodyGrid.SetBinding(Grid.IsVisibleProperty, new Binding(nameof(IsExpanded), source: this));
        MyHeaderText.SetBinding(Label.TextProperty, new Binding(nameof(HeaderText),  source: this));
        MyHeaderText.SetBinding(Label.StyleProperty, new Binding(nameof(HeaderTextStyle), source: this));
        MyHeaderIcon.SetBinding(Label.StyleProperty, new Binding(nameof(HeaderIconStyle), source: this));

        var expanderGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition(GridLength.Star),
            }
        };

        HeaderGrid.Add(MyHeaderText, 0,0);
        HeaderGrid.Add(MyHeaderIcon, 1, 0);
        MyHeaderBorder.Content = HeaderGrid;

        expanderGrid.Add(MyHeaderBorder, 0,0);
        expanderGrid.Add(BodyGrid,0,1);

        Content = expanderGrid;
    }

    private void OnHeaderContentTapped(object sender, TappedEventArgs e)
    {
        IsExpanded = !IsExpanded;
        HeaderTapped?.Invoke(this, new ExpandedEventArgs { Expanded = IsExpanded });
        Command?.Execute(CommandParameter);
    }

    private async void OnIsExpandedChanged()
    {
        var notified = false;

        try
        {
            if (!await _semaphoreSlim.WaitAsync((int)Math.Max(ExpandDuration, CollapseDuration) + 50))
            {
                return;
            }

            if (!CanAnimate() || BodyContent == null)
            {
                return;
            }

            var originalHeightRequest = BodyContent.HeightRequest;

            var size = BodyContent.Measure(double.PositiveInfinity, double.PositiveInfinity);

            if (IsExpanded)
            {
                BodyContent.HeightRequest = 0;
                BodyContent.TranslationY = -size.Minimum.Height;

                OnPropertyChanged(nameof(IsExpanded));
                notified = true;

                var animation = new Animation()
                    .Add(BodyContent.AnimateHeightRequest(start: 0, end: size.Minimum.Height, easing: ExpandEasing))
                    .Add(BodyContent.AnimateTranslationY(start: -size.Minimum.Height, end: 0, easing: ExpandEasing));

                MyHeaderIcon.Rotation = HeaderIconRotateAngle;

                //await BodyContent.AnimateAsync(animation, ExpandDuration);



                var a1 = BodyContent.AnimateAsync(animation, ExpandDuration);

                //var a2 = new Animation(v => MyHeaderBorder.FadeTo(_fadeToOpacity, _fadeToExecutionTime, Easing.SinOut));
                //var a3 = new Animation(v => MyHeaderBorder.FadeTo(1, _fadeToExecutionTime, Easing.SinOut));

                var a = new Animation()
                    .Add(new Animation(v => MyHeaderBorder.FadeTo(_fadeToOpacity, _fadeToExecutionTime, Easing.SinOut)))
                    .Add(new Animation(v => MyHeaderBorder.FadeTo(1, _fadeToExecutionTime, Easing.SinOut)));
                
                //var tcs = new TaskCompletionSource();
                //a.Commit(MyHeaderBorder, "E", 250, finished: (v, c) => { tcs.SetResult(); });
                await BodyContent.AnimateAsync(animation, ExpandDuration);

                //await Task.WhenAll( a1); 

                //await MyHeaderBorder.FadeTo(1, _fadeToExecutionTime, Easing.SinOut);

            }
            else
            {
                var animation = new Animation()
                    .Add(BodyContent.AnimateHeightRequest(start: size.Minimum.Height, end: 0, easing: CollapseEasing))
                    .Add(BodyContent.AnimateTranslationY(start: 0, end: -size.Minimum.Height, easing: CollapseEasing));

                MyHeaderIcon.Rotation = HeaderIconDefaultRotationAngle;

                // await BodyContent.AnimateAsync(animation, CollapseDuration);

                //var a1 = BodyContent.AnimateAsync(animation, CollapseDuration);
                //var a2 = MyHeaderBorder.FadeTo(_fadeToOpacity, _fadeToExecutionTime, Easing.SinOut);
                //await Task.WhenAny(a1, a2); ;
                //await MyHeaderBorder.FadeTo(1, _fadeToExecutionTime, Easing.SinOut);

                var a = new Animation()
                    .Add(new Animation(v => MyHeaderBorder.FadeTo(_fadeToOpacity, _fadeToExecutionTime, Easing.SinOut)))
                    .Add(new Animation(v => MyHeaderBorder.FadeTo(1, _fadeToExecutionTime, Easing.SinOut)));

                //var tcs = new TaskCompletionSource();
                //a.Commit(MyHeaderBorder, "E", 250, finished: (v, c) => { tcs.SetResult(); });
                await BodyContent.AnimateAsync(animation, CollapseDuration);


                OnPropertyChanged(nameof(IsExpanded));
                notified = true;
            }

            BodyContent.HeightRequest = originalHeightRequest;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            _semaphoreSlim.Release();

            if (!notified)
            {
                OnPropertyChanged(nameof(IsExpanded));
            }

            IsExpandedChanged?.Invoke(this, new ExpandedEventArgs { Expanded = IsExpanded });
        }
    }

    private bool CanAnimate() => _animationsEnabled && Animated;

    /// <summary>
    /// Animations are experimental at the moment, call this once in App.xaml.cs or MauiProgram.cs to enable animation. 
    /// </summary>
    /// <param name="enable"></param>
    public static void EnableAnimations(bool enable = true) => _animationsEnabled = enable;
}
#pragma warning restore CA1001
