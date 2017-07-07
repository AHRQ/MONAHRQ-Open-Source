using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Theme.Controls.MultiSelect;

namespace Monahrq.Infrastructure.Types
{
    public class MultiSelectCollectionView<T> : ListCollectionView, IMultiSelectCollectionView
        where T : class, ISelectable
    {
        public MultiSelectCollectionView(IList list)
            : base(list)
        {
        }

        void IMultiSelectCollectionView.AddControl(Selector selector)
        {
            _controls.Add(selector);
            SetSelection(selector);

            //var multiSelector = selector as DataGrid;
            selector.SelectionChanged -= control_SelectionChanged;

            if (selector is DataGrid)
            {
                (selector as DataGrid).SelectedCellsChanged += GridOnSelectedCellsChanged;
                selector.SelectionChanged -= control_SelectionChanged;
            }
            else
            {
                selector.SelectionChanged += control_SelectionChanged;
                // selector.SelectedCellsChanged -= GridOnSelectedCellsChanged;
            }
        }

        #region Unused Code
        //private static bool IsDataGridCheckboxColumn(Selector selector, out DataGrid datagrid)
        //{
        //    datagrid = selector as DataGrid;
        //    return true;

        //    var grid = selector as DataGrid;
        //    if (grid != null)
        //    {
        //        CheckBox checkBox = null;
        //        if (grid.Columns != null && grid.Columns.Any())
        //        {
        //            DataGridTemplateColumn dgc = grid.Columns[0] as DataGridTemplateColumn;

        //            foreach (var d in grid.Columns)
        //            {

        //            }
        //            foreach (var dataGridCell in grid.SelectedCells.ToList())
        //            {
        //                if (dataGridCell.Column.DisplayIndex > 0) continue;

        //                var tem = dataGridCell.Item;
        //            }
        //            //checkBox = dgc.FindChild<CheckBox>("SelectionCheckBox");
        //        }

        //        if (checkBox != null)
        //        {
        //            datagrid = grid;
        //            return true;
        //        }

        //        datagrid = null;
        //        return false;
        //    }

        //    datagrid = null;
        //    return false;
        //}
        #endregion

        public void DeselectAll()
        {

            // Remove all the selected items
            //if (SelectedItems != null)
            {
                SourceCollection.OfType<T>().ForEach(item =>
                {
                    item.IsSelected = false;

                    //if (SelectedItems.Contains(item))
                    //    SelectedItems.Remove(item);
                });

                //SelectedItems.Clear();
            }


            // Update the UI controls.
            foreach (var control in _controls)
                SetSelection((Selector)control);
        }

        public void SelectAll()
        {

            // Reload all the elements in the selected collection
            SourceCollection.OfType<T>().ForEach(item =>
            {
                if (item.IsSelected)
                    item.IsSelected = true;
            });
                            
                       
            //foreach (T item in SourceCollection)
            //    SelectedItems.Add(item);

            // Update the UI controls.
            foreach (var control in _controls.ToList())
                SetSelection(control as Selector);
        }

        private void GridOnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (_ignoreSelectionChanged) return;

            var changed = false;

            _ignoreSelectionChanged = true;

            try
            {
                if (e.AddedCells.Any())
                {
                    var addedCols = e.AddedCells.Where(c => c.Column.DisplayIndex > 0).ToList();
                    if (addedCols.Count == 1)
                    {
                        if (addedCols[0].Column.DisplayIndex == 0) return;

                        var item = addedCols[0].Item as T;

                        if (item == null /*|| SelectedItems.Contains(item)*/) return;

                        if (item is IWebsiteReportValidableSelectable)
                        {
                            if (((IWebsiteReportValidableSelectable)item).ValidateBeforSelection(new WebsiteReportValidableSelectableStruct { Items = SelectedItems.OfType<object>().ToList(), WebsiteAudiences = ((IWebsiteReportValidableSelectable)item).WebsiteAudiences }))
                            {
                                item.IsSelected = true;
                            }
                        }
                        else if (item is IValidableSelectable )
                        {
                            if (((IValidableSelectable) item).ValidateBeforSelection(SelectedItems))
                            {
                                item.IsSelected = true;
                            }
                        }
                        else
                        {
                            item.IsSelected = true;
                        }
                        
                        //SelectedItems.Add(item);
                        changed = true;
                    }
                    else
                    {
                        foreach (var cell in addedCols)
                        {
                            if (cell.Column.DisplayIndex == 0) break;

                            var item = cell.Item as T;

                            if (item == null || item.IsSelected) continue;

                            if (item is IWebsiteReportValidableSelectable)
                            {
                                if (((IWebsiteReportValidableSelectable)item).ValidateBeforSelection(new WebsiteReportValidableSelectableStruct {Items = SelectedItems.OfType<object>().ToList(), WebsiteAudiences = ((IWebsiteReportValidableSelectable)item).WebsiteAudiences}))
                                {
                                    item.IsSelected = true;
                                }
                            }
                            else if (item is IValidableSelectable)
                            {
                                if (((IValidableSelectable)item).ValidateBeforSelection(SelectedItems))
                                {
                                    item.IsSelected = true;
                                }
                            }
                            else
                            {
                                item.IsSelected = true;
                            }

                           // item.IsSelected = true;
                            //SelectedItems.Add(item);
                            changed = true;
                        }
                    }
                }

                #region unused code
                //if (e.RemovedCells.Any())
                //{
                //    var removedCells = e.RemovedCells.Where(c => c.Column != null && c.Column.DisplayIndex == 0).ToList();

                //    if (removedCells.Count == 1 /*&& SelectedItems.Contains(removedCells[0].Item)*/)
                //    {
                //        var currentItem = removedCells[0].Item as T;
                //        if (currentItem != null)
                //        {
                //            currentItem.IsSelected = false;
                //            changed = true;
                //            //SelectedItems.Remove(currentItem);
                //        }

                //        return;
                //    }

                //    foreach (var cell in removedCells)

                //    {
                //        if (cell.Column.DisplayIndex > 0) break;

                //        T item = cell.Item as T;

                //        if (item == null) continue;

                //        item.IsSelected = false;
                //        //if (SelectedItems.Remove(item))
                //        {
                //            changed = true;
                //        }
                //    }
                //}
                #endregion

                if (changed)
                {
                    foreach (Selector control in _controls)
                    {
                        if (control != sender)
                        {
                            SetSelection(control);
                        }
                    }
                }
            }
            finally
            {
                _ignoreSelectionChanged = false;
            }
        }

