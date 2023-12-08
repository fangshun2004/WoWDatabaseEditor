using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaStyles.Controls.FastTableView;

namespace AvaloniaStyles.Controls.OptimizedVeryFastTableView;

public abstract class BaseVirtualizedTableController : IVirtualizedTableController
{
    private static IPen ModifiedCellPen = new Pen(Brushes.Red, 1);
    private static IPen PhantomRowPen = new Pen(Brushes.Orange, 1);
    private static IPen ButtonTextPen = new Pen(new SolidColorBrush(Color.FromRgb(30, 30, 30)), 1);
    private static IPen ButtonBorderPen = new Pen(new SolidColorBrush(Color.FromRgb(245, 245, 245)), 1);
    private static IPen ButtonBackgroundPen = new Pen(new SolidColorBrush(Colors.White), 0);
    private static IPen ButtonBackgroundHoverPen = new Pen(new SolidColorBrush(Color.FromRgb(245, 245, 245)), 0);
    private static IPen ButtonBackgroundPressedPen = new Pen(new SolidColorBrush(Color.FromRgb(215, 215, 215)), 0);
    private static IPen ButtonBackgroundDisabledPen = new Pen(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 0);
    private static ISolidColorBrush TextBrush = new SolidColorBrush(Color.FromRgb(41, 41, 41));
    private static ISolidColorBrush FocusTextBrush = new SolidColorBrush(Colors.White);
    private static FontFamily MonoFont = FontFamily.Default;
    
    protected Point mouseCursor;
    protected bool leftPressed;
    
    static BaseVirtualizedTableController()
    {
        ModifiedCellPen.GetResource("FastTableView.ModifiedCellPen", ModifiedCellPen, out ModifiedCellPen);
        PhantomRowPen.GetResource("FastTableView.PhantomRowPen", PhantomRowPen, out PhantomRowPen);
        ButtonTextPen.GetResource("FastTableView.ButtonTextPen", ButtonTextPen, out ButtonTextPen);
        ButtonBorderPen.GetResource("FastTableView.ButtonBorderPen", ButtonBorderPen, out ButtonBorderPen);
        ButtonBackgroundPen.GetResource("FastTableView.ButtonBackgroundPen", ButtonBackgroundPen, out ButtonBackgroundPen);
        ButtonBackgroundHoverPen.GetResource("FastTableView.ButtonBackgroundHoverPen", ButtonBackgroundHoverPen, out ButtonBackgroundHoverPen);
        ButtonBackgroundPressedPen.GetResource("FastTableView.ButtonBackgroundPressedPen", ButtonBackgroundPressedPen, out ButtonBackgroundPressedPen);
        ButtonBackgroundDisabledPen.GetResource("FastTableView.ButtonBackgroundDisabledPen", ButtonBackgroundDisabledPen, out ButtonBackgroundDisabledPen);
        TextBrush.GetResource("FastTableView.TextBrush", TextBrush, out TextBrush);
        FocusTextBrush.GetResource("FastTableView.FocusTextBrush", FocusTextBrush, out FocusTextBrush);
        MonoFont.GetResource("MonoFont", MonoFont, out MonoFont);
    }

    public abstract bool PointerDown(int rowIndex, int cellIndex, Rect cellRect, Point pressPoint, bool leftPressed, bool rightPressed, int clickCount);

    public abstract bool PointerUp(int rowIndex, int cellIndex, Rect cellRect, Point pressPoint, bool leftPressed, bool rightPressed);

    public bool UpdateCursor(Point point, bool leftPressed)
    {
        mouseCursor = point;
        this.leftPressed = leftPressed;
        return true;
    }

    public abstract bool SpawnEditorFor(int rowIndex, int cellIndex, Rect cellRect, string? typedText, VirtualizedVeryFastTableView view);
    public abstract string? GetCellText(int rowIndex, int cellIndex);
    public abstract void DrawRow(int rowIndex, Rect rowRect, DrawingContext drawingContext, VirtualizedVeryFastTableView view);
    public abstract bool Draw(int rowIndex, int cellIndex, DrawingContext drawingContext, VirtualizedVeryFastTableView view, ref Rect rect);
    public abstract void DrawHeader(int cellIndex, DrawingContext context, VirtualizedVeryFastTableView view, ref Rect rect);

    protected void DrawText(DrawingContext context, Rect rect, string text, Color? color = null, float opacity = 0.4f, TextAlignment alignment = TextAlignment.Left)
    {
        var ft = new FormattedText
        {
            Text = text,
            Constraint = new Size(rect.Width, rect.Height),
            Typeface = new Typeface(MonoFont),//Typeface.Default,
            FontSize = 12,
            TextAlignment = alignment
        };
        var textColor = new SolidColorBrush(color ?? TextBrush.Color, opacity);
        context.DrawText(textColor, new Point(rect.X, rect.Center.Y - ft.Bounds.Height / 2), ft);
    }

    protected void DrawButton(DrawingContext context, Rect rect, string text, int margin, bool enabled = true)
    {
        rect = rect.Deflate(margin);

        bool isOver = rect.Contains(mouseCursor);
            
        context.DrawRectangle((!enabled ? ButtonBackgroundDisabledPen : isOver ? (leftPressed ? ButtonBackgroundPressedPen : ButtonBackgroundHoverPen) : ButtonBackgroundPen).Brush, ButtonBorderPen, rect, 4, 4);

        if (string.IsNullOrEmpty(text))
            return;
        
        var state = context.PushClip(rect);
        if (char.IsAscii(text[0]))
        {
            var ft = new FormattedText
            {
                Text = text,
                Constraint = new Size(rect.Width, rect.Height),
                Typeface = Typeface.Default,
                FontSize = 12
            };
            context.DrawText(ButtonTextPen.Brush, new Point(rect.Center.X - ft.Bounds.Width / 2, rect.Center.Y - ft.Bounds.Height / 2), ft);
        }
        else
        {
            var tl = new TextLayout(text, Typeface.Default, 12, ButtonTextPen.Brush, TextAlignment.Center, maxWidth: rect.Width, maxHeight:rect.Height);
            Draw(tl, context, new Point(rect.Left, rect.Center.Y - tl.Size.Height / 2));
        }
        state.Dispose();
    }
    
    
    
    /* not required in avalonia 11, just call layout.Draw(context, p) */
    private static void Draw(TextLayout layout, DrawingContext context, Point p)
    {
        double offset = layout.MaxWidth / 2 - layout.Size.Width / 2;
        using var _ = context.PushPostTransform(Matrix.CreateTranslation(p.X + offset, p.Y));
        layout.Draw(context);
    }
}