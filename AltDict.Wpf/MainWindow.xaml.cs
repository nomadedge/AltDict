using AltDict.Data.Dtos;
using AltDict.Data.Repositories;
using AltDict.Wpf.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AltDict.Wpf
{
    public partial class MainWindow : Window
    {
        private IAltDictRepository _altDictRepository;
        private IMapper _mapper;

        private ConnectionDto NewConnection { get; set; }
        private ConnectionDto ConnectionForEdit { get; set; }
        private SearchDto Search { get; set; }
        private List<List<SearchResultModel>> SearchResults { get; set; }

        public MainWindow(
            IAltDictRepository altDictRepository,
            IMapper mapper)
        {
            InitializeComponent();

            _altDictRepository = altDictRepository;
            _mapper = mapper;

            GetConnections();

            NewConnection = new ConnectionDto();
            ConnectionForEdit = new ConnectionDto();
            Search = new SearchDto();

            EditConnectionGrid.DataContext = ConnectionForEdit;
            AddNewConnectionGrid.DataContext = NewConnection;
            SearchRoutesGrid.DataContext = Search;
        }

        private void ShowMessageBox(string message)
        {
            MessageBox.Show(message, "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
        }

        private void GetConnections()
        {
            ConnectionsDataGrid.ItemsSource = _altDictRepository.GetAllConnections();
        }

        private void AddConnection(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                _altDictRepository.CreateUpdateConnection(NewConnection);
                GetConnections();
                NewConnection = new ConnectionDto();
                AddNewConnectionGrid.DataContext = NewConnection;
            }
            catch (Exception e)
            {
                ShowMessageBox(e.Message);
            }
        }

        private void SetConnectionForEdit(object sender, RoutedEventArgs eventArgs)
        {
            ConnectionForEdit = (sender as FrameworkElement).DataContext as ConnectionDto;
            EditConnectionGrid.DataContext = ConnectionForEdit;
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
        }

        private void StopEdit()
        {
            ConnectionForEdit = new ConnectionDto();
            EditConnectionGrid.DataContext = ConnectionForEdit;
            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
        }

        private void CancelEdit(object sender, RoutedEventArgs eventArgs)
        {
            StopEdit();
        }

        private void EditConnection(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                _altDictRepository.CreateUpdateConnection(ConnectionForEdit);
                GetConnections();
                StopEdit();
            }
            catch (Exception e)
            {
                ShowMessageBox(e.Message);
            }
        }

        private void DeleteConnection(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                var connectionForDelete = (sender as FrameworkElement).DataContext as ConnectionDto;
                _altDictRepository.DeleteConnection(connectionForDelete.ConnectionId.Value);
                GetConnections();
            }
            catch (Exception e)
            {
                ShowMessageBox(e.Message);
            }
        }

        private static readonly Regex _regex = new Regex("^[0-9]+$");
        private bool IsTextAllowed(string text)
        {
            if (_regex.IsMatch(text) && byte.TryParse(text, out _))
            {
                return false;
            }
            return true;
        }

        private void ValidateInput(object sender, TextCompositionEventArgs eventArgs)
        {
            var text = (eventArgs.OriginalSource as TextBox).Text + eventArgs.Text;
            eventArgs.Handled = IsTextAllowed(text);
        }

        private void ValidatePaste(object sender, DataObjectPastingEventArgs eventArgs)
        {
            if (eventArgs.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)eventArgs.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    eventArgs.CancelCommand();
                }
            }
            else
            {
                eventArgs.CancelCommand();
            }
        }

        private void SearchRoutes(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                SearchResults = _mapper.Map<List<List<SearchResultModel>>>(_altDictRepository.SearchRoutes(Search));
                if (!SearchResults.Any())
                {
                    ShowMessageBox($"Product connection was not found in {Search.SearchDepth} steps.");
                    return;
                }
                var routes = SearchResults.Select((sr, i) => new RouteModel
                {
                    Index = i,
                    StepsCount = sr.Count
                }).ToList();
                SearchResultRoutesDataGrid.ItemsSource = routes;
                SearchResultStepsDataGrid.ItemsSource = SearchResults.First();
            }
            catch (Exception e)
            {
                ShowMessageBox(e.Message);
            }
        }

        private void ShowRoute(object sender, RoutedEventArgs eventArgs)
        {
            var routeModel = (sender as FrameworkElement).DataContext as RouteModel;
            SearchResultStepsDataGrid.ItemsSource = SearchResults[routeModel.Index];
        }
    }
}
