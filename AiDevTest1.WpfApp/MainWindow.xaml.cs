using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AiDevTest1.WpfApp.ViewModels;

namespace AiDevTest1.WpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    /// <summary>
    /// ウィンドウが閉じられる前に呼び出されるイベントハンドラー
    /// </summary>
    /// <param name="e">キャンセル可能なイベント引数</param>
    protected override void OnClosing(CancelEventArgs e)
    {
        // ViewModelの終了処理を呼び出し
        var canClose = _viewModel.OnWindowClosing();
        if (!canClose)
        {
            e.Cancel = true;
            return;
        }

        base.OnClosing(e);
    }
}
