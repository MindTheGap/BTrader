using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace BTraderWPF.Windows.ResearchWindow
{
  /// <summary>
  /// Interaction logic for ResearchWindow.xaml
  /// </summary>
  public partial class ResearchWindow : Window
  {
    private ResearchViewModel _viewModel;

    public ResearchWindow(ResearchViewModel viewModel)
    {
      _viewModel = viewModel;

      InitializeComponent();

      DataContext = viewModel;
    }

    private void ResearchWindow_OnClosing(object sender, CancelEventArgs e)
    {
      var window = sender as ResearchWindow;
      if (window != null)
      {
        window.Visibility = Visibility.Hidden;
        e.Cancel = true;
      }
    }

    private void MainDataGrid_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      var dataGrid = sender as DataGrid;
      if (dataGrid == null) return;

      dataGrid.ContextMenu = null;

      var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
      var cell = VisualTreeHelper.GetParent(hit.VisualHit);
      while (cell != null && !(cell is DataGridCell))
      {
        cell = VisualTreeHelper.GetParent(cell);
      }
      var targetCell = cell as DataGridCell;

      if (targetCell == null)
      {
        // this means the user clicked on a place that is not a step
        return;
      }

      var targetCellList = GetRowAndColumnIndicesFromGivenCell(dataGrid, targetCell);
      var selectedCellList = GetRowAndColumnIndicesFromCellsSelection(dataGrid);
      if (selectedCellList == null || targetCellList == null)
      {
        return;
      }

      if (selectedCellList.Any(t => t.Item1 == targetCellList[0].Item1 && t.Item2 == targetCellList[0].Item2) == false)
      {
        return;
      }

      var m = new ContextMenu();
      var stepsList = dataGrid.SelectedItems.OfType<Step>().ToList();
      if (stepsList.Count == 0)
      {
        return;
      }

      var multipleStepsSelected = stepsList.Count > 1;

      if (!multipleStepsSelected)
      {
        var runStep = new MenuItem()
        {
          Header = "Run Step",
          Tag = new List<Step>(stepsList)
        };
        runStep.Click += RunStepOnClick;
        m.Items.Add(runStep);

        var runStepsFromHere = new MenuItem()
        {
          Header = "Run Steps From Here",
          Tag = new List<Step>(stepsList)
        };
        runStepsFromHere.Click += RunStepsFromHereOnClick;
        m.Items.Add(runStepsFromHere);
      }
      else
      {
        var runSelectedSteps = new MenuItem()
        {
          Header = "Run Selected Steps",
          Tag = new List<Step>(stepsList)
        };
        runSelectedSteps.Click += RunSelectedStepsOnClick;
        m.Items.Add(runSelectedSteps);
      }

      if (m.Items.Count <= 0) return;

      dataGrid.ContextMenu = m;
      dataGrid.ContextMenu.IsOpen = true;
    }

    private void RunStepOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (sender is MenuItem)
      {
        var menuItem = sender as MenuItem;
        var step = menuItem.Tag as Step;

        if (step != null)
        {
          _viewModel.RunStep(step);
        }
      }
    }

    private void RunStepsFromHereOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (sender is MenuItem)
      {
        var menuItem = sender as MenuItem;
        var step = menuItem.Tag as Step;

        if (step != null)
        {
          _viewModel.RunStepsFromHere(step);
        }
      }
    }

    private void RunSelectedStepsOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (sender is MenuItem)
      {
        var menuItem = sender as MenuItem;
        var steps = menuItem.Tag as List<Step>;

        if (steps != null)
        {
          _viewModel.RunSelectedSteps(steps);
        }
      }
    }

    #region Row And Column Indices

    private List<Tuple<int, int>> GetRowAndColumnIndicesFromCellsSelection(DataGrid dataGrid)
    {
      List<Tuple<int, int>> cellList = null;
      foreach (DataGridCellInfo selectedCell in dataGrid.SelectedCells)
      {
        var columnIndex = selectedCell.Column.DisplayIndex;

        var item = selectedCell.Item;

        int rowIndex = dataGrid.Items.IndexOf(item);
        if (rowIndex != -1)
        {
          if (cellList == null)
          {
            cellList = new List<Tuple<int, int>>();
          }

          cellList.Add(new Tuple<int, int>(rowIndex, columnIndex));
        }
      }

      return cellList;
    }

    private List<Tuple<int, int>> GetRowAndColumnIndicesFromGivenCell(DataGrid dataGrid, DataGridCell cell)
    {
      List<Tuple<int, int>> cellList = null;

      var columnIndex = cell.Column.DisplayIndex;
      var parent = VisualTreeHelper.GetParent(cell);
      while (parent != null && parent.GetType() != typeof (DataGridRow))
      {
        parent = VisualTreeHelper.GetParent(parent);
      }

      if (parent is DataGridRow)
      {
        var dataGridRow = parent as DataGridRow;
        var item = dataGridRow.Item;
        if (item != null)
        {
          var rowIndex = dataGrid.Items.IndexOf(item);
          if (rowIndex != -1)
          {
            cellList = new List<Tuple<int, int>>()
            {
              new Tuple<int, int>(rowIndex, columnIndex)
            };
          }
        }
      }

      return cellList;
    }

    #endregion
  }
}