        void IMultiSelectCollectionView.RemoveControl(Selector selector)
        {
            if (_controls.Remove(selector))
            {
                if (selector is DataGrid)
                {
                    ((DataGrid)selector).SelectedCellsChanged -= GridOnSelectedCellsChanged;
                    selector.SelectionChanged -= control_SelectionChanged;
                }
                else
                {
                    selector.SelectionChanged -= control_SelectionChanged;
                }
            }
        }

        public void SetSelectOnly(bool isSelectOnly)
        {
            _isSelectOnly = isSelectOnly;
        }

        public ObservableCollection<T> SelectedItems
        {
            get
            {
                return SourceCollection.OfType<T>()
                                       .Where(item => item.IsSelected)
                                       .ToObservableCollection();
            }
        }

        void SetSelection(Selector selector)
        {
            var multiSelector = selector as MultiSelector;
            var listBox = selector as ListBox;

            if (multiSelector != null)
            {
                multiSelector.SelectedItems.Clear();

                foreach (var item in SourceCollection.OfType<T>().Where(x => x.IsSelected).ToList())
                {
                    if (multiSelector.SelectedItems.Contains(item)) continue;
                    multiSelector.SelectedItems.Add(item);
                }
            }
            else if (listBox != null)
            {
                listBox.SelectedItems.Clear();

                foreach (var item in SourceCollection.OfType<T>().Where(x => x.IsSelected).ToList())
                {
                    if (listBox.SelectedItems.Contains(item)) continue;
                    listBox.SelectedItems.Add(item);
                }
            }
        }

        void control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ignoreSelectionChanged) return;

            var changed = false;

            _ignoreSelectionChanged = true;

            try
            {
                foreach (T item in e.AddedItems.OfType<T>().ToList())
                {
                    T item2 = item as T;

                    if (item2 == null) continue;

                    if (item.IsSelected) continue;
                    item.IsSelected = true;
                   // SelectedItems.Add(item);
                    changed = true;
                }

                foreach (T item in e.RemovedItems.OfType<T>().ToList())
                {
                    item.IsSelected = false;
                    //if (SelectedItems.Remove(item))
                    {
                        changed = true;
                    }
                }

                if (changed)
                {
                    foreach (Selector control in _controls)
                    {
                        if (control != sender)
                        {
                            SetSelection(control);
                        }
                    }
                }
            }
            finally
            {
                _ignoreSelectionChanged = false;
            }
        }

        private bool _isSelectOnly;
        private bool _ignoreSelectionChanged;
        readonly List<Selector> _controls = new List<Selector>();


    }
}
