﻿using ExceptionLogger;
using Fingerprints.Factories;
using Fingerprints.MinutiaeTypes;
using Fingerprints.Models;
using Fingerprints.Resources;
using Fingerprints.Tools;
using Fingerprints.Tools.Exporters;
using Fingerprints.Windows.Controls;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Fingerprints.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        private MinutiaeTypeController dbController;

        private bool _bCanComboBoxChangeCurrentDrawing;

        public ObservableCollection<MinutiaState> MinutiaeStates { get; set; }

        private MinutiaState selectedComboboxItem;
        public MinutiaState SelectedComboboxItem { get { return selectedComboboxItem; } set { SetProperty(ref selectedComboboxItem, value); } }

        public DrawingService LeftDrawingService { get; }

        public DrawingService RightDrawingService { get; }

        public GridContextMenu ContextMenuLeftObject { get; }
        public GridContextMenu ContextMenuRightObject { get; }

        public MyObservableCollection<MinutiaStateBase> LeftDrawingData
        { get { return LeftDrawingService.DrawingData; } }

        public MyObservableCollection<MinutiaStateBase> RightDrawingData
        { get { return RightDrawingService.DrawingData; } }

        public DataGridActivities DataGridActivities { get; }

        public ICommand SaveClickCommand { get; }
        public ICommand MinutiaeStatesSelectionChanged { get; }
        public ICommand SaveAsClickCommand { get; }
        public ICommand LoadLeftImageCommand { get; }
        public ICommand LoadRightImageCommand { get; }

        public MainWindowViewModel()
        {
            try
            {
                _bCanComboBoxChangeCurrentDrawing = true;

                dbController = new MinutiaeTypeController();

                //Initialize Drawing Services
                LeftDrawingService = new DrawingService();
                RightDrawingService = new DrawingService();

                //Add method for CollectinoChanged
                LeftDrawingData.CollectionChanged += LeftDrawingDataChanged;
                RightDrawingData.CollectionChanged += RightDrawingDataChanged;

                ContextMenuLeftObject = new GridContextMenu(LeftDrawingService, RightDrawingService);
                ContextMenuRightObject = new GridContextMenu(RightDrawingService, LeftDrawingService);

                //init grid with data from drawing services
                DataGridActivities = new DataGridActivities(LeftDrawingService, RightDrawingService);

                //Get MinutiaeStates for combobox
                MinutiaeStates = new ObservableCollection<MinutiaState>(dbController.getStates());

                //button clicks delegates
                SaveClickCommand = new DelegateCommand(SaveClick);
                MinutiaeStatesSelectionChanged = new DelegateCommand<MinutiaState>(MinutiaStatesSelectionChanged, CanComboBoxChangeCurrentDrawing);
                SaveAsClickCommand = new DelegateCommand(SaveAsClick);
                LoadLeftImageCommand = new DelegateCommand(LoadLeftImage);
                LoadRightImageCommand = new DelegateCommand(LoadRightImage);
                LeftDrawingService.CurrentDrawingChanged += LeftDrawingService_CurrentDrawingChanged;
                RightDrawingService.CurrentDrawingChanged += RightDrawingService_CurrentDrawingChanged;

                LeftDrawingService.DrawingObjectAdded += LeftDrawingService_DrawingObjectAdded;
                RightDrawingService.DrawingObjectAdded += RightDrawingService_DrawingObjectAdded;

                LeftDrawingService.NewDrawingInitialized += LeftDrawingService_NewDrawingInitialized;
                RightDrawingService.NewDrawingInitialized += RightDrawingService_NewDrawingInitialized;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        private void RightDrawingService_NewDrawingInitialized(object sender, EventArgs e)
        {
            try
            {
                if (LeftDrawingService.CurrentDrawing != null && LeftDrawingData[RightDrawingData.Count - 2].Minutia.TypeId == 7 &&
                    RightDrawingData[RightDrawingData.Count - 2].Minutia.TypeId == LeftDrawingService.CurrentDrawing.Minutia.TypeId)
                {
                    LeftDrawingService.CurrentDrawing.InsertIndex = RightDrawingData.Count - 2;
                    LeftDrawingService.RefreshDrawingIndexTarget(LeftDrawingService.CurrentDrawing.InsertIndex.Value);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        private void LeftDrawingService_NewDrawingInitialized(object sender, EventArgs e)
        {
            try
            {
                if (RightDrawingService.CurrentDrawing != null && RightDrawingData[LeftDrawingData.Count - 2].Minutia.TypeId == 7
                    && LeftDrawingService.DrawingData[RightDrawingData.Count - 2].Minutia.TypeId == RightDrawingService.CurrentDrawing.Minutia.TypeId)
                {
                    RightDrawingService.CurrentDrawing.InsertIndex = LeftDrawingData.Count - 2;
                    RightDrawingService.RefreshDrawingIndexTarget(RightDrawingService.CurrentDrawing.InsertIndex.Value);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        private void RightDrawingService_DrawingObjectAdded(object sender, EventArgs e)
        {
            try
            {
                AddEmptyObject(RightDrawingService, LeftDrawingService);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        private void LeftDrawingService_DrawingObjectAdded(object sender, EventArgs e)
        {
            try
            {
                AddEmptyObject(LeftDrawingService, RightDrawingService);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        private void AddEmptyObject(DrawingService _drawingService, DrawingService _oppositeDrawingService)
        {
            try
            {
                if (_drawingService.DrawingData.Count > 0 &&
                    (!(_drawingService.DrawingData[_drawingService.DrawingData.Count - 1] is EmptyState) ||
                    _oppositeDrawingService.DrawingData.Count > _drawingService.DrawingData.Count))
                {
                    _drawingService.DrawingData.Add(new EmptyState(_drawingService));

                }

                if (_oppositeDrawingService.DrawingData.Count > 0 &&
                    (!(_oppositeDrawingService.DrawingData[_oppositeDrawingService.DrawingData.Count - 1] is EmptyState) ||
                    _drawingService.DrawingData.Count > _oppositeDrawingService.DrawingData.Count))
                {
                    _oppositeDrawingService.DrawingData.Add(new EmptyState(_oppositeDrawingService));

                }

            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// Event occurs when CurrentDrawing changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightDrawingService_CurrentDrawingChanged(object sender, EventArgs e)
        {
            try
            {
                _bCanComboBoxChangeCurrentDrawing = false;

                SetComboboxTitle(sender);

                SetActiveColor(sender);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
            finally
            {
                _bCanComboBoxChangeCurrentDrawing = true;
            }
        }

        private void SetActiveColor(object _sender)
        {
            DrawingService senderObject = null;
            try
            {
                senderObject = ((DrawingService)_sender);
                if (senderObject.CurrentDrawing.InsertIndex.HasValue)
                {
                    senderObject.RefreshDrawingIndexTarget(senderObject.CurrentDrawing.InsertIndex.Value);
                }
                else
                {
                    senderObject.RefreshDrawingIndexTarget(senderObject.DrawingData.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// Event occurs when CurrentDrawing changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftDrawingService_CurrentDrawingChanged(object sender, EventArgs e)
        {
            try
            {
                _bCanComboBoxChangeCurrentDrawing = false;

                SetComboboxTitle(sender);

                SetActiveColor(sender);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
            finally
            {
                _bCanComboBoxChangeCurrentDrawing = true;
            }
        }

        /// <summary>
        /// sets SelectedItem in combobox
        /// </summary>
        /// <param name="_sender"></param>
        private void SetComboboxTitle(object _sender)
        {
            DrawingService senderObject = null;
            try
            {
                senderObject = ((DrawingService)_sender);
                SelectedComboboxItem = MinutiaeStates.FirstOrDefault(x => x.MinutiaName == senderObject.CurrentDrawing.MinutiaName);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// Load Right image
        /// Fill DrawingData with Empty if amount of objects in DrawingData is different
        /// </summary>
        private void LoadRightImage()
        {
            try
            {
                RightDrawingService.LoadImage();

                FillEmpty(RightDrawingService, LeftDrawingData.Count - RightDrawingData.Count);

                RightDrawingService.RefreshDrawingIndexTarget(null);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// Load Left image
        /// Fill DrawingData with Empty if amount of objects in DrawingData is different
        /// </summary>
        private void LoadLeftImage()
        {
            try
            {
                LeftDrawingService.LoadImage();

                FillEmpty(LeftDrawingService, RightDrawingData.Count - LeftDrawingData.Count);

                LeftDrawingService.RefreshDrawingIndexTarget(null);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// Peform SaveAs dialog window to save data
        /// </summary>
        private void SaveAsClick()
        {
            try
            {
                string leftFileName = LeftDrawingService.BackgroundImage.GetFileName();
                string rightFileName = RightDrawingService.BackgroundImage.GetFileName();

                ExportService.SaveAsFileDialog(LeftDrawingData.ToList(), leftFileName, RightDrawingData.ToList(), rightFileName);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// Methods indicates if MinutiaStatesSelectionChanged method can run
        /// </summary>
        /// <param name="_oSelectedMinutiaState"></param>
        /// <returns></returns>
        private bool CanComboBoxChangeCurrentDrawing(MinutiaState _oSelectedMinutiaState)
        {
            return _bCanComboBoxChangeCurrentDrawing;
        }

        /// <summary>
        /// Initiate new drawing for left and right drawing serivce
        /// </summary>
        /// <param name="_oSelectedMinutiaState"></param>
        private void MinutiaStatesSelectionChanged(MinutiaState _oSelectedMinutiaState)
        {
            try
            {
                LeftDrawingService.CurrentDrawing = MinutiaStateFactory.Create(_oSelectedMinutiaState.Minutia, LeftDrawingService);
                RightDrawingService.CurrentDrawing = MinutiaStateFactory.Create(_oSelectedMinutiaState.Minutia, RightDrawingService);

            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        private void RightDrawingDataChanged(object _sender, NotifyCollectionChangedEventArgs _eventArgs)
        {
            //code for inserting empty minutiae
            //code for asign minutiaID
            ObservableCollection<MinutiaStateBase> senderObject = null;
            try
            {
                senderObject = (ObservableCollection<MinutiaStateBase>)_sender;

                CollectionChangedActions(senderObject, _eventArgs, LeftDrawingService);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        public void LeftDrawingDataChanged(object _sender, NotifyCollectionChangedEventArgs _eventArgs)
        {
            //code for inserting empty minutiae
            //code for asign minutiaID
            ObservableCollection<MinutiaStateBase> senderObject = null;
            try
            {
                senderObject = (ObservableCollection<MinutiaStateBase>)_sender;

                CollectionChangedActions(senderObject, _eventArgs, RightDrawingService);
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// Method executes when DrawingData collection changed
        /// </summary>
        /// <param name="_senderObject"></param>
        /// <param name="_eventArgs"></param>
        /// <param name="_oppositeDrawingService"></param>
        private void CollectionChangedActions(ObservableCollection<MinutiaStateBase> _senderObject, NotifyCollectionChangedEventArgs _eventArgs, DrawingService _oppositeDrawingService)
        {
            if (_eventArgs.Action == NotifyCollectionChangedAction.Add && _senderObject.Count > 0)
            {
                AssignNewIDIfCan(_eventArgs);

                //AddEmptyToOppositeIfCan(_senderObject, _oppositeDrawingService);
            }

            if (_eventArgs.Action == NotifyCollectionChangedAction.Replace)
            {
                AssignIDOnReplace(_senderObject, _eventArgs, _oppositeDrawingService);
            }
        }

        /// <summary>
        /// Assigns id from opposite object or from old object
        /// </summary>
        /// <param name="_senderObject"></param>
        /// <param name="_eventArgs"></param>
        /// <param name="_oppositeDrawingService"></param>
        private void AssignIDOnReplace(ObservableCollection<MinutiaStateBase> _senderObject, NotifyCollectionChangedEventArgs _eventArgs, DrawingService _oppositeDrawingService)
        {
            try
            {
                if (_oppositeDrawingService.DrawingData?.Count > _eventArgs.NewStartingIndex)
                {
                    //get id from opposite drawing object
                    _senderObject[_eventArgs.NewStartingIndex].Id = _oppositeDrawingService.DrawingData[_eventArgs.NewStartingIndex].Id;
                }
                else
                {
                    //get id from old drawing object
                    _senderObject[_eventArgs.NewStartingIndex].Id = ((MinutiaStateBase)_eventArgs.OldItems[0]).Id;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// insert Empty object to opposite Drawing data if count is diferrent
        /// </summary>
        /// <param name="_senderObject"></param>
        /// <param name="_oppositeDrawingService"></param>
        private void AddEmptyToOppositeIfCan(ObservableCollection<MinutiaStateBase> _senderObject, DrawingService _oppositeDrawingService)
        {
            if (_oppositeDrawingService.WriteableBitmap == null)
            {
                return;
            }

            if (_senderObject.Count > _oppositeDrawingService.DrawingData.Count)
            {
                _oppositeDrawingService.AddMinutiaToDrawingData(new EmptyState(_oppositeDrawingService) { Id = _senderObject.LastOrDefault().Id });
            }
        }

        private void FillEmpty(DrawingService _drawingService, int _count)
        {
            try
            {
                if (_count <= 0)
                {
                    return;
                }

                for (int i = 0; i < _count; i++)
                {
                    _drawingService.AddMinutiaToDrawingData(new EmptyState(_drawingService));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }

        /// <summary>
        /// Assigns Guid to Minutia ID is null or empty
        /// </summary>
        /// <param name="_eventArgs"></param>
        private void AssignNewIDIfCan(NotifyCollectionChangedEventArgs _eventArgs)
        {
            foreach (MinutiaStateBase item in _eventArgs.NewItems)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    item.Id = Guid.NewGuid().ToString();
                }
            }
        }

        /// <summary>
        /// Save Data to files named as BackgroundImage file
        /// </summary>
        public void SaveClick()
        {
            try
            {
                //get path to save data as BackgroundImage file name with txt extension
                //string leftPath = Path.ChangeExtension(LeftDrawingService.BackgroundImage.UriSource.AbsolutePath, ".txt");
                //string rightPath = Path.ChangeExtension(RightDrawingService.BackgroundImage.UriSource.AbsolutePath, ".txt");

                //ExportService.SaveTxt(LeftDrawingData.ToList(), leftPath, RightDrawingData.ToList(), rightPath);
                LeftDrawingData[0].WillBeReplaced = true;
            }
            catch (Exception ex)
            {
                Logger.WriteExceptionLog(ex);
            }
        }
    }
}
