using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Monahrq.Theme.Controls.Wizard.Models
{



	/// <summary>
	/// Collection of steps.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.IStepCollection{TValue}" />
	public class StepCollection<TValue>: IStepCollection<TValue>
        where TValue: new()
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="StepCollection{TValue}"/> class.
		/// </summary>
		public StepCollection()
        {
            Context = new TValue();
            Collection =  new Dictionary<StepGroup, List<CompleteStep<TValue>>>();
        }

		/// <summary>
		/// Gets the context.
		/// </summary>
		/// <value>
		/// The context.
		/// </value>
		public TValue Context { get; private set; }
		/// <summary>
		/// Gets or sets the collection.
		/// </summary>
		/// <value>
		/// The collection.
		/// </value>
		public Dictionary<StepGroup, List<CompleteStep<TValue>>> Collection
        {
            get;
            set;
        }
		/// <summary>
		/// Adds the group of steps.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="steps">The steps.</param>
		public void AddGroupOfSteps(StepGroup group, List<CompleteStep<TValue>> steps)
        {
            foreach (var step in steps)
            {
             
                step.GroupName = group.DisplayName;
            }

            Collection.Add(group,steps);
        }

		/// <summary>
		/// Gets all steps.
		/// </summary>
		/// <returns></returns>
		public List<CompleteStep<TValue>> GetAllSteps()
        {
            var steplist = new List<CompleteStep<TValue>>();

            foreach (var group in Collection)
            {
                steplist.AddRange(@group.Value);
            }

            return steplist;
        }

		/// <summary>
		/// Gets the groups.
		/// </summary>
		/// <returns></returns>
		public List<StepGroup> GetGroups()
        {
            return Collection.Select(@group => @group.Key).ToList();
        }

		/// <summary>
		/// Gets the type of the context.
		/// </summary>
		/// <value>
		/// The type of the context.
		/// </value>
		public Type ContextType
        {
            get { return typeof(TValue); }
        }


		/// <summary>
		/// Gets the collection.
		/// </summary>
		/// <value>
		/// The collection.
		/// </value>
		IDictionary IStepCollection.Collection
        {
            get
            {
                return this.Collection;
            }
        }
    }
}
