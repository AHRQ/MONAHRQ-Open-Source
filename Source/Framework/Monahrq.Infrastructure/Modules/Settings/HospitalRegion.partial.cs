using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Extensions;
using Monahrq.Infrastructure.Services.Hospitals;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.Sdk.Modules.Settings
{
    [DebuggerStepThrough]
    public sealed partial class HospitalRegion : IHospitalRegion 
    {
        IHospitalRegistryService Service { get; set; }

        public HospitalRegion()
            : this(ServiceLocator.Current.GetInstance<IHospitalRegistryService>())
        {
        }

        public HospitalRegion(IHospitalRegistryService service )
        {
            Service = service;

            this.PropertyChanged += (o, e) =>
            {
                if (string.Equals(e.PropertyName, "DefaultStates", StringComparison.OrdinalIgnoreCase))
                {
                    InitLazySelectedStates();
                }
            };

            if (DefaultStates == null)
            {
                DefaultStates = new StringCollection();
            }

            //if (_LazySelectedStates == null)
            //{
            InitLazySelectedStates();
            //}
        }

        public Type SelectedRegionType
        {
            get
            {
                return string.IsNullOrWhiteSpace(DefaultRegionTypeName)
                    ? typeof(object)
                    : Type.GetType(DefaultRegionTypeName);
            }
            set
            {
                DefaultRegionTypeName = value.AssemblyQualifiedName;
            }
        }

        void InitLazySelectedStates()
        {
            if ((_lazySelectedStates == null || !_lazySelectedStates.Any()))
            {
                var stateservice = ServiceLocator.Current.GetInstance<IHospitalRegistryService>();
                _lazySelectedStates = stateservice.GetStates(DefaultStates.OfType<string>().ToArray(), false).ToList();
            }
            if(DefaultStates != null && !_lazySelectedStates.Any(s => s.Abbreviation.In(DefaultStates.OfType<string>().ToList())))
            {
                var stateservice = ServiceLocator.Current.GetInstance<IHospitalRegistryService>();
                _lazySelectedStates = stateservice.GetStates(DefaultStates.OfType<string>().ToArray(), false).ToList();
            }
        }

        private IEnumerable<State> _lazySelectedStates = new List<State>();
        public IEnumerable<State> SelectedStates
        {
            get
            {
                var selectedStates = _lazySelectedStates.ToList();
                MonahrqContext.ReportingStatesContext = selectedStates;
                return selectedStates;
            }
            set
            {
                var selectedStates = value.ToList();
                MonahrqContext.ReportingStatesContext = selectedStates;
                _lazySelectedStates = selectedStates;
                var collection = new StringCollection();
                collection.AddRange(selectedStates.Select(state => state.Abbreviation).ToArray());
                DefaultStates = collection;
            }
        }

        public bool IsDefined
        {
            get
            {
                return SelectedRegionType != typeof(object) && DefaultStates.Count > 0;
            }
        }

        public override void Save()
        {
            base.Save();

            ServiceLocator.Current.GetInstance<IEventAggregator>()
                                  .GetEvent<HospitalRegionSettingsSavedEvent>()
                                  .Publish(this);
        }

        public static bool AreCongruent(HospitalRegion thisOne, HospitalRegion thatOne)
        {
            if (thisOne == null || thatOne == null) return false;
            if (thisOne.SelectedRegionType != thatOne.SelectedRegionType) return false;
            if (thisOne.SelectedStates.Count() != thatOne.SelectedStates.Count()) return false;

            var notInThis = thisOne.SelectedStates.Any(state => !thatOne.SelectedStates.Contains(state));

            return !notInThis && thatOne.SelectedStates.Any(state => !thisOne.SelectedStates.Contains(state));
        }

        public void AddState(State state)
        {
            if (state == null) return;

            if (!DefaultStates.Contains(state.Abbreviation))
                DefaultStates.Add(state.Abbreviation);

            this.PropertyChanged += (o, e) =>
            {
                if (string.Equals(e.PropertyName, "DefaultStates", StringComparison.OrdinalIgnoreCase))
                {
                    InitLazySelectedStates();
                }
            };
        }

        public void AddState(string state)
        {
            if (string.IsNullOrEmpty(state)) return;

            if (!DefaultStates.Contains(state))
                DefaultStates.Add(state);

            this.PropertyChanged += (o, e) =>
            {
                if (string.Equals(e.PropertyName, "DefaultStates", StringComparison.OrdinalIgnoreCase))
                {
                    InitLazySelectedStates();
                }
            };
        }

        public void AddStates(IEnumerable<string> states)
        {
            if (states == null || !states.Any()) return;

            foreach (var state in states)
            {
                if (!DefaultStates.Contains(state))
                    DefaultStates.Add(state);
            }

            this.PropertyChanged += (o, e) =>
            {
                if (string.Equals(e.PropertyName, "DefaultStates", StringComparison.OrdinalIgnoreCase))
                {
                    InitLazySelectedStates();
                }
            };
        }

        public void Remove(State state)
        {
            if (state == null) return;

            if (DefaultStates.Contains(state.Abbreviation))
                DefaultStates.Remove(state.Abbreviation);

            this.PropertyChanged += (o, e) =>
            {
                if (string.Equals(e.PropertyName, "DefaultStates", StringComparison.OrdinalIgnoreCase))
                {
                    InitLazySelectedStates();
                }
            };
        }
    }
}