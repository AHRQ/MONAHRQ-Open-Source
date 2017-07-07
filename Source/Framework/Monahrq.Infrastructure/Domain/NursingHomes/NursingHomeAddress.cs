//using System;
//using Monahrq.Infrastructure.Domain.Common;
//using PropertyChanged;

//namespace Monahrq.Infrastructure.Domain.NursingHomes
//{
//    [Serializable, 
//     ImplementPropertyChanged]
//    public class NursingHomeAddress : Address
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="NursingHomeAddress"/> class.
//        /// </summary>
//        public NursingHomeAddress() {}
//        /// <summary>
//        /// Initializes a new instance of the <see cref="NursingHomeAddress"/> class.
//        /// </summary>
//        /// <param name="nursingHome">The nursing home.</param>
//        public NursingHomeAddress(NursingHome nursingHome)
//        {
//            NursingHome = nursingHome;
//        }

//        /// <summary>
//        /// Gets or sets the nursing home.
//        /// </summary>
//        /// <value>
//        /// The nursing home.
//        /// </value>
//        public virtual NursingHome NursingHome { get; set; }
//    }
//}