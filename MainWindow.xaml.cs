using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using System.Windows.Interop;
using StaticMapImageSeeker.OpenStreetMap;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Win32;

namespace StaticImageMap
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private Bitmap currentBitmap;
        private OSMGeoFacade geoFacade;

        #endregion

        #region Initializers and constructors

        public MainWindow()
        {
            InitializeComponent();
            InitGeoFacade();
        }
        private void InitGeoFacade()
        {
            this.geoFacade = new OSMGeoFacade(new OSMGeocoder(), new OSMGeoImageLinkFormatter());
        }

        #endregion

        #region Utilities
        public BitmapImage ConvertFromBitmap(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
        #endregion

        #region Event Handlers

        private void SearchMapButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchMapTextbox.Text != "")
            {
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    this.InputGrid.IsEnabled = false;
                    try
                    {
                        int imageWidth = int.Parse(this.ImageWidthTextbox.Text), imageHeight = int.Parse(this.ImageHeightTextbox.Text);
                        if(imageHeight <= 0 || imageWidth <= 0)
                            throw new Exception();
                        Bitmap mapImage = await this.geoFacade.GetStaticMapImage(this.SearchMapTextbox.Text, (int)this.PolygonSlider.Value, 
                            imageWidth, imageHeight);
                        this.mapImageView.Source = ConvertFromBitmap(mapImage);
                        this.currentBitmap = mapImage;
                    }
                    catch
                    {
                        MessageBox.Show($"Проверьте правильность введенных данных.{(this.PolygonSlider.Value < 3 ? "Возможно, размер запроса превышает максимально допустимую длину. Попробуйте увеличить множитель снижения полигональности" : "")}");
                    }
                    finally
                    {
                        this.InputGrid.IsEnabled = true;
                    }
                }));
            }
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.PolygonMultiplierLabel.Content = $"{this.PolygonSlider.Value}x";
        }
        private void ImageHeightTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void ImageWidthTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentBitmap != null)
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    DefaultExt = ".png",
                    Filter = "png файлы(*.png) |*.png| Все файлы(*.*) | *.*",
                };
                if (dialog.ShowDialog() == true)
                {
                    this.currentBitmap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        #endregion

    }
}
