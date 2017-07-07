using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Measures.Events;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Events;
using NHibernate.Linq;

namespace Monahrq.Measures.ViewModels
{
    //public class Worker
    //{
    //    private ManageMeasuresViewModel ViewModel { get; set; }

    //    internal readonly BackgroundWorker _ = new BackgroundWorker();

    //    public Worker(ManageMeasuresViewModel viewModel)
    //    {
    //        if (viewModel == null) throw new ArgumentNullException("_vm");

    //        ViewModel = viewModel;
    //        RefreshCommand = new DelegateCommand(ExcecuteRefreshCommand);

    //        _.WorkerReportsProgress = true;
    //        _.DoWork += DoWork;
    //        _.ProgressChanged += ProgressChanged;
    //        _.RunWorkerCompleted += RunWorkerCompleted;
    //    }

    //    public DelegateCommand RefreshCommand { get; set; }

    //    public void LoadData()
    //    {

    //        //if (!ViewModel.UI.IsBusy)
    //        //    ViewModel.UI.IsBusy = true;

    //        if (_.IsBusy != true)
    //        {
    //            _.RunWorkerAsync();
    //        }

    //    }

    //    public void ExcecuteRefreshCommand()
    //    {
    //        LoadData();
    //    }

    //    public void DoWork(object sender, DoWorkEventArgs e)
    //    {
    //        //var worker = sender as BackgroundWorker;
    //        //var topics = Enumerable.Empty<TopicViewModel>();
    //        //var measures = Enumerable.Empty<MeasureModel>();  
    //        //var datasets = Enumerable.Empty<string>();
    //        //var exceptions = new List<Exception>();
    //        //try
    //        //{
    //        //    //using (var session = ViewModel.FactoryProvider.SessionFactory.OpenSession())
    //        //    //{
    //        //    //    topics = session.Query<TopicCategory>().Select(t => new TopicViewModel(t)).ToList();
    //        //    //    var temp = session.Query<Measure>().ToList();
    //        //    //    measures = temp.Select(m => new MeasureModel(m, topics)).ToList();
    //        //    //    datasets = session.Query<Target>().Select(t => t.Name).ToList();
    //        //    //}
    //        //}
    //        //catch (Exception ex)
    //        //{
    //        //    ViewModel.EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
    //        //    exceptions.Add(ex);
    //        //}
    //        //if (worker != null)
    //        //{
    //        //    worker.ReportProgress(100);
    //        //}
    //        //e.Result = new Tuple<IEnumerable<String>,
    //        //    IEnumerable<MeasureModel>,
    //        //    IEnumerable<TopicViewModel>,
    //        //    IEnumerable<Exception>>(datasets, measures, topics, exceptions);

    //    }

    //    public void ProgressChanged(object sender, ProgressChangedEventArgs e)
    //    {
    //        //ViewModel.UI.ProgressPercentage = e.ProgressPercentage;
    //        //ViewModel.Events.GetEvent<ProgressNotificationEvent>().Publish(e.ProgressPercentage);
    //    }

    //    public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    //    {
    //        //    if (e.Error == null && e.Result != null)
    //        //    {
    //        //        var result = e.Result as Tuple<IEnumerable<String>,
    //        //                IEnumerable<MeasureModel>,
    //        //                IEnumerable<TopicViewModel>, 
    //        //                IEnumerable<Exception>>;
    //        //        var temp = new ObservableCollection<string>(result.Item1);
    //        //        temp.Insert(0, BaseTabViewModel.ALL_DATASETS);

    //        //        ViewModel.AvailableDataSets = CollectionViewSource.GetDefaultView(temp) as ListCollectionView;
    //        //        ViewModel.AvailableDataSets.MoveCurrentToFirst();

    //        //        ViewModel.Measures = CollectionViewSource.GetDefaultView(
    //        //            new ObservableCollection<MeasureModel>(result.Item2)) as ListCollectionView;


    //        //        ViewModel.Measures.MoveCurrentToFirst();

    //        //        ViewModel.AvailableTopics =
    //        //            CollectionViewSource.GetDefaultView(
    //        //                new ObservableCollection<TopicViewModel>(result.Item3.OrderBy(a => a.TopicName))) as ListCollectionView;
    //        //        ViewModel.AvailableTopics.MoveCurrentToFirst();
    //        //        ViewModel.UI.Errors = new ObservableCollection<Exception>(result.Item4);
    //        //    }
    //        //    else
    //        //    {
    //        //        ViewModel.UI.Errors.Add(e.Error);
    //        //        ViewModel.EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.Error);
    //        //    }


    //        //    ViewModel.UI.ProgressPercentage = 100;
    //        //    ViewModel.EventAggregator.GetEvent<ProgressNotificationEvent>().Publish(ViewModel.UI.ProgressPercentage);
    //        //    ViewModel.UI.IsBusy = false;
    //        //}
    //    }
    //}
}