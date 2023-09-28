using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace PDFViewer
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PdfDocument _pdfDocument = null;
        // 表示中のページ番号
        private uint _pageIndex = 0;
        // ページ総数
        private uint _pageCount = 0;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // ********************
        // 「PDFファイルを開く」ボタン押下処理
        // ********************
        private async void btnOpenPdf_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    // 現在表示しているPDFを破棄する
                    imgPdf.Source = null;

                    // 表示するページ番号の設定
                    _pageIndex = 0;

                    // PDFファイルを読み込む
                    _pdfDocument = await PdfDocument.LoadFromFileAsync(file);

                    // 前へボタンを非活性にする
                    btnPrev.IsEnabled = false;

                    // ページ数を取得
                    _pageCount = _pdfDocument.PageCount;

                    // 1ページ以下の場合次へボタンを非活性にする
                    if (_pageCount <= 1)
                    {
                        btnNext.IsEnabled = false;
                    }

                    ShowPdf();
                }
                catch (Exception ex)
                {
                    Windows.UI.Popups.MessageDialog dlgMsg = new Windows.UI.Popups.MessageDialog(ex.Message, "エラー");
                    await dlgMsg.ShowAsync();
                    
                    grdPdf.Visibility = Visibility.Collapsed;
                }
            }
        }

        // ********************
        // PDFファイル表示処理
        // ********************
        private async void ShowPdf()
        {
            if (_pdfDocument != null)
            {
                // PDF表示領域を表示する
                grdPdf.Visibility = Visibility.Visible;

                // ProgressRingを表示する
                progressRing.Visibility = Visibility.Visible;

                // PDFを読み込む
                using (PdfPage page = _pdfDocument.GetPage(_pageIndex))
                {
                    // ストリーム作成
                    var stream = new InMemoryRandomAccessStream();
                    await page.RenderToStreamAsync(stream);
                    BitmapImage src = new BitmapImage();

                    // PDFをImageコントロール内に収まるようにする
                    imgPdf.Stretch = Stretch.Fill;

                    imgPdf.Source = src;

                    await src.SetSourceAsync(stream);
                }

                progressRing.Visibility = Visibility.Collapsed;
            }
        }

        // ********************
        // 「前へ」ボタン押下処理
        // ********************
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            _pageIndex--;

            btnNext.IsEnabled = true;

            // 先頭ページに達した場合
            if (_pageIndex == 0)
            {
                btnPrev.IsEnabled = false;
            }

            ShowPdf();
        }

        // ********************
        // 「次へ」ボタン押下処理
        // ********************
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            _pageIndex++;

            btnPrev.IsEnabled = true;

            // 最終ページに達した場合
            if (_pageIndex == (_pageCount - 1))
            {
                btnNext.IsEnabled = false;
            }

            ShowPdf();
        }
    }
}
