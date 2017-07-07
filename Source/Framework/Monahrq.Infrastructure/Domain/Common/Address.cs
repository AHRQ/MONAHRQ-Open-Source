using System;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Common
{
    [Serializable, ImplementPropertyChanged, EntityTableName("Addresses")]
    public abstract class Address : Entity<int>
    {
        public bool IsSelected { get; set; }

        public string MedicalPracticeName { get; set; }

        [Required(ErrorMessage = @"Please enter a valid street.")]
        public virtual string Line1 { get; set; }

        public virtual string Line2 { get; set; }

        public virtual string Line3 { get; set; }

        public virtual string City { get; set; }

        public virtual string State { get; set; }

        [RegularExpression(@"^\d{5}(?:\d{4})?$", ErrorMessage = @"Please enter a valid zip code.")]
        public virtual string ZipCode { get; set; }

        internal virtual int Index { get; set; }
    }
}
