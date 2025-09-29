using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AegisDispatcher.Services;

namespace AegisDispatcher.Controls
{
    public partial class AddressSearchControl : UserControl, INotifyPropertyChanged
    {
        private IGeocodingService _geocodingService;
        private string _searchText = string.Empty;
        private bool _isSearching = false;
        private List<GeocodingResult> _searchResults = new List<GeocodingResult>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<AddressSelectedEventArgs> AddressSelected;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSearching
        {
            get => _isSearching;
            private set
            {
                if (_isSearching != value)
                {
                    _isSearching = value;
                    OnPropertyChanged();
                    UpdateSearchingState();
                }
            }
        }

        public AddressSearchControl()
        {
            InitializeComponent();
            DataContext = this;
            
            // Geocoding service will be set via SetGeocodingService method
            _geocodingService = null!;
        }

        public AddressSearchControl(IGeocodingService geocodingService) : this()
        {
            _geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
        }

        public void SetGeocodingService(IGeocodingService geocodingService)
        {
            _geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch();
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformSearch();
            }
            else if (e.Key == Key.Escape)
            {
                HideResults();
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Show results if we have any
            if (_searchResults.Any())
            {
                ShowResults();
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Hide results after a short delay to allow for selection
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!IsKeyboardFocusWithin)
                {
                    HideResults();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultsListBox.SelectedItem is GeocodingResult selectedResult)
            {
                SelectAddress(selectedResult);
            }
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem is GeocodingResult selectedResult)
            {
                SelectAddress(selectedResult);
            }
        }

        private async Task PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText) || IsSearching || _geocodingService == null)
                return;

            try
            {
                IsSearching = true;
                ShowResults();

                var results = await _geocodingService.SearchAddressAsync(SearchText);
                
                _searchResults = results?.ToList() ?? new List<GeocodingResult>();
                
                if (_searchResults.Any())
                {
                    ResultsListBox.ItemsSource = _searchResults;
                    ResultsListBox.Visibility = Visibility.Visible;
                    NoResultsMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ResultsListBox.Visibility = Visibility.Collapsed;
                    NoResultsMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search failed: {ex.Message}", "Search Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                
                ResultsListBox.Visibility = Visibility.Collapsed;
                NoResultsMessage.Visibility = Visibility.Visible;
            }
            finally
            {
                IsSearching = false;
            }
        }

        private void SelectAddress(GeocodingResult result)
        {
            if (result == null) return;

            // Update search text with selected address
            SearchText = result.DisplayName;
            
            // Hide results
            HideResults();
            
            // Raise the AddressSelected event
            AddressSelected?.Invoke(this, new AddressSelectedEventArgs(result));
        }

        private void ShowResults()
        {
            ResultsContainer.Visibility = Visibility.Visible;
        }

        private void HideResults()
        {
            ResultsContainer.Visibility = Visibility.Collapsed;
        }

        private void UpdateSearchingState()
        {
            if (IsSearching)
            {
                LoadingIndicator.Visibility = Visibility.Visible;
                ResultsListBox.Visibility = Visibility.Collapsed;
                NoResultsMessage.Visibility = Visibility.Collapsed;
                SearchButton.IsEnabled = false;
            }
            else
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
                SearchButton.IsEnabled = true;
            }
        }

        public void ClearResults()
        {
            _searchResults.Clear();
            ResultsListBox.ItemsSource = null;
            HideResults();
        }

        public void SetSearchText(string text)
        {
            SearchText = text;
            ClearResults();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AddressSelectedEventArgs : EventArgs
    {
        public GeocodingResult SelectedAddress { get; }

        public AddressSelectedEventArgs(GeocodingResult selectedAddress)
        {
            SelectedAddress = selectedAddress;
        }
    }
}