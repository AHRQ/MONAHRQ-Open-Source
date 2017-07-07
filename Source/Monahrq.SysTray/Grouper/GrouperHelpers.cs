using Monahrq.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.SysTray.Grouper
{
    /// <summary>
    /// Class to implement dicionary like feature for the Grouper
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.Int32, System.String}" />
    public class GrouperInputDictionary_String : Dictionary<int, string>
    {
        private int _upperIndex;
        private int _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperInputDictionary_String"/> class.
        /// </summary>
        /// <param name="upperIndex">Index of the upper.</param>
        /// <param name="length">The length.</param>
        public GrouperInputDictionary_String(int upperIndex, int length)
        {
            this._upperIndex = upperIndex;
            this._length = length;
            for (int i = 1; i <= upperIndex; i++)
            {
                this.Add(i, null);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">
        /// Index out of range.
        /// or
        /// Invalid string length.
        /// or
        /// Index out of range.
        /// </exception>
        public new string this[int index]
        {
            get
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    {
                        if (this.ContainsKey(index))
                        {
                            return this.FirstOrDefault(v => v.Key.Equals(index)).Value;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }

            set
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    if(value.Length <= _length)
                    {
                        this[index] = value;
                    }
                    else
                    {
                        throw new GrouperRecordException("Invalid string length.");
                    }
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }
        }
    }

    /// <summary>
    /// Class to implement dicionary like feature for the Grouper
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.Int32, System.String}" />
    public class GrouperOutputDictionary_String : Dictionary<int, string>
    {
        private IGrouperOutputRecord _groupRecord;
        private int _upperIndex;
        private int _basePosition;
        private int _length;
        private int _fieldWidth;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperOutputDictionary_String"/> class.
        /// </summary>
        /// <param name="groupRecord">The group record.</param>
        /// <param name="upperIndex">Index of the upper.</param>
        /// <param name="basePosition">The base position.</param>
        /// <param name="length">The length.</param>
        /// <param name="fieldWidth">Width of the field.</param>
        public GrouperOutputDictionary_String(IGrouperOutputRecord groupRecord, int upperIndex, int basePosition, int length, int fieldWidth)
        {
            this._groupRecord = groupRecord;
            this._upperIndex = upperIndex;
            this._basePosition = basePosition;
            this._length = length;
            this._fieldWidth = fieldWidth;
            for (int i = 1; i <= upperIndex; i++)
            {
                this.Add(i, null);
            }
        }

        /// <summary>
        /// Gets the <see cref="System.String"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">
        /// Invalid grouper output record.
        /// or
        /// Index out of range.
        /// </exception>
        public new string this[int index]
        {
            get
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    {
                        if (_groupRecord.OutputRecordValid)
                        {
                            return _groupRecord.OutputRecord.Substring((_basePosition + ((index - 1) * _fieldWidth)), _length).Trim();
                        }
                        else
                        {
                            throw new GrouperRecordException("Invalid grouper output record.");
                        }
                    }
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }
        }
    }

    /// <summary>
    /// Class to implement dicionary like feature for the Grouper.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.Int32, System.Int32?}" />
    public class GrouperInputDictionary_Int : Dictionary<int, int?>
    {
        /// <summary>
        /// The upper index
        /// </summary>
        private int _upperIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperInputDictionary_Int"/> class.
        /// </summary>
        /// <param name="upperIndex">Index of the upper.</param>
        public GrouperInputDictionary_Int(int upperIndex)
        {
            this._upperIndex = upperIndex;
            for (int i = 1; i <= upperIndex; i++)
            {
                this.Add(i, null);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Nullable{System.Int32}"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Nullable{System.Int32}"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">
        /// Index out of range.
        /// or
        /// Index out of range.
        /// </exception>
        public new int? this[int index]
        {
            get
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    {
                        if (this.ContainsKey(index))
                        {
                            return this.FirstOrDefault(v => v.Key.Equals(index)).Value;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }

            set
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    this[index] = value;
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }
        }
    }

    /// <summary>
    ///  Class to implement dicionary like feature for the Grouper.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.Int32, System.Int32?}" />
    public class GrouperOutputDictionary_Int : Dictionary<int, int?>
    {
        private IGrouperOutputRecord _groupRecord;
        private int _upperIndex;
        private int _basePosition;
        private int _length;
        private int _fieldWidth;
        private int _tempInt;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperOutputDictionary_Int"/> class.
        /// </summary>
        /// <param name="groupRecord">The group record.</param>
        /// <param name="upperIndex">Index of the upper.</param>
        /// <param name="basePosition">The base position.</param>
        /// <param name="length">The length.</param>
        /// <param name="fieldWidth">Width of the field.</param>
        public GrouperOutputDictionary_Int(IGrouperOutputRecord groupRecord, int upperIndex, int basePosition, int length, int fieldWidth)
        {
            this._groupRecord = groupRecord;
            this._upperIndex = upperIndex;
            this._basePosition = basePosition;
            this._length = length;
            this._fieldWidth = fieldWidth;
            for (int i = 1; i <= upperIndex; i++)
            {
                this.Add(i, null);
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Nullable{System.Int32}"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Nullable{System.Int32}"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">
        /// Invalid grouper output record.
        /// or
        /// Index out of range.
        /// </exception>
        public new int? this[int index]
        {
            get
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    {
                        if (_groupRecord.OutputRecordValid)
                        {
                            if (int.TryParse(_groupRecord.OutputRecord.Substring((_basePosition + ((index - 1) * _fieldWidth)), _length), out _tempInt))
                            {
                                return _tempInt;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            throw new GrouperRecordException("Invalid grouper output record.");
                        }
                    }
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }
        }
    }

    /// <summary>
    /// Class to implement dicionary like feature for the Grouper.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.Int32, System.DateTime?}" />
    public class GrouperInputDictionary_Date : Dictionary<int, DateTime?>
    {
        private int _upperIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperInputDictionary_Date"/> class.
        /// </summary>
        /// <param name="upperIndex">Index of the upper.</param>
        public GrouperInputDictionary_Date(int upperIndex)
        {
            this._upperIndex = upperIndex;
            for (int i = 1; i <= upperIndex; i++)
            {
                this.Add(i, null);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Nullable{DateTime}"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Nullable{DateTime}"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">
        /// Index out of range.
        /// or
        /// Index out of range.
        /// </exception>
        public new DateTime? this[int index]
        {
            get
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    {
                        if (this.ContainsKey(index))
                        {
                            return this.FirstOrDefault(v => v.Key.Equals(index)).Value;
                        }
                        else
                        {
                            // TODO: Raise error?
                            return null;
                        }
                    }
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }

            set
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    this[index] = value;
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }
        }
    }

    /// <summary>
    /// Class to implement dicionary like feature for the Grouper
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.Int32, System.DateTime?}" />
    public class GrouperOutputDictionary_Date : Dictionary<int, DateTime?>
    {
        private IGrouperOutputRecord _groupRecord;
        private int _upperIndex;
        private int _basePosition;
        private int _length;
        private int _fieldWidth;
        private DateTime _tempDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperOutputDictionary_Date"/> class.
        /// </summary>
        /// <param name="groupRecord">The group record.</param>
        /// <param name="upperIndex">Index of the upper.</param>
        /// <param name="basePosition">The base position.</param>
        /// <param name="length">The length.</param>
        /// <param name="fieldWidth">Width of the field.</param>
        public GrouperOutputDictionary_Date(IGrouperOutputRecord groupRecord, int upperIndex, int basePosition, int length, int fieldWidth)
        {
            this._groupRecord = groupRecord;
            this._upperIndex = upperIndex;
            this._basePosition = basePosition;
            this._length = length;
            this._fieldWidth = fieldWidth;
            for (int i = 1; i <= upperIndex; i++)
            {
                this.Add(i, null);
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Nullable{DateTime}"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Nullable{DateTime}"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">
        /// Invalid grouper output record.
        /// or
        /// Index out of range.
        /// </exception>
        public new DateTime? this[int index]
        {
            get
            {
                if (index >= 1 && index <= _upperIndex)
                {
                    {
                        if (_groupRecord.OutputRecordValid)
                        {
                            if (DateTime.TryParse(_groupRecord.OutputRecord.Substring((_basePosition + ((index - 1) * _fieldWidth)), _length), out _tempDate))
                            {
                                return _tempDate;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            throw new GrouperRecordException("Invalid grouper output record.");
                        }
                    }
                }
                else
                {
                    throw new GrouperRecordException("Index out of range.");
                }
            }
        }
    }


}
