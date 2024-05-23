using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using src.ViewModels;
using System;
using System.IO;
using static Converter;
using static AlayMatcher;
using static BM;
using static KMP;
using static LevenshteinDistance;

namespace src.Views
{
    public partial class MainWindow : Window
    {
        private readonly Window openFileDialog; // Use OpenFileDialog instead of Window
        private Bitmap uploadedImage; // Store the uploaded image

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            openFileDialog = new Window(); // Initialize OpenFileDialog
        }

        private async void OnImageUploadClicked(object sender, RoutedEventArgs e)
        {
            // Set properties of the file dialog
            var customBMPFileType = new FilePickerFileType("Only BMP Images")
            {
                Patterns = ["*.BMP"],
                AppleUniformTypeIdentifiers = [ "org.webmproject.BMP"
                ],
                MimeTypes = [ "image/BMP"
                ]
            };

            var selectedFiles = await openFileDialog.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [customBMPFileType],
                Title = "Select an image file"
            });

            // Show the file picker dialog
            // Check if a file was selected
            if (selectedFiles != null && selectedFiles.Count > 0)
            {
                // Open reading stream from the first file.
                using var stream = await selectedFiles[0].OpenReadAsync();

                // Create a Bitmap from the stream.
                uploadedImage = new Bitmap(stream);

                // Create an ImageBrush using the Bitmap.
                var imageBrush = new ImageBrush
                {
                    Source = uploadedImage
                };

                // Set the Fill property of the Rectangle to the ImageBrush.
                imageUploaded.Fill = imageBrush;

                // Enable the Search button if an algorithm is selected
                EnableSearchButtonIfReady();
            }
        }

        private void OnAlgorithmChecked(object sender, RoutedEventArgs e)
        {
            // Enable the Search button if an image is uploaded
            EnableSearchButtonIfReady();
        }

        private void EnableSearchButtonIfReady()
        {
            // Check if an image is uploaded and an algorithm is selected
            if (uploadedImage != null && (BMRadioButton.IsChecked == true || KMPRadioButton.IsChecked == true))
            {
                SearchButton.IsEnabled = true;
            }
            else
            {
                SearchButton.IsEnabled = false;
            }
        }

        private void OnSearchClicked(object sender, RoutedEventArgs e)
        {
            // Perform search using the uploaded image
            if (uploadedImage != null)
            {
                // Get the selected algorithm
                string algorithm = BMRadioButton.IsChecked == true ? "BM" : "KMP";

                // Get the image data
                var image_binary = ConvertToBinary(uploadedImage, 128);

                var image_ascii = ConvertBinaryToAscii(image_binary);

                // Get the pattern


                // Perform search

            }
        }
    }
}
