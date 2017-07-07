using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Wing.BaseData.TopicBaseData
{
    public class TopicImport
    {
        IDomainSessionFactoryProvider FactoryProvider { get; set; }
        ILogWriter Logger { get; set; }

        public TopicImport()
        {
            FactoryProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            Logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
        }

        public void Import()
        {
            try
            {
                DataTable dtData = new DataTable();

                OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder();
                builder.Provider = "Microsoft.ACE.OLEDB.12.0";
                builder.DataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BaseData\\");
                builder["Extended Properties"] = "text;HDR=YES;FMT=Delimited";

                string file = "MeasureTopics.csv";
                string query = "SELECT * FROM [" + file + "]";

                //create an OleDbDataAdapter to execute the query
                OleDbDataAdapter dAdapter = new OleDbDataAdapter(query, builder.ConnectionString);
                dAdapter.Fill(dtData);
                dAdapter.Dispose();

                foreach (DataRow dr in dtData.Rows)
                {
                    AddToDB(dr["ParentName"].ToString(), dr["Name"].ToString(), dr["Longtitle"].ToString(), dr["description"].ToString());
                }
                dtData.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        private bool AddToDB(string parentName, string topicName, string longTitle, string description)
        {
            try
            {
                using (var session = FactoryProvider.SessionFactory.OpenSession())
                {
                    MeasureService measureSvc = new MeasureService(session);
                    if (topicName == string.Empty)
                    {
                        // Row is a topic category.
                        measureSvc.AddTopicCategory(parentName, longTitle, description);
                    }
                    else
                    {
                        // Row is a topic.
                        // Find the topic topic field.
                        TopicCategory category = measureSvc.GetTopicCategory(parentName);
                        // Check to make sure we found one.
                        if (category == null)
                        {
                            // Create a new Topic topic.
                            // longTitle and description are related to the topic, not the parent topic, so just reuse the name.
                            measureSvc.AddTopicCategory(parentName, parentName, parentName);
                        }
                        Topic topic = measureSvc.AddTopic(category, topicName, longTitle, description);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return false;
            }
        }

    }
}
