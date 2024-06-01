using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using src.ViewModels;
using static Converter;
using static BM;
using static KMP;
using static LevenshteinDistance;
using Avalonia.Controls.Shapes;

namespace src.Views
{
    public partial class MainWindow : Window
    {
        public Bitmap uploadedImage;
        private readonly DatabaseHelper dbHelper = new();
        private readonly string baseDirectory = AppContext.BaseDirectory;



        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private async void OnImageUploadClicked(object sender, RoutedEventArgs e)
        {
            var customBMPFileType = new FilePickerFileType("Only BMP Images")
            {
                Patterns = new[] { "*.BMP" },
                AppleUniformTypeIdentifiers = new[] { "org.webmproject.BMP" },
                MimeTypes = new[] { "image/BMP" }
            };

            var selectedFiles = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = new[] { customBMPFileType },
                Title = "Select an image file"
            });

            if (selectedFiles != null && selectedFiles.Count > 0)
            {
                using var stream = await selectedFiles[0].OpenReadAsync();
                uploadedImage = new Bitmap(stream);
                imageUploaded.Fill = new ImageBrush { Source = uploadedImage };
                EnableSearchButtonIfReady();
            }
        }

        private void OnAlgorithmChecked(object sender, RoutedEventArgs e) => EnableSearchButtonIfReady();

        private void EnableSearchButtonIfReady()
        {
            SearchButton.IsEnabled = uploadedImage != null && (BMRadioButton.IsChecked == true || KMPRadioButton.IsChecked == true);
        }

        private void OnSearchClicked(object sender, RoutedEventArgs e)
        {
            if (uploadedImage == null) return;

            var stopwatch = Stopwatch.StartNew();
            string algorithm = BMRadioButton.IsChecked == true ? "BM" : "KMP";
            var imageBinary = GetSelectedBinary(uploadedImage);
            string imageAscii = ConvertBinaryToAscii(imageBinary);
            var imagesToCompare = dbHelper.GetAllImages();

            bool found = SearchForExactMatch(imagesToCompare, imageAscii, algorithm);
            stopwatch.Stop();

            if (!found)

            {

                string fullAscii = ConvertBinaryToAscii(ConvertFullImageToBinary(uploadedImage));
                SearchForApproximateMatch(imagesToCompare, fullAscii);
            }

            executionTimeTextBlock.Text = $"Waktu Pencarian: {stopwatch.ElapsedMilliseconds} ms";
        }

        private bool SearchForExactMatch(IEnumerable<ImageRecord> imagesToCompare, string imageAscii, string algorithm)
        {
            foreach (var imageEntity in imagesToCompare)
            {
                string imagePath = GetImagePath(imageEntity.BerkasCitra);
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"File {imagePath} not found.");
                    continue;
                }

                using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                var dbImage = new Bitmap(stream);
                var dbImageBinary = ConvertFullImageToBinary(dbImage);
                var dbImageAscii = ConvertBinaryToAscii(dbImageBinary);

                int result = algorithm == "BM" ? BmMatch(dbImageAscii, imageAscii) : KmpMatch(dbImageAscii, imageAscii);

                if (result != -1)
                {
                    matchedImage.Fill = new ImageBrush { Source = dbImage };
                    DisplayPersonDetails(imageEntity.Name);
                    similarityTextBlock.Text = "Persentase Kecocokan: 100%";
                    return true;
                }
            }
            return false;
        }

        private void SearchForApproximateMatch(IEnumerable<ImageRecord> imagesToCompare, string imageAscii)
        {
            double maxSimilarity = 0.0;
            ImageRecord bestMatch = null;

            foreach (var imageEntity in imagesToCompare)
            {
                string imagePath = GetImagePath(imageEntity.BerkasCitra);
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"File {imagePath} not found.");
                    continue;
                }

                using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                var dbImage = new Bitmap(stream);
                var dbImageBinary = ConvertFullImageToBinary(dbImage);
                var dbImageAscii = ConvertBinaryToAscii(dbImageBinary);

                double similarity = ComputeSimilarity(dbImageAscii, imageAscii);

                
                if (similarity > maxSimilarity)
                {
                    maxSimilarity = similarity;
                    bestMatch = imageEntity;
                }
            }

            if (maxSimilarity > 60)
            {
                matchedImage.Fill = new ImageBrush { Source = new Bitmap(GetImagePath(bestMatch.BerkasCitra)) };
                DisplayPersonDetails(bestMatch.Name);
                similarityTextBlock.Text = $"Persentase Kecocokan: {maxSimilarity}%";
            }
            else
            {

                similarityTextBlock.Text = "Persentase Kecocokan: 0%";
            }
        }

        private void DisplayPersonDetails(string name)
        {
            var person = dbHelper.GetAllPeople().FirstOrDefault(p => p.Nama == name);
            if (person != null)
            {
                var details = new TextBlock
                {
                    Text = $"Name: {person.Nama}\n" +
                           $"NIK: {person.NIK}\n" +
                           $"Tempat Lahir: {person.Tempat_lahir}\n" +
                           $"Tanggal Lahir: {person.Tanggal_lahir:dd-MM-yyyy}\n" +
                           $"Jenis Kelamin: {person.Jenis_kelamin}\n" +
                           $"Golongan Darah: {person.Golongan_darah}\n" +
                           $"Alamat: {person.Alamat}\n" +
                           $"Agama: {person.Agama}\n" +
                           $"Status Perkawinan: {person.Status_perkawinan}\n" +
                           $"Pekerjaan: {person.Pekerjaan}\n" +
                           $"Kewarganegaraan: {person.Kewarganegaraan}",
                    Foreground = Brushes.Black
                };
                personDetails.Text = details.Text;
            }
        }

        private string GetImagePath(string imageName)
        {
            string truncatedBaseDirectory = baseDirectory.Substring(0, baseDirectory.IndexOf("src", StringComparison.Ordinal));
            return System.IO.Path.Combine(truncatedBaseDirectory, "test", "Real", imageName);
        }
    }
}
