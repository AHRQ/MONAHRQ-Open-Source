using System;
using System.ComponentModel.Composition;
using System.Linq;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using NHibernate;

namespace Monahrq.Wing.Physician.Physicians
{
    [Export(typeof(ITargetDeletionVisitor))]
    public class PhysicianTargetDeletionVistor : ITargetDeletionVisitor 
    {
        public string TargetType
        {
            get { return PhysicianConstants.WING_TARGET_NAME; }
        }

        public int Order { get { return 999; } }

        //[Import]
        //public IDomainSessionFactoryProvider DataProvider { get; set; }

        //[Import(LogNames.Session)]
        //ILogWriter Logger { get; set; }

        public void Visit(Monahrq.Infrastructure.Entities.Domain.IEntity entity, VisitorOptions options /*NHibernate.ISession session*/)
        {
            if (options == null || options.DataProvider == null || entity == null) return;
            var dataset = entity as Dataset;
            if (dataset == null) return;
            if (dataset.ContentType == null || !dataset.ContentType.Name.EqualsIgnoreCase(TargetType)) return;


            var statesList = dataset.ProviderStates.Split(',').ToList();
            var itemCounter = 0;
            var selectStatesSummary = string.Empty;

            using (var sess = options.DataProvider.SessionFactory.OpenSession())
            {
                using (var trans = sess.BeginTransaction())
                {
                    try
                    {

                        foreach (var state in statesList.ToList())
                        {
                            var query = string.Format("if exists (select distinct wd.[Id] from [Wings_Datasets] wd inner join [Wings_Targets] wt on wt.[Id] = wd.[ContentType_Id] Where wd.[Id]!={0} and UPPER(wt.Name) = UPPER('{1}') ", dataset.Id, dataset.ContentType.Name);
                            query += " and ( ";

                            //if (itemCounter > 0)
                            //    query += " or ";

                            query += Environment.NewLine + string.Format(" charindex('{0}',wd.[ProviderStates],0) > 0", state);


                            query += " ))";
                            query += Environment.NewLine + @"begin
	select 1 as result;
end
Else
begin
	select 0 as result;
end";
                            itemCounter++;

                            if (sess.CreateSQLQuery(query).AddScalar("result", NHibernateUtil.Int32).List<int>().Single() == 1) continue;

                            //foreach (var state in statesList.ToList())
                            //{
                            //selectStatesSummary += string.Format("'{0}',", state);
                            selectStatesSummary += string.Format("'{0}'", state);
                            //}
                            //if (selectStatesSummary.EndsWith(","))
                            //    selectStatesSummary = selectStatesSummary.SubStrBeforeLast(",");

//                            sess.CreateSQLQuery(string.Format(@"-- Medical Practice Addresses
//                                                DELETE FROM [dbo].[Addresses]
//                                                    WHERE [AddressType] = 'MEDICALPRACTICE' 
//                                                    AND [MedicalPractice_Id] in (SELECT DISTINCT mp.[Id] FROM [dbo].[MedicalPractices] mp WHERE mp.[State] in ({0}) AND mp.[IsEdited] = 0);
//                                                    --AND [State] in ({0});
//                                                -- Physician Addresses
//                                                DELETE FROM [dbo].[Addresses]
//                                                    WHERE [AddressType] = 'PHYSICIAN' 
//                                                    AND [Physician_Id] in (SELECT DISTINCT p.[Id] FROM [dbo].[Physicians] p WHERE p.[States] in ({0}) AND p.[IsEdited] = 0);
//                                                    --AND [State] in ({0});
//                                                -- Physician Affiliated Hospitals
//                                                DELETE FROM [dbo].[Physicians_AffiliatedHospitals]
//                                                    WHERE [Physician_Id] in (SELECT DISTINCT p.[Id] FROM [dbo].[Physicians] p WHERE p.[States] in ({0}) AND p.[IsEdited] = 0);
//                                                -- Physician Medical Practices
//                                                DELETE FROM [dbo].[Physicians_MedicalPractices]
//                                                    WHERE [Physician_Id] in (SELECT DISTINCT p.[Id] FROM [dbo].[Physicians] p WHERE p.[States] in ({0}) AND p.[IsEdited] = 0)
//                                                    OR [MedicalPractice_Id] in (SELECT DISTINCT mp.[Id] FROM [dbo].[MedicalPractices] mp WHERE mp.[State] in ({0}) AND mp.[IsEdited] = 0);
//                                                DELETE FROM [dbo].[Physicians_MedicalPractices]
//                                                    WHERE [Physician_Id] is null OR [MedicalPractice_Id] is null;
//                                                -- Medical Practices
//                                                DELETE FROM [dbo].[MedicalPractices] 
//                                                    WHERE [State] in ({0}) AND [IsEdited] = 0;
//                                                -- Physicians
//                                                DELETE FROM [dbo].[Physicians] 
//                                                    WHERE [States] in ({0}) AND [IsEdited] = 0;", selectStatesSummary))
//                                 .SetTimeout(10000)
//                                 .ExecuteUpdate();

                            sess.CreateSQLQuery(string.Format(@"-- Medical Practice Addresses
                                                DELETE FROM [dbo].[Addresses]
                                                    WHERE UPPER([AddressType]) = 'MEDICALPRACTICE' 
                                                    AND [MedicalPractice_Id] in (SELECT DISTINCT mp.[Id] FROM [dbo].[MedicalPractices] mp WHERE mp.[State] in ('{0}') AND mp.[IsEdited] = 0);
                                                    --AND [State] in ({0});

                                                -- Physician Addresses
                                                DELETE FROM [dbo].[Addresses]
                                                    WHERE UPPER([AddressType]) = 'PHYSICIAN' 
                                                    AND [Physician_Id] in (SELECT DISTINCT p.[Id] FROM [dbo].[Physicians] p WHERE p.[States] like '%{0}%' AND p.[IsEdited] = 0);
                                                    --AND [State] in ({0});

                                                -- Physician Affiliated Hospitals
                                                DELETE FROM [dbo].[Physicians_AffiliatedHospitals]
                                                    WHERE [Physician_Id] in (SELECT DISTINCT p.[Id] FROM [dbo].[Physicians] p WHERE p.[States] like '%{0}%' AND p.[IsEdited] = 0);

                                                -- Physician Medical Practices
                                                DELETE FROM [dbo].[Physicians_MedicalPractices]
                                                    WHERE [Physician_Id] in (SELECT DISTINCT p.[Id] FROM [dbo].[Physicians] p WHERE p.[States] like '%{0}%' AND p.[IsEdited] = 0)
                                                    OR [MedicalPractice_Id] in (SELECT DISTINCT mp.[Id] FROM [dbo].[MedicalPractices] mp WHERE mp.[State] in ('{0}') AND mp.[IsEdited] = 0);
                                                    
                                                DELETE FROM [dbo].[Physicians_MedicalPractices]
                                                    WHERE [Physician_Id] is null
                                                     OR [MedicalPractice_Id] is null;
                                                
                                                -- Medical Practices
                                                DELETE FROM [dbo].[MedicalPractices] 
                                                    WHERE [State] = '{0}' AND [IsEdited] = 0;

                                                -- Physicians
                                                DELETE FROM [dbo].[Physicians] 
                                                    WHERE [States] like '%{0}%' AND [IsEdited] = 0;", state))
                                     .SetTimeout(30000)
                                     .ExecuteUpdate();
                        }

                        trans.Commit();
                    }
                    catch(Exception exc)
                    {
                        trans.Rollback();
                        options.Logger.Write(exc);
                    }
                }
            }
        }
    }
}
