using System;
using System.Globalization;
using log4net.Layout;
using log4net.Layout.Pattern;
using log4net.Util;

namespace Monahrq.Sdk.Logging
{
    /// <summary>
    /// The custom log4net color pattern converter.
    /// </summary>
    /// <seealso cref="log4net.Layout.Pattern.PatternLayoutConverter" />
    public class ColorPatternConverter : PatternLayoutConverter
    {
        ///// <summary>
        ///// If Rtf is needed, this method can be used to color the message according to log type.
        ///// </summary>
        //protected override void Convert(System.IO.TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
        //{
        //    var msg = string.Format("{0} [{1}] {2} {3} - {4}", loggingEvent.TimeStamp.ToString("yyyy-MM-dd hh:mm:ss tt"), loggingEvent.ThreadName, loggingEvent.Level, loggingEvent.LoggerName, loggingEvent.RenderedMessage);
        //    var color = "\\par0";
        //    switch (loggingEvent.Level.Name)
        //    {
        //        case "EXCEPTION":
        //            color += "\\cf3";
        //            break;
        //        case "WARN":
        //        case "DEBUG":
        //            color += "\\cf2";
        //            break;
        //        case "INFO":
        //            color += "\\cf1";
        //            break;
        //        default:
        //            color += "\\cf1";
        //            break;
        //    }

        //    writer.Write("{0}{2}{1}{2}", color, msg, Environment.NewLine);
        //}

        /// <summary>
        /// Derived pattern converters must override this method in order to
        /// convert conversion specifiers in the correct way.
        /// </summary>
        /// <param name="writer"><see cref="T:System.IO.TextWriter" /> that will receive the formatted result.</param>
        /// <param name="loggingEvent">The <see cref="T:log4net.Core.LoggingEvent" /> on which the pattern converter should be executed.</param>
        protected override void Convert(System.IO.TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
        {
            var msg = string.Format("{0} [{1}] {2} {3} - {4}", loggingEvent.TimeStamp.ToString("yyyy-MM-dd hh:mm:ss tt"), loggingEvent.ThreadName, loggingEvent.Level, loggingEvent.LoggerName, loggingEvent.RenderedMessage);
            var color = "";
            switch (loggingEvent.Level.Name)
            {
                case "EXCEPTION":
                case "ERROR":
                    color += "red";
                    break;
                case "WARN":
                case "DEBUG":
                    color += "orange";
                    break;
                case "INFO":
                    color += "black";
                    break;
                default:
                    color += "black";
                    break;
            }

            writer.Write("<p style='color:{0};'>{1}</p>{2}", color, msg, Environment.NewLine);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="log4net.Layout.PatternLayout" />
    public class CustomPatternLayout : PatternLayout
    {
        /// <summary>
        /// the head of the pattern converter chain
        /// </summary>
        private PatternConverter m_head;


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPatternLayout"/> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default pattern just produces the application supplied message.
        /// </para>
        /// <para>
        /// Note to Inheritors: This constructor calls the virtual method
        /// <see cref="M:log4net.Layout.PatternLayout.CreatePatternParser(System.String)" />. If you override this method be
        /// aware that it will be called before your is called constructor.
        /// </para>
        /// <para>
        /// As per the <see cref="T:log4net.Core.IOptionHandler" /> contract the <see cref="M:log4net.Layout.PatternLayout.ActivateOptions" />
        /// method must be called after the properties on this object have been
        /// configured.
        /// </para>
        /// </remarks>
        public CustomPatternLayout()
            : base()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            AddConverter(new ConverterInfo() { Name = "custom", Type = typeof(ColorPatternConverter) });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPatternLayout"/> class.
        /// </summary>
        /// <param name="pattern">the pattern to use</param>
        /// <remarks>
        /// <para>
        /// Note to Inheritors: This constructor calls the virtual method
        /// <see cref="M:log4net.Layout.PatternLayout.CreatePatternParser(System.String)" />. If you override this method be
        /// aware that it will be called before your is called constructor.
        /// </para>
        /// <para>
        /// When using this constructor the <see cref="M:log4net.Layout.PatternLayout.ActivateOptions" /> method
        /// need not be called. This may not be the case when using a subclass.
        /// </para>
        /// </remarks>
        public CustomPatternLayout(string pattern)
            : base(pattern)
        {
            ConversionPattern = pattern;
            Initialize();
        }

        /// <summary>
        /// Create the pattern parser instance
        /// </summary>
        /// <param name="pattern">the pattern to parse</param>
        /// <returns>
        /// The <see cref="T:log4net.Util.PatternParser" /> that will format the event
        /// </returns>
        /// <remarks>
        /// Creates the <see cref="T:log4net.Util.PatternParser" /> used to parse the conversion string. Sets the
        /// global and instance rules on the <see cref="T:log4net.Util.PatternParser" />.
        /// </remarks>
        protected override PatternParser CreatePatternParser(string pattern)
        {
            var parser = base.CreatePatternParser(pattern);

            parser.PatternConverters["custom"] = new ConverterInfo()
            {
                Name = "custom",
                Type = typeof(ColorPatternConverter)
            };

            return parser;
        }
    }
}
