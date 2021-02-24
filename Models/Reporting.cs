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
        // TODO use attribute rather than a full field
        public static bool _bDontSaveName = true;

        static ReportingLight()
        {
            Manager.Register<ReportingLight>((int)CtReportingManagers.CT_REPORTING_MANAGER, (int)LanguageMasks.LongDesc ); // TranslatableField.None
            Manager.RegisterDelegate<ReportingLight>((Context roContext, ICtObjectManager voMgr, string vsID1, string vsID2) => 
                                                        (ICtObject)((ICtReportingManager)voMgr).Reporting[
                                                            roContext.GetRefValue(Dims.Phase, vsID1).FCValue(), 
                                                            roContext.GetRefValue(Dims.UpdPer, vsID2).FCValue()]
                                                    );
        }

        // Argument-less constructor
        public ReportingLight() { }

        public string Phase;
        public string UpdatePeriod;
        public string FrameworkVersion;

        public DateTime ReportingStartDate;
        public DateTime ReportingEndDate;

        public override void ReadFrom(ICtObject roObject, Context roContext)
        {
            base.ReadFrom(roObject, roContext);


            if (!(roObject is null))
            {
                ICtReporting reporting = (ICtReporting)roObject;

                Phase = reporting.Phase.Name;
                UpdatePeriod = reporting.UpdatePeriod.Name;
                FrameworkVersion = reporting.FrameworkVersion.Name;
                ReportingStartDate = reporting.ReportingStartDate;
                ReportingEndDate = reporting.ReportingEndDate;
            }
            else
            {
                // Reasonable defaults to pass the validations
                ReportingStartDate = DateTime.Now;
                ReportingEndDate = DateTime.Now.AddMonths(1);
            }
        }

        public override void WriteInto(ICtObject roObject, MessageList roMess, Context roContext)
        {
            base.WriteInto(roObject, roMess, roContext);

            // Not used
            // _oLog.Debug($"Writen  {this.GetType().Name} {Name}");
        }

        public override bool IsValid(Context roContext, MessageList roMess)
        {
            return base.IsValid(roContext, roMess);
        }


        public override bool Exists(Context roContext)
        {
            bool bRet;
            if (ID != 0)
            {
                bRet = Manager.TryGetFCObject(roContext, ID, GetType(), out _);
            }
            else
            {
                if (String.IsNullOrEmpty(Phase) || string.IsNullOrEmpty(UpdatePeriod))
                {
                    bRet = false;
                }
                else
                {
                    bRet = Manager.TryGetFCObject(roContext, Phase, UpdatePeriod, GetType(), out _);
                }
            }
            return bRet;
        }

    }


    public class Reporting : ReportingLight
    {
        private static readonly ILog _oLog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static Reporting() 
        {
            Manager.Register<Reporting>((int)CtReportingManagers.CT_REPORTING_MANAGER, (int)(LanguageMasks.ShortDesc | LanguageMasks.LongDesc | LanguageMasks.Comment)); // TranslatableField.None
            // Get reporting from phase and updper
            Manager.RegisterDelegate<Reporting>((Context roContext, ICtObjectManager voMgr, string vsID1, string vsID2) =>
                                                        (ICtObject)((ICtReportingManager)voMgr).Reporting[
                                                            roContext.GetRefValue(Dims.Phase, vsID1).FCValue(),
                                                            roContext.GetRefValue(Dims.UpdPer, vsID2).FCValue()]
                                                    );
        }

        // Argument-less constructor
        public Reporting() { }

        public int Status;
        public Framework Framework;
        //public ExchangeRate ExchangeRate;
        //public ExchangeRateUpdatePeriod ExchangeRateUpdatePeriod;
        //public ExchangeRateVersion ExchangeRateVersion;
        //public ExchangeRateType ExchangeRateType;

        //public uint ReportingModifyComment;
        //public DateTime ReportingHierarchyDate;

        public Package DefaultPackage                 = new Package();
        public Restriction DefaultRestriction         = new Restriction();
        public Operation DefaultOperation             = new Operation();
        public List<EntityReporting> EntityReportings = new List<EntityReporting>();


        public override void ReadFrom(ICtObject roObject, Context roContext)
        {
            base.ReadFrom(roObject, roContext);

            if (!(roObject is null))
            {
                ICtReporting oFC = (ICtReporting)roObject;

                Status = (int)oFC.Status;
                Framework = new Framework();
                Framework.ReadFrom((ICtObject)oFC.Framework, roContext);

                DefaultPackage.ReadFrom(roObject, roContext);
                DefaultRestriction.ReadFrom(roObject, roContext);
                DefaultOperation.ReadFrom(roObject, roContext);

                EntityReporting o;
                if (!(oFC.RelatedEntityReportingCollection is null))
                {
                    foreach (ICtEntityReporting oFCDetail in oFC.RelatedEntityReportingCollection)
                    {
                        o = new EntityReporting();
                        o.ReadFrom(oFCDetail, roContext);
                        EntityReportings.Add(o);
                    }
                }

                _oLog.Debug($"Read {Phase} - {UpdatePeriod}");
            }
            else
            {
                // Set reasonable default
                DefaultOperation.PackPublishingCutOffDate = ReportingEndDate;
            }
        }

        public override void WriteInto(ICtObject roObject, MessageList roMess, Context roContext)
        {
            // Set default descriptions if none is set
            if ((Phase != "") && (UpdatePeriod != ""))
            {
                RefValue oPhase = roContext.GetRefValue(Dims.Phase, Phase);
                string s;
                foreach ((lang_t, string) o in roContext.Language.SupportedLanguages)
                {
                    s = GetDesc(o.Item2, LanguageText.Type.Long);
                    if ((s is null) && (String.IsNullOrEmpty(Language.Description(roObject, LanguageText.Type.Long, o.Item1))))
                        SetDesc(o.Item2, LanguageText.Type.Long, oPhase.GetDesc(o.Item2, LanguageText.Type.Long) + " - " + UpdatePeriod);
                }
            }

            base.WriteInto(roObject, roMess, roContext);

            ICtReporting oFC = (ICtReporting)roObject;

            RefValue oUpdPer = roContext.GetRefValue(Dims.UpdPer, UpdatePeriod);
            Framework = roContext.Get<Framework>(Phase, FrameworkVersion);

            oFC.UpdatePeriod = oUpdPer.FCValue();
            oFC.Framework = Framework.FCValue();

            // From ReportingLight
            oFC.ReportingStartDate = ReportingStartDate;
            oFC.ReportingEndDate = ReportingEndDate;

            // Status is not saved

            DefaultPackage.WriteInto(oFC, roMess, roContext, Framework);
            DefaultRestriction.WriteInto(oFC, roMess, roContext);
            DefaultOperation.WriteInto(oFC, roMess, roContext, Framework);

            foreach (EntityReporting o in EntityReportings)
            {
                o.WriteInto(oFC, roMess, roContext, Framework);
            }

            _oLog.Debug($"Writen {this.GetType().Name} {Phase} - {UpdatePeriod}");
        }



        public override bool IsValid(Context roContext, MessageList roMess)
        {
            bool bRet = base.IsValid(roContext, roMess);

            if (bRet) bRet = (!(ReportingStartDate == default)) && (!(ReportingEndDate == default)) && (ReportingStartDate <= ReportingEndDate);
            if (!bRet) roMess.Add("RF0510");
            if (bRet) bRet = (!(DefaultOperation.PackPublishingCutOffDate == default)) 
                    && (ReportingStartDate <= DefaultOperation.PackPublishingCutOffDate) 
                    && (DefaultOperation.PackPublishingCutOffDate <= ReportingEndDate);
            if (!bRet) roMess.Add("RF0511");
            if (bRet) bRet = (DefaultOperation.AfterPublication.Level is null) ^ (DefaultOperation.AfterPublication.Advanced);
            if (!bRet) roMess.Add("RF0513");
            if (bRet) bRet = (DefaultOperation.AfterTransfer.Level is null) ^ (DefaultOperation.AfterTransfer.Advanced);
            if (!bRet) roMess.Add("RF0514");
            foreach (EntityReporting o in EntityReportings)
            {
                if (bRet)
                {
                    bRet = (!(o.PackOperation.PackPublishingCutOffDate == default)) 
                        && (ReportingStartDate <= o.PackOperation.PackPublishingCutOffDate) 
                        && (o.PackOperation.PackPublishingCutOffDate <= ReportingEndDate);
                    if (!bRet) roMess.Add("RF0512", o.Entity);
                    if (bRet) bRet = (o.PackOperation.AfterPublication.Level is null) ^ (o.PackOperation.AfterPublication.Advanced);
                    if (!bRet) roMess.Add("RF0515", o.Entity);
                    if (bRet) bRet = (o.PackOperation.AfterTransfer.Level is null) ^ (o.PackOperation.AfterTransfer.Advanced);
                    if (!bRet) roMess.Add("RF0516", o.Entity);
                }
                else
                {
                    break;
                }
            }

            return bRet;
        }



        public static List<Reporting> LoadFromDataSet(DataSet voData, Context roContext, MessageList roMessages)
        {
            List<Reporting> oRet = new List<Reporting>();

            IControl oCtrl = new ControlColumnsExist() { TableName = "Table", RequiredColumns = new List<string> 
                                                                    { "Phase", "UpdatePeriod", "FrameworkVersion", "ReportingStartDate", "ReportingEndDate" } };
            if (oCtrl.Pass(voData, roMessages))
            {
                HashSet<int> oInvalidRows = new HashSet<int>();
                new ControlValidateColumn("Table", "Phase", roContext.GetRefValues(Dims.Phase)).Pass(voData, roMessages, oInvalidRows, true, null);
                new ControlValidateColumn("Table", "UpdatePeriod", Context.GetPeriodValidator).Pass(voData, roMessages, oInvalidRows, true, null);
                new ControlValidateColumn("Table", "FrameworkVersion", roContext.GetRefValues(Dims.FrameworkVersion)).Pass(voData, roMessages, oInvalidRows, true, null);

                Reporting oFullRep;
                int c = 0;
                foreach (DataRow o in voData.Tables["Table"].Rows) 
                {
                    if (!oInvalidRows.Contains(c))
                    { 
                        if (!roContext.TryGet<Reporting>((string)o["Phase"], (string)o["UpdatePeriod"], out oFullRep))
                        {
                            // New reporting to create
                            oFullRep = new Reporting();
                            oFullRep.ReadFrom(null, roContext);    // sets up the object, equivalent to constructor TODO: maybe a Construct method?
                        } // Else Update existing reporting, already loaded

                        oFullRep.Phase = (string)o["Phase"];
                        oFullRep.UpdatePeriod = (string)o["UpdatePeriod"];
                        oFullRep.Name = oFullRep.Phase + " - " + oFullRep.UpdatePeriod;
                        oFullRep.FrameworkVersion = (string)o["FrameworkVersion"];
                        // Check framework is published
                        if (!roContext.TryGet<Framework>(oFullRep.Phase, oFullRep.FrameworkVersion, out oFullRep.Framework))
                        {
                            roMessages.Add("RF0010", oFullRep.Phase, oFullRep.FrameworkVersion);
                        }
                        else
                        {
                            if (oFullRep.Framework.Status != CTKREFLib.kref_framework_status.FRMK_STATUS_PUBLISHED)
                            {
                                roMessages.Add("RF0010", oFullRep.Phase, oFullRep.FrameworkVersion);
                            }
                            else
                            {
                                oFullRep.ReportingStartDate = (DateTime)o["ReportingStartDate"];
                                oFullRep.ReportingEndDate = (DateTime)o["ReportingEndDate"];
                                // TODO  Should not save each time. Maybe get the reporting from the list
                                // Save only once and not per line
                                roContext.Save<Reporting>(oFullRep, roMessages);
                                oRet.Add(oFullRep);
                            }
                        }
                    }
                    c++;
                }
            }
            return oRet;
        }
    }



    public class EntityReporting : ManagedObject
    {
        private static readonly ILog _oLog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool _bDontSaveName = true;

        static EntityReporting()
        {
            // No manager, read from reporting
            //Manager.Register<ReportingLight>((int)CtReportingManagers.CT_REPORTING_MANAGER, (int)LanguageMasks.LongDesc); // TranslatableField.None
        }

        // Argument-less constructor
        public EntityReporting() { }

        public string Entity;
        public string InputCurrency;
        public string InputSite;
        public string PublicationSite;

        public Package PackPackage = new Package();
        public Restriction PackRestriction = new Restriction();
        public Operation PackOperation = new Operation();

        public override void ReadFrom(ICtObject roObject, Context roContext)
        {
            //base.ReadFrom(roObject, roContext);


            if (!(roObject is null))
            {
                ICtEntityReporting oFC = (ICtEntityReporting)roObject;

                Entity          = oFC.Entity.Name;
                InputCurrency   = oFC.InputCurrency.Name;
                InputSite       = oFC.InputRecipient.Name;
                PublicationSite = oFC.PublishingRecipient.Name;
                PackPackage.ReadFrom(roObject, roContext);
                PackRestriction.ReadFrom(roObject, roContext);
                PackOperation.ReadFrom(roObject, roContext);
            }
        }

        public void WriteInto(ICtObject roObject, MessageList roMess, Context roContext, Framework voFramework)
        {
            ICtEntityReporting oFC = (ICtEntityReporting)roObject;
            oFC.Entity = roContext.GetRefValue(Dims.Entity, Entity).FCValue();
            oFC.InputCurrency = roContext.GetRefValue(Dims.Currency, InputCurrency).FCValue();
            oFC.InputRecipient = roContext.Get<Recipient>(InputSite).FCValue();
            oFC.PublishingRecipient = roContext.Get<Recipient>(PublicationSite).FCValue();
            PackPackage.WriteInto(roObject, roMess, roContext, voFramework);
            PackRestriction.WriteInto(roObject, roMess, roContext);
            PackOperation.WriteInto(roObject, roMess, roContext);

            _oLog.Debug($"Writen  {this.GetType().Name} {Name}");
        }
    }



    public class Package : ManagedObject
    {
        public static bool _bDontSaveName = true;

        static Package()
        {
            // No manager, read from reporting
            //Manager.Register<ReportingLight>((int)CtReportingManagers.CT_REPORTING_MANAGER, (int)LanguageMasks.LongDesc); // TranslatableField.None
        }

        // Argument-less constructor
        public Package() { }

        public bool UseDefaultWindowsFolder = true;
        public string WindowsFolder;
        public bool UseDefaultInternetFolder = true;
        public string InternetFolder;
        public bool UseDefaultSetOfControls = true;
        public string SetOfControls;
        public bool UseDefaultLevel = true;
        public short? LevelToReach;
        public bool? Blocking;
        public bool UseDefaultLock = true;
        public int? LockOnPublication;
        public bool UseDefaultRuleSet = true;
        public string RuleSet;
        public bool UseDefaultOpbal = true;
        public bool HasOpBal;
        public string OpbPhase;
        public string OpbUpdatePeriod;
        public string OpbScope;
        public string OpbVariant;
        public string OpbConsCurrency;


        public override void ReadFrom(ICtObject roObject, Context roContext)
        {
            // No need from the base, no ID nor name
            //base.ReadFrom(roObject, roContext);

            UseDefaultWindowsFolder  = (0 != (sbyte)roObject.PropVal[-514174]);
            WindowsFolder            = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_WINDOWS_INPUT_FOLDER]))?.Name;
            UseDefaultInternetFolder = (0 != (sbyte)roObject.PropVal[-514173]);
            InternetFolder           = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_INTERNET_INPUT_FOLDER]))?.Name;
            UseDefaultSetOfControls  = (0 != (sbyte)roObject.PropVal[-514172]);
            SetOfControls            = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_CONTROL_SET]))?.Name;
            UseDefaultLevel          = (0 != (sbyte)roObject.PropVal[-514170]);
            LevelToReach             = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_CONTROL_LEVEL]))?.Rank;
            Blocking                 = (0 != (sbyte)roObject.PropVal[-514187]);
            UseDefaultLock           = (0 != (sbyte)roObject.PropVal[-514171]);
            LockOnPublication        = (int)roObject.PropVal[-514188];
            UseDefaultRuleSet        = (0 != (sbyte)roObject.PropVal[-514169]);        // ToDo check the set rule default
            RuleSet                  = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_RULE_SET]))?.Name;
            UseDefaultOpbal          = (0 != (sbyte)roObject.PropVal[-514168]);
            HasOpBal                 = (0 != (sbyte)roObject.PropVal[-514186]);
            OpbPhase                 = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_OP_BAL_PHASE]))?.Name;
            OpbUpdatePeriod          = roContext.GetRefValue(Dims.UpdPer, (int)roObject.PropVal[-514175])?.Name;
            OpbScope                 = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_OP_BAL_SCOPE_CODE]))?.Name;
            OpbVariant               = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_OP_BAL_VERSION]))?.Name;
            OpbConsCurrency          = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_OP_BAL_CONSOLIDATION_CURRENCY]))?.Name;
        }

        public void WriteInto(ICtObject roObject, MessageList roMess, Context roContext, Framework voFramework)
        {
            //base.WriteInto(roObject, roMess, roContext);

            ICtReporting oFC = (ICtReporting)roObject;
            oFC.PropVal[-514174] = (sbyte)(UseDefaultWindowsFolder ? 1 : 0);
            if (!UseDefaultWindowsFolder) oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_WINDOWS_INPUT_FOLDER] = roContext.Get<Folder>(WindowsFolder);

            oFC.PropVal[-514173] = (sbyte)(UseDefaultInternetFolder ? 1 : 0);
            if (!UseDefaultInternetFolder) oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_INTERNET_INPUT_FOLDER] = roContext.Get<Folder>(InternetFolder);

            oFC.PropVal[-514172] = (sbyte)(UseDefaultSetOfControls ? 1 : 0);
            if (!UseDefaultSetOfControls) oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_CONTROL_SET] = voFramework.GetSetOfControl(SetOfControls);

            oFC.PropVal[-514170] = (sbyte)(UseDefaultLevel ? 1 : 0);
            if (!UseDefaultLevel)
            {
                oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_CONTROL_LEVEL] = voFramework.GetControlLevel(LevelToReach);
                oFC.PropVal[-514187] = (sbyte)(Blocking is null ? 0 : ((bool)Blocking ? 1 : 0));
            }

            oFC.PropVal[-514171] = (sbyte)(UseDefaultLock ? 1 :0);
            if (!UseDefaultLock) oFC.PropVal[-514188] = (LockOnPublication == 2 ? 2 :(LockOnPublication == 1 ? 1 : 0));

            oFC.PropVal[-514169] = (sbyte)(UseDefaultRuleSet ? 1 : 0);
            // ToDo set the ruleset
            // RuleSet = oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_RULE_SET]?.Name;  

            oFC.PropVal[-514168] = (sbyte)(UseDefaultOpbal ? 1 : 0);
            if (!UseDefaultOpbal)
            {
                oFC.PropVal[-514186] = (sbyte)(HasOpBal ? 1 : 0);
                oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_OP_BAL_PHASE] = roContext.GetRefValue(Dims.Phase, OpbPhase).FCValue();
                oFC.PropVal[-514175] = roContext.GetRefValue(Dims.UpdPer, OpbUpdatePeriod).ID;
                oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_OP_BAL_SCOPE_CODE] = roContext.GetRefValue(Dims.Scope, OpbScope).FCValue();
                oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_OP_BAL_VERSION] = roContext.GetRefValue(Dims.Variant, OpbVariant).FCValue();
                oFC.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_OP_BAL_CONSOLIDATION_CURRENCY] = roContext.GetRefValue(Dims.Currency, OpbConsCurrency).FCValue();
                // TODO: Trigger on ct_reporting to update ct_reporting_period_trans
                // Maybe search the dll for table name to see a closeby function
            }
        }
    }



    public class Restriction : ManagedObject
    {
        public static bool _bDontSaveName = true;

        static Restriction()
        {
            // No manager, read from reporting
            //Manager.Register<ReportingLight>((int)CtReportingManagers.CT_REPORTING_MANAGER, (int)LanguageMasks.LongDesc); // TranslatableField.None
        }

        // Argument-less constructor
        public Restriction() { }

        // Fields
        //bool UseDefaultReadOnlyFlows = true;
        //bool IsROFlowsFilter = false;
        //List<string> ROFlows = new List<string>();
        //string ROFlowsFilter;

        //bool UseDefaultOnlyIfOPBal = true;
        //bool IsLockOnlyIfOPBal;

        //bool UseDefaultReadOnlyPeriods = true;
        //bool IsROPeriodsFilter = false;
        //List<string> ROPeriods = new List<string>();
        //string ROPeriodsFilter;

        //bool UseDefaultRestriction = true;
        //string RestrictionName;

        //bool UseDefaultROTechOrig = true;
        //bool IsIntercoRO;

        public override void ReadFrom(ICtObject roObject, Context roContext)
        {
            // TODO: read something rather than use defaults
        }

        public override void WriteInto(ICtObject roObject, MessageList roMess, Context roContext)
        {
            //base.WriteInto(roObject, roMess, roContext);

            // TODO: Save. Uses defaults if nothing is saved.
        }
    }



    public class Operation : ManagedObject
    {
        public static bool _bDontSaveName = true;

        static Operation()
        {
            // No manager, read from reporting
            //Manager.Register<ReportingLight>((int)CtReportingManagers.CT_REPORTING_MANAGER, (int)LanguageMasks.LongDesc); // TranslatableField.None
        }

        // Argument-less constructor
        public Operation() { }


        public class IntegrateMode
        {
            private const int PrStandardMask = 0x10000;
            private const int PrSpecialMask = 0x20000;
            private const int PrAdvancedMask = 0x40000;

            public bool Standard;
            public bool Special;
            public bool Advanced;
            public short? Level;

            public int GetFlag() => (Standard ? PrStandardMask : 0) | (Special ? PrSpecialMask : 0) | (Advanced ? PrAdvancedMask : 0);
            public void SetFlag (int viFlag) { Standard = ((viFlag & PrStandardMask) != 0); Special = ((viFlag & PrSpecialMask) != 0); Advanced = ((viFlag & PrAdvancedMask) != 0); }

            public IntegrateMode() { }

            public IntegrateMode(int viFlag) { SetFlag(viFlag); }
        }

        public bool UseDefaultPublish = true;
        public DateTime PackPublishingCutOffDate;
        public bool AllowEarlyPublishing;

        public bool UseDefaultAfterPub = true;
        public IntegrateMode AfterPublication = new IntegrateMode();

        public bool UseDefaultAfterTran = true;
        public IntegrateMode AfterTransfer = new IntegrateMode();


        public override void ReadFrom(ICtObject roObject, Context roContext)
        {
            // No need from the base, no ID nor name
            //base.ReadFrom(roObject, roContext);

            // Need this because the flags are not present in Reporting, only in EntityReporting
            bool IsFCPropTrue(ICtObject roFC, CtReportingProperties viPropID)
            {
                sbyte? i = (sbyte?)roFC.PropVal[(int)viPropID];
                return (i is null) ? false : i != 0;
            }

            if (!(roObject is null))
            {
                UseDefaultPublish = IsFCPropTrue(roObject, CtReportingProperties.CT_PROP_USE_DEFAULT_PUBLISHING_PROPS);
                if (!UseDefaultPublish)
                {
                    PackPublishingCutOffDate = (DateTime)roObject.PropVal[(int)CtReportingProperties.CT_PROP_PACK_PUBLISHING_CUTOFF_DATE];
                    AllowEarlyPublishing = (0 != (sbyte)roObject.PropVal[(int)CtReportingProperties.CT_PROP_ALLOW_EARLY_PUBLISHING]);
                }
                UseDefaultAfterPub = IsFCPropTrue(roObject, CtReportingProperties.CT_PROP_USE_DEFAULT_INTEGRATION_PROPS);
                if (!UseDefaultAfterPub)
                {
                    AfterPublication.SetFlag((int)roObject.PropVal[(int)CtReportingProperties.CT_PROP_INTEGRATE_AFTER_PUB]);
                    AfterPublication.Level = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_CTRL_LEVEL_REACHED_PUB]))?.Rank;
                }
                UseDefaultAfterTran = IsFCPropTrue(roObject, CtReportingProperties.CT_PROP_USE_DEFAULT_DELIVERY_PROPS);
                if (!UseDefaultAfterTran)
                {
                    AfterTransfer.SetFlag((int)roObject.PropVal[(int)CtReportingProperties.CT_PROP_INTEGRATE_AFTER_TRANSFER]);
                    AfterTransfer.Level = ((dynamic)(roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_CTRL_LEVEL_REACHED_TRANSFER]))?.Rank;
                }
            }
        }

        // Do not override because we need the framework
        public void WriteInto(ICtObject roObject, MessageList roMess, Context roContext, Framework voFramework)
        {
            // No need from the base, no ID nor name

            void SetFCBool(ICtObject roFC, CtReportingProperties viPropID, bool vbValue) {roFC.PropVal[(int)viPropID] = (sbyte)(vbValue ? 1 : 0);}


            SetFCBool(roObject, CtReportingProperties.CT_PROP_USE_DEFAULT_PUBLISHING_PROPS, UseDefaultPublish);
            if (!UseDefaultPublish)
            {
                roObject.PropVal[(int)CtReportingProperties.CT_PROP_PACK_PUBLISHING_CUTOFF_DATE] = PackPublishingCutOffDate;
                roObject.PropVal[(int)CtReportingProperties.CT_PROP_ALLOW_EARLY_PUBLISHING] = (sbyte)(AllowEarlyPublishing ? 1 : 0);
            }
            SetFCBool(roObject, CtReportingProperties.CT_PROP_USE_DEFAULT_INTEGRATION_PROPS, UseDefaultAfterPub);
            if (!UseDefaultAfterPub)
            {
                roObject.PropVal[(int)CtReportingProperties.CT_PROP_INTEGRATE_AFTER_PUB] = AfterPublication.GetFlag();
                roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_CTRL_LEVEL_REACHED_PUB] = voFramework.GetControlLevel(AfterPublication.Level)?.FCValue();
            }
            SetFCBool(roObject, CtReportingProperties.CT_PROP_USE_DEFAULT_DELIVERY_PROPS, UseDefaultAfterTran);
            if (!UseDefaultAfterTran)
            {
                roObject.PropVal[(int)CtReportingProperties.CT_PROP_INTEGRATE_AFTER_TRANSFER] = AfterTransfer.GetFlag();
                roObject.RelVal[(int)CtReportingRelationships.CT_REL_REPORTING_CTRL_LEVEL_REACHED_TRANSFER] = voFramework.GetControlLevel(AfterTransfer.Level)?.FCValue();
            }
        }
    }

}

