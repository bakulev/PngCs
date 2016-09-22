using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Hjg.Pngcs;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestPngCs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                PngReader pngReader = new PngReader(stream.AsStream());
                Debug.WriteLine(pngReader);
            }
            else
            {
                //  
            }
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("PNG Image", new List<string>() { ".png" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "map2";
            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);

                // write to file
                //await Windows.Storage.FileIO.WriteTextAsync(file, file.Name);
                var stream = (file.OpenStreamForWriteAsync()).Result;
                ImageInfo imgInfo = new ImageInfo(200, 100, 16, false, true, false);
                PngWriter wrt = new PngWriter(stream, imgInfo);
                Hjg.Pngcs.Chunks.PngMetadata metadata = wrt.GetMetadata();
                metadata.SetText("test", "value");
                ImageLines imLines = new ImageLines(imgInfo, ImageLine.ESampleType.INT, false, 0, 100, 1);
                // fill in image by random values.
                Random rnd = new Random();
                for (int i = 0; i < imLines.ImgInfo.Rows; i++)
                    for (int j = 0; j < imLines.ImgInfo.Cols; j++)
                        imLines.Scanlines[i][j] = rnd.Next(0, ushort.MaxValue);
                for (int i = 0; i < imLines.ImgInfo.Rows; i++)
                    for (int j = 0; j < imLines.ImgInfo.Cols / 10; j++)
                        imLines.Scanlines[i][j] = (int)((ushort.MaxValue * 254.1) / 255.0);
                wrt.WriteRowsInt(imLines.Scanlines);
                wrt.End();
                //ream.Dispose();

                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    this.textBlock.Text = "File " + file.Name + " was saved.";
                }
                else
                {
                    this.textBlock.Text = "File " + file.Name + " couldn't be saved.";
                }
            }
            else
            {
                this.textBlock.Text = "Action cancelled.";
            }
        }

        /*private async Task Do()
        {
            await Task.Run(() => {
                PngReader png = FileHelper.CreatePngReader(Windows.Storage.KnownFolders.PicturesLibrary.Path + "map.png");
                Debug.WriteLine(png);
                //ImageInfo imgInfo = new ImageInfo(png.ImgInfo);
                PngWriter wrt = FileHelper.CreatePngWriter(Windows.Storage.KnownFolders.PicturesLibrary.Path + "map2.png", png.ImgInfo, true);
                ImageLines imLines = new ImageLines(png.ImgInfo, ImageLine.ESampleType.INT, true, 0, png.ImgInfo.Rows, 1);
                wrt.WriteRowsInt(imLines.Scanlines);
                wrt.End();
            });
        }*/
    }
}
