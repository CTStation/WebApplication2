﻿#region Copyright
// ----------------------- IMPORTANT - READ CAREFULLY: COPYRIGHT NOTICE -------------------
// -- THIS SOFTWARE IS THE PROPERTY OF CTStation S.A.S. IN ANY COUNTRY                   --
// -- (WWW.CTSTATION.NET). ANY COPY, CHANGE OR DERIVATIVE WORK                           --
// -- IS SUBJECT TO CTSTATION S.A.S.’S PRIOR WRITTEN CONSENT.                            --
// -- THIS SOFTWARE IS REGISTERED TO THE FRENCH ANTI-PIRACY AGENCY (APP).                --
// -- COPYRIGHT 2020-01 CTSTATTION S.A.S. – ALL RIGHTS RESERVED.                         --
// ----------------------------------------------------------------------------------------
#endregion
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using log4net;
using CTREPORTINGMODULELib;
using CTCORELib;
using CTCLIENTSERVERLib;
using CTSWeb.Util;


namespace CTSWeb.Models
{
    public class ReportingLight : ManagedObjectWithDescAndSecurity // Inherits ID and Name
    {
        private static readonly ILog _oLog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static ReportingLight()
        {
            Manager.Register<ReportingLight>((int)CtReportingManagers.CT_REPORTING_MANAGER, (int)(LanguageMasks.ShortDesc | LanguageMasks.LongDesc | LanguageMasks.Comment)); // TranslatableField.None
        }

        // Argument-less constructor
        public ReportingLight() { }

        public string Phase;
        public string UpdatePeriod;
        public string FrameworkVersion;

        public DateTime ReportingStartDate;
        public DateTime ReportingEndDate;

        public override void ReadFrom(ICtObject roObject, Language roLang)
        {
            base.ReadFrom(roObject, roLang);

            ICtReporting reporting = (ICtReporting)roObject;
            Phase = reporting.Phase.Name;
            UpdatePeriod = reporting.UpdatePeriod.Name;
            FrameworkVersion = reporting.FrameworkVersion.Name;
            ReportingStartDate = reporting.ReportingStartDate;
            ReportingEndDate = reporting.ReportingEndDate;
        }

        public override void WriteInto(ICtObject roObject)
        {
            // Not used
            _oLog.Debug($"Writen  {this.GetType().Name} {Name}");
        }
    }


    public class Reporting : ReportingLight
    {
        private static readonly ILog _oLog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static Reporting() 
        {
            Manager.Register<Reporting>((int)CtReportingManagers.CT_REPORTING_MANAGER, (int)(LanguageMasks.ShortDesc | LanguageMasks.LongDesc | LanguageMasks.Comment)); // TranslatableField.None
        }
        
        // Argument-less constructor
        public Reporting() { }


        public int Status;
        public Framework Framework;
        public ExchangeRate ExchangeRate;
        public ExchangeRateUpdatePeriod ExchangeRateUpdatePeriod;
        public ExchangeRateVersion ExchangeRateVersion;
        public ExchangeRateType ExchangeRateType;
        public List<Period> Periods;
        public uint ReportingModifyComment;
        public DateTime ReportingHierarchyDate;
        public List<RelatedEntityReportingCollection> RelatedEntityReportingCollection;


        public override void ReadFrom(ICtObject roObject, Language roLang)
        {
            base.ReadFrom(roObject, roLang);

            ICtReporting reporting = (ICtReporting)roObject;

            Status = (int)reporting.Status;
            Framework = new Framework(reporting.Framework);
            ExchangeRate = new ExchangeRate(reporting.ExchangeRate);
            ExchangeRateUpdatePeriod = new ExchangeRateUpdatePeriod(reporting.ExchangeRateUpdatePeriod);
            ExchangeRateVersion = new ExchangeRateVersion(reporting.ExchangeRateVersion);
            RelatedEntityReportingCollection = new List<RelatedEntityReportingCollection>();

            if (reporting.ExchangeRateType != null)
            {
                ExchangeRateType = new ExchangeRateType()
                {

                    ID = reporting.ExchangeRateType.ID,
                    Name = reporting.ExchangeRateType.Name
                };
            }
            else
            {
                ExchangeRateType = new ExchangeRateType();
            }

            foreach (ICtEntityReporting reporting1 in reporting.RelatedEntityReportingCollection)
            {
                RelatedEntityReportingCollection.Add(new RelatedEntityReportingCollection(reporting1));
            }

            Periods = new List<Period>();
            foreach (ICtRefValue period in reporting.Periods)
            {
                Periods.Add(new Period() { ID = period.ID, Name = period.Name });
            }
            ReportingModifyComment = reporting.ReportingModifyComment;
            ReportingHierarchyDate = reporting.ReportingHierarchyDate;
            _oLog.Debug($"Read {Phase} - {UpdatePeriod}");
        }

        public override void WriteInto(ICtObject roObject)
        {
            base.WriteInto(roObject);

            // To do
            _oLog.Debug($"Writen {this.GetType().Name} {Phase} - {UpdatePeriod}");
        }

        public static List<Reporting> LoadFromDataSet(DataSet voData, Context voContext, MessageList roMessages)
        {
            IControl oCtrl = new ControlColumnsExist() { TableName = "Table", RequiredColumns = new List<string> { "Phase", "UpdatePeriod", "FrameworkVersion" } };
            if (oCtrl.Pass(voData, roMessages))
            {
                HashSet<int> oInvalidRows = new HashSet<int>();
                new ControlValidateColumn("Table", "Phase", voContext.GetRefValues("Phase")).Pass(voData, roMessages, oInvalidRows, true, null);
                new ControlValidateColumn("Table", "UpdatePeriod", Context.GetPeriodValidator).Pass(voData, roMessages, oInvalidRows, true, null);
                new ControlValidateColumn("Table", "FrameworkVersion", voContext.GetRefValues("FrameworkVersion")).Pass(voData, roMessages, oInvalidRows, true, null);
            }
            return new List<Reporting>();
        }
    }
}



