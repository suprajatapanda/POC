namespace TRS.IT.SOA.Model.PreSales.FundLineupData
{
    using System.Xml.Serialization;


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class FMRS
    {

        private FundLineup_Type fundLineupField;

        private Product_Type productField;

        private Contract_Type contractField;

        private MDP_Type mDPField;

        private User_Type userField;

        private Session_Type sessionField;

        private Application_Type applicationField;

        private FundGroup_Type[] fundGroupsField;

        private Index_Type[] indicesField;

        private Note_List_Type[] notesField;

        private string typeField;

        private System.DateTime asOfDateField;

        private System.DateTime lineupDateField;

        private bool lineupDateFieldSpecified;

        private System.DateTime perfDateField;

        private bool perfDateFieldSpecified;

        private System.DateTime scoreDateField;

        private bool scoreDateFieldSpecified;

        private int languageIDField;

        private bool languageIDFieldSpecified;

        public FMRS()
        {
            this.typeField = "FundLineup";
        }

        /// <remarks/>
        public FundLineup_Type FundLineup
        {
            get
            {
                return this.fundLineupField;
            }
            set
            {
                this.fundLineupField = value;
            }
        }

        /// <remarks/>
        public Product_Type Product
        {
            get
            {
                return this.productField;
            }
            set
            {
                this.productField = value;
            }
        }

        /// <remarks/>
        public Contract_Type Contract
        {
            get
            {
                return this.contractField;
            }
            set
            {
                this.contractField = value;
            }
        }

        /// <remarks/>
        public MDP_Type MDP
        {
            get
            {
                return this.mDPField;
            }
            set
            {
                this.mDPField = value;
            }
        }

        /// <remarks/>
        public User_Type User
        {
            get
            {
                return this.userField;
            }
            set
            {
                this.userField = value;
            }
        }

        /// <remarks/>
        public Session_Type Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }

        /// <remarks/>
        public Application_Type Application
        {
            get
            {
                return this.applicationField;
            }
            set
            {
                this.applicationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("FundGroup", IsNullable = false)]
        public FundGroup_Type[] FundGroups
        {
            get
            {
                return this.fundGroupsField;
            }
            set
            {
                this.fundGroupsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Index", IsNullable = false)]
        public Index_Type[] Indices
        {
            get
            {
                return this.indicesField;
            }
            set
            {
                this.indicesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Note", IsNullable = false)]
        public Note_List_Type[] Notes
        {
            get
            {
                return this.notesField;
            }
            set
            {
                this.notesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime AsOfDate
        {
            get
            {
                return this.asOfDateField;
            }
            set
            {
                this.asOfDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime LineupDate
        {
            get
            {
                return this.lineupDateField;
            }
            set
            {
                this.lineupDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineupDateSpecified
        {
            get
            {
                return this.lineupDateFieldSpecified;
            }
            set
            {
                this.lineupDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime PerfDate
        {
            get
            {
                return this.perfDateField;
            }
            set
            {
                this.perfDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PerfDateSpecified
        {
            get
            {
                return this.perfDateFieldSpecified;
            }
            set
            {
                this.perfDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime ScoreDate
        {
            get
            {
                return this.scoreDateField;
            }
            set
            {
                this.scoreDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ScoreDateSpecified
        {
            get
            {
                return this.scoreDateFieldSpecified;
            }
            set
            {
                this.scoreDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LanguageID
        {
            get
            {
                return this.languageIDField;
            }
            set
            {
                this.languageIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LanguageIDSpecified
        {
            get
            {
                return this.languageIDFieldSpecified;
            }
            set
            {
                this.languageIDFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class FundLineup_Type
    {

        private FundLineup_Attribute_Type[] attributesField;

        private string fundLineupIDField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Attribute", IsNullable = false)]
        public FundLineup_Attribute_Type[] Attributes
        {
            get
            {
                return this.attributesField;
            }
            set
            {
                this.attributesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string FundLineupID
        {
            get
            {
                return this.fundLineupIDField;
            }
            set
            {
                this.fundLineupIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class FundLineup_Attribute_Type
    {

        private FundLineup_Attribute_Value_Type[] valueField;

        private string _IDField;

        private string value1Field;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Value")]
        public FundLineup_Attribute_Value_Type[] Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string _ID
        {
            get
            {
                return this._IDField;
            }
            set
            {
                this._IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("Value", DataType = "integer")]
        public string Value1
        {
            get
            {
                return this.value1Field;
            }
            set
            {
                this.value1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class FundLineup_Attribute_Value_Type
    {

        private string _IDField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string _ID
        {
            get
            {
                return this._IDField;
            }
            set
            {
                this._IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Mapping_Type
    {

        private string fromFundNameField;

        private string toFundIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FromFundName
        {
            get
            {
                return this.fromFundNameField;
            }
            set
            {
                this.fromFundNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string ToFundID
        {
            get
            {
                return this.toFundIDField;
            }
            set
            {
                this.toFundIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DefaultFund_Type
    {

        private string fundIDField;

        private string intendedAsQDIAField;

        private string useSeriesField;

        private string qDIASelectedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string FundID
        {
            get
            {
                return this.fundIDField;
            }
            set
            {
                this.fundIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string IntendedAsQDIA
        {
            get
            {
                return this.intendedAsQDIAField;
            }
            set
            {
                this.intendedAsQDIAField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string UseSeries
        {
            get
            {
                return this.useSeriesField;
            }
            set
            {
                this.useSeriesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string QDIASelected
        {
            get
            {
                return this.qDIASelectedField;
            }
            set
            {
                this.qDIASelectedField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class FundLineupData
    {

        private Fund_Type[] fundListField;

        private DefaultFund_Type defaultFundField;

        private string forfeitureFundIDField;

        private Mapping_Type[] investmentMappingField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Fund", IsNullable = false)]
        public Fund_Type[] FundList
        {
            get
            {
                return this.fundListField;
            }
            set
            {
                this.fundListField = value;
            }
        }

        /// <remarks/>
        public DefaultFund_Type DefaultFund
        {
            get
            {
                return this.defaultFundField;
            }
            set
            {
                this.defaultFundField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string ForfeitureFundID
        {
            get
            {
                return this.forfeitureFundIDField;
            }
            set
            {
                this.forfeitureFundIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Mapping", IsNullable = false)]
        public Mapping_Type[] InvestmentMapping
        {
            get
            {
                return this.investmentMappingField;
            }
            set
            {
                this.investmentMappingField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Fund_Type
    {

        private string nameField;

        private Param_Type accountTypeField;

        private TradingRestrictions_Type tradingRestrictionsField;

        private Param_Type shareClassField;

        private RedemptionFee_Type redemptionFeeField;

        private TradingRestrictions_Type managerField;

        private Score_Type[] scoreField;

        private Performance_Type[] performanceField;

        private Fee_Type feesField;

        private XRef_Partner_Type[] xRefField;

        private FundID_Type[] competingFundsField;

        private Note_Type[] notesField;

        private Morningstar_Type[] morningstarField;

        private int fundIDField;

        private int orderIDField;

        private string abbrevField;

        private string tickerField;

        private string identifiedAsField;

        private string fundGroupIDConcatField;

        private string indexIDField;

        private string indexNameField;

        private double indexPerformanceSIField;

        private bool indexPerformanceSIFieldSpecified;

        private double expenseRatioField;

        private bool expenseRatioFieldSpecified;

        private double grossExpenseRatioField;

        private bool grossExpenseRatioFieldSpecified;

        private string pegStatusCurrField;

        private string pegStatusNewField;

        private bool inKitsStatusCurrField;

        private bool inKitsStatusCurrFieldSpecified;

        private int qDIAEligibleField;

        private bool qDIAEligibleFieldSpecified;

        private string pDFFundSheetField;

        private System.DateTime inceptionDateField;

        private bool inceptionDateFieldSpecified;

        private System.DateTime perfInceptionDateField;

        private bool perfInceptionDateFieldSpecified;

        private short bandField;

        private bool bandFieldSpecified;

        private short currentBandField;

        private bool currentBandFieldSpecified;

        private short dontChgBandField;

        private bool dontChgBandFieldSpecified;

        private string cUSIPField;

        private System.DateTime liquidationDateField;

        private bool liquidationDateFieldSpecified;

        private System.DateTime newFundDateField;

        private bool newFundDateFieldSpecified;

        private string pXCustomStatusCurrField;

        private string pXCustomStatusNewField;

        private double pSFValue;
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double PSFValue
        {
            get
            {
                return this.pSFValue;
            }
            set
            {
                this.pSFValue = value;
            }
        }
        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public Param_Type AccountType
        {
            get
            {
                return this.accountTypeField;
            }
            set
            {
                this.accountTypeField = value;
            }
        }

        /// <remarks/>
        public TradingRestrictions_Type TradingRestrictions
        {
            get
            {
                return this.tradingRestrictionsField;
            }
            set
            {
                this.tradingRestrictionsField = value;
            }
        }

        /// <remarks/>
        public Param_Type ShareClass
        {
            get
            {
                return this.shareClassField;
            }
            set
            {
                this.shareClassField = value;
            }
        }

        /// <remarks/>
        public RedemptionFee_Type RedemptionFee
        {
            get
            {
                return this.redemptionFeeField;
            }
            set
            {
                this.redemptionFeeField = value;
            }
        }

        /// <remarks/>
        public TradingRestrictions_Type Manager
        {
            get
            {
                return this.managerField;
            }
            set
            {
                this.managerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Score")]
        public Score_Type[] Score
        {
            get
            {
                return this.scoreField;
            }
            set
            {
                this.scoreField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Performance")]
        public Performance_Type[] Performance
        {
            get
            {
                return this.performanceField;
            }
            set
            {
                this.performanceField = value;
            }
        }

        /// <remarks/>
        public Fee_Type Fees
        {
            get
            {
                return this.feesField;
            }
            set
            {
                this.feesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Partner", IsNullable = false)]
        public XRef_Partner_Type[] XRef
        {
            get
            {
                return this.xRefField;
            }
            set
            {
                this.xRefField = value;
            }
        }
        [System.Xml.Serialization.XmlArrayItemAttribute("FundID")]
        public FundID_Type[] CompetingFunds
        {
            get
            {
                return this.competingFundsField;
            }
            set
            {
                this.competingFundsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Note", IsNullable = false)]
        public Note_Type[] Notes
        {
            get
            {
                return this.notesField;
            }
            set
            {
                this.notesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Morningstar")]
        public Morningstar_Type[] Morningstar
        {
            get
            {
                return this.morningstarField;
            }
            set
            {
                this.morningstarField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int FundID
        {
            get
            {
                return this.fundIDField;
            }
            set
            {
                this.fundIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int OrderID
        {
            get
            {
                return this.orderIDField;
            }
            set
            {
                this.orderIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Abbrev
        {
            get
            {
                return this.abbrevField;
            }
            set
            {
                this.abbrevField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Ticker
        {
            get
            {
                return this.tickerField;
            }
            set
            {
                this.tickerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IdentifiedAs
        {
            get
            {
                return this.identifiedAsField;
            }
            set
            {
                this.identifiedAsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FundGroupIDConcat
        {
            get
            {
                return this.fundGroupIDConcatField;
            }
            set
            {
                this.fundGroupIDConcatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IndexID
        {
            get
            {
                return this.indexIDField;
            }
            set
            {
                this.indexIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IndexName
        {
            get
            {
                return this.indexNameField;
            }
            set
            {
                this.indexNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double IndexPerformanceSI
        {
            get
            {
                return this.indexPerformanceSIField;
            }
            set
            {
                this.indexPerformanceSIField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IndexPerformanceSISpecified
        {
            get
            {
                return this.indexPerformanceSIFieldSpecified;
            }
            set
            {
                this.indexPerformanceSIFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double ExpenseRatio
        {
            get
            {
                return this.expenseRatioField;
            }
            set
            {
                this.expenseRatioField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpenseRatioSpecified
        {
            get
            {
                return this.expenseRatioFieldSpecified;
            }
            set
            {
                this.expenseRatioFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double GrossExpenseRatio
        {
            get
            {
                return this.grossExpenseRatioField;
            }
            set
            {
                this.grossExpenseRatioField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GrossExpenseRatioSpecified
        {
            get
            {
                return this.grossExpenseRatioFieldSpecified;
            }
            set
            {
                this.grossExpenseRatioFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PegStatusCurr
        {
            get
            {
                return this.pegStatusCurrField;
            }
            set
            {
                this.pegStatusCurrField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PegStatusNew
        {
            get
            {
                return this.pegStatusNewField;
            }
            set
            {
                this.pegStatusNewField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool InKitsStatusCurr
        {
            get
            {
                return this.inKitsStatusCurrField;
            }
            set
            {
                this.inKitsStatusCurrField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InKitsStatusCurrSpecified
        {
            get
            {
                return this.inKitsStatusCurrFieldSpecified;
            }
            set
            {
                this.inKitsStatusCurrFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int QDIAEligible
        {
            get
            {
                return this.qDIAEligibleField;
            }
            set
            {
                this.qDIAEligibleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QDIAEligibleSpecified
        {
            get
            {
                return this.qDIAEligibleFieldSpecified;
            }
            set
            {
                this.qDIAEligibleFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PDFFundSheet
        {
            get
            {
                return this.pDFFundSheetField;
            }
            set
            {
                this.pDFFundSheetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime InceptionDate
        {
            get
            {
                return this.inceptionDateField;
            }
            set
            {
                this.inceptionDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InceptionDateSpecified
        {
            get
            {
                return this.inceptionDateFieldSpecified;
            }
            set
            {
                this.inceptionDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime PerfInceptionDate
        {
            get
            {
                return this.perfInceptionDateField;
            }
            set
            {
                this.perfInceptionDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PerfInceptionDateSpecified
        {
            get
            {
                return this.perfInceptionDateFieldSpecified;
            }
            set
            {
                this.perfInceptionDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public short Band
        {
            get
            {
                return this.bandField;
            }
            set
            {
                this.bandField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BandSpecified
        {
            get
            {
                return this.bandFieldSpecified;
            }
            set
            {
                this.bandFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public short CurrentBand
        {
            get
            {
                return this.currentBandField;
            }
            set
            {
                this.currentBandField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CurrentBandSpecified
        {
            get
            {
                return this.currentBandFieldSpecified;
            }
            set
            {
                this.currentBandFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public short DontChgBand
        {
            get
            {
                return this.dontChgBandField;
            }
            set
            {
                this.dontChgBandField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DontChgBandSpecified
        {
            get
            {
                return this.dontChgBandFieldSpecified;
            }
            set
            {
                this.dontChgBandFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CUSIP
        {
            get
            {
                return this.cUSIPField;
            }
            set
            {
                this.cUSIPField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime LiquidationDate
        {
            get
            {
                return this.liquidationDateField;
            }
            set
            {
                this.liquidationDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LiquidationDateSpecified
        {
            get
            {
                return this.liquidationDateFieldSpecified;
            }
            set
            {
                this.liquidationDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime NewFundDate
        {
            get
            {
                return this.newFundDateField;
            }
            set
            {
                this.newFundDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NewFundDateSpecified
        {
            get
            {
                return this.newFundDateFieldSpecified;
            }
            set
            {
                this.newFundDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PXCustomStatusCurr
        {
            get
            {
                return this.pXCustomStatusCurrField;
            }
            set
            {
                this.pXCustomStatusCurrField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PXCustomStatusNew
        {
            get
            {
                return this.pXCustomStatusNewField;
            }
            set
            {
                this.pXCustomStatusNewField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradingRestrictions_Type))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Param_Type
    {

        private int _IDField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int _ID
        {
            get
            {
                return this._IDField;
            }
            set
            {
                this._IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class TradingRestrictions_Type : Param_Type
    {

        private Note_Type[] notesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Note", IsNullable = false)]
        public Note_Type[] Notes
        {
            get
            {
                return this.notesField;
            }
            set
            {
                this.notesField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Note_Type
    {

        private string noteIDField;

        private string orderIDField;

        private string reportIDField;

        private string typeIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string NoteID
        {
            get
            {
                return this.noteIDField;
            }
            set
            {
                this.noteIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string OrderID
        {
            get
            {
                return this.orderIDField;
            }
            set
            {
                this.orderIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string ReportID
        {
            get
            {
                return this.reportIDField;
            }
            set
            {
                this.reportIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string TypeID
        {
            get
            {
                return this.typeIDField;
            }
            set
            {
                this.typeIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RedemptionFee_Type
    {

        private double feeField;

        private string daysField;

        private string textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Fee
        {
            get
            {
                return this.feeField;
            }
            set
            {
                this.feeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string Days
        {
            get
            {
                return this.daysField;
            }
            set
            {
                this.daysField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Score_Type
    {

        private System.DateTime asOfDateField;

        private TimePeriod_Main_Type timePeriodField;

        private bool timePeriodFieldSpecified;

        private int timePeriodOffsetField;

        private bool timePeriodOffsetFieldSpecified;

        private decimal performanceMeasurementField;

        private bool performanceMeasurementFieldSpecified;

        private decimal styleConsistencyField;

        private bool styleConsistencyFieldSpecified;

        private decimal feesExpensesField;

        private bool feesExpensesFieldSpecified;

        private decimal invProcessPortCompField;

        private bool invProcessPortCompFieldSpecified;

        private decimal managementTenureField;

        private bool managementTenureFieldSpecified;

        private decimal organizationField;

        private bool organizationFieldSpecified;

        private decimal totalScoreField;

        private bool totalScoreFieldSpecified;

        private string totalScoreDescField;

        private string statusField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime AsOfDate
        {
            get
            {
                return this.asOfDateField;
            }
            set
            {
                this.asOfDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimePeriod_Main_Type TimePeriod
        {
            get
            {
                return this.timePeriodField;
            }
            set
            {
                this.timePeriodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimePeriodSpecified
        {
            get
            {
                return this.timePeriodFieldSpecified;
            }
            set
            {
                this.timePeriodFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int TimePeriodOffset
        {
            get
            {
                return this.timePeriodOffsetField;
            }
            set
            {
                this.timePeriodOffsetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimePeriodOffsetSpecified
        {
            get
            {
                return this.timePeriodOffsetFieldSpecified;
            }
            set
            {
                this.timePeriodOffsetFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal PerformanceMeasurement
        {
            get
            {
                return this.performanceMeasurementField;
            }
            set
            {
                this.performanceMeasurementField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PerformanceMeasurementSpecified
        {
            get
            {
                return this.performanceMeasurementFieldSpecified;
            }
            set
            {
                this.performanceMeasurementFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal StyleConsistency
        {
            get
            {
                return this.styleConsistencyField;
            }
            set
            {
                this.styleConsistencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StyleConsistencySpecified
        {
            get
            {
                return this.styleConsistencyFieldSpecified;
            }
            set
            {
                this.styleConsistencyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal FeesExpenses
        {
            get
            {
                return this.feesExpensesField;
            }
            set
            {
                this.feesExpensesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FeesExpensesSpecified
        {
            get
            {
                return this.feesExpensesFieldSpecified;
            }
            set
            {
                this.feesExpensesFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal InvProcessPortComp
        {
            get
            {
                return this.invProcessPortCompField;
            }
            set
            {
                this.invProcessPortCompField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InvProcessPortCompSpecified
        {
            get
            {
                return this.invProcessPortCompFieldSpecified;
            }
            set
            {
                this.invProcessPortCompFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal ManagementTenure
        {
            get
            {
                return this.managementTenureField;
            }
            set
            {
                this.managementTenureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ManagementTenureSpecified
        {
            get
            {
                return this.managementTenureFieldSpecified;
            }
            set
            {
                this.managementTenureFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Organization
        {
            get
            {
                return this.organizationField;
            }
            set
            {
                this.organizationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrganizationSpecified
        {
            get
            {
                return this.organizationFieldSpecified;
            }
            set
            {
                this.organizationFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal TotalScore
        {
            get
            {
                return this.totalScoreField;
            }
            set
            {
                this.totalScoreField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotalScoreSpecified
        {
            get
            {
                return this.totalScoreFieldSpecified;
            }
            set
            {
                this.totalScoreFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TotalScoreDesc
        {
            get
            {
                return this.totalScoreDescField;
            }
            set
            {
                this.totalScoreDescField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    public enum TimePeriod_Main_Type
    {

        /// <remarks/>
        Quarter_Last,

        /// <remarks/>
        Quarter_Prev,

        /// <remarks/>
        Month_Last,

        /// <remarks/>
        Daily,

        /// <remarks/>
        Quarter,

        /// <remarks/>
        Month,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Performance_Type
    {

        private TrailingReturn_Type trailingReturnField;

        private System.DateTime asOfDateField;

        private TimePeriod_Main_Type timePeriodField;

        /// <remarks/>
        public TrailingReturn_Type TrailingReturn
        {
            get
            {
                return this.trailingReturnField;
            }
            set
            {
                this.trailingReturnField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime AsOfDate
        {
            get
            {
                return this.asOfDateField;
            }
            set
            {
                this.asOfDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimePeriod_Main_Type TimePeriod
        {
            get
            {
                return this.timePeriodField;
            }
            set
            {
                this.timePeriodField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class TrailingReturn_Type
    {

        private Return_Type returnField;

        /// <remarks/>
        public Return_Type Return
        {
            get
            {
                return this.returnField;
            }
            set
            {
                this.returnField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Return_Type
    {

        private ReturnDetail_Type[] returnDetailField;

        private string asOfDateField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ReturnDetail")]
        public ReturnDetail_Type[] ReturnDetail
        {
            get
            {
                return this.returnDetailField;
            }
            set
            {
                this.returnDetailField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AsOfDate
        {
            get
            {
                return this.asOfDateField;
            }
            set
            {
                this.asOfDateField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ReturnDetail_Type
    {

        private TimePeriod_Detail_Type timePeriodField;

        private decimal valueField;

        private bool valueFieldSpecified;

        private string symbolField;

        private int categoryRankField;

        private bool categoryRankFieldSpecified;

        private int categorySizeField;

        private bool categorySizeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimePeriod_Detail_Type TimePeriod
        {
            get
            {
                return this.timePeriodField;
            }
            set
            {
                this.timePeriodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ValueSpecified
        {
            get
            {
                return this.valueFieldSpecified;
            }
            set
            {
                this.valueFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Symbol
        {
            get
            {
                return this.symbolField;
            }
            set
            {
                this.symbolField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int CategoryRank
        {
            get
            {
                return this.categoryRankField;
            }
            set
            {
                this.categoryRankField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CategoryRankSpecified
        {
            get
            {
                return this.categoryRankFieldSpecified;
            }
            set
            {
                this.categoryRankFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int CategorySize
        {
            get
            {
                return this.categorySizeField;
            }
            set
            {
                this.categorySizeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CategorySizeSpecified
        {
            get
            {
                return this.categorySizeFieldSpecified;
            }
            set
            {
                this.categorySizeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    public enum TimePeriod_Detail_Type
    {

        /// <remarks/>
        ST,

        /// <remarks/>
        M0,

        /// <remarks/>
        M1,

        /// <remarks/>
        M3,

        /// <remarks/>
        M6,

        /// <remarks/>
        M12,

        /// <remarks/>
        M36,

        /// <remarks/>
        M60,

        /// <remarks/>
        M120,

        /// <remarks/>
        M255,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Fee_Type
    {

        private Fee_Contract_Type contractField;

        private Fee_Disclosure_Type disclosureField;

        /// <remarks/>
        public Fee_Contract_Type Contract
        {
            get
            {
                return this.contractField;
            }
            set
            {
                this.contractField = value;
            }
        }

        /// <remarks/>
        public Fee_Disclosure_Type Disclosure
        {
            get
            {
                return this.disclosureField;
            }
            set
            {
                this.disclosureField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Fee_Contract_Type
    {

        private Fee_Scale_Type adminChargeScaleField;

        private int bandIDField;

        private bool bandIDFieldSpecified;

        private double investmentChargeField;

        private bool investmentChargeFieldSpecified;

        private double adminChargeField;

        private bool adminChargeFieldSpecified;

        private double annualFeeReimbursementField;

        private bool annualFeeReimbursementFieldSpecified;

        private double adjustedChargeCreditField;

        private bool adjustedChargeCreditFieldSpecified;

        private double totalIMAdminField;

        private bool totalIMAdminFieldSpecified;

        /// <remarks/>
        public Fee_Scale_Type AdminChargeScale
        {
            get
            {
                return this.adminChargeScaleField;
            }
            set
            {
                this.adminChargeScaleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BandID
        {
            get
            {
                return this.bandIDField;
            }
            set
            {
                this.bandIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BandIDSpecified
        {
            get
            {
                return this.bandIDFieldSpecified;
            }
            set
            {
                this.bandIDFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double InvestmentCharge
        {
            get
            {
                return this.investmentChargeField;
            }
            set
            {
                this.investmentChargeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InvestmentChargeSpecified
        {
            get
            {
                return this.investmentChargeFieldSpecified;
            }
            set
            {
                this.investmentChargeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double AdminCharge
        {
            get
            {
                return this.adminChargeField;
            }
            set
            {
                this.adminChargeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AdminChargeSpecified
        {
            get
            {
                return this.adminChargeFieldSpecified;
            }
            set
            {
                this.adminChargeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double AnnualFeeReimbursement
        {
            get
            {
                return this.annualFeeReimbursementField;
            }
            set
            {
                this.annualFeeReimbursementField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AnnualFeeReimbursementSpecified
        {
            get
            {
                return this.annualFeeReimbursementFieldSpecified;
            }
            set
            {
                this.annualFeeReimbursementFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double AdjustedChargeCredit
        {
            get
            {
                return this.adjustedChargeCreditField;
            }
            set
            {
                this.adjustedChargeCreditField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AdjustedChargeCreditSpecified
        {
            get
            {
                return this.adjustedChargeCreditFieldSpecified;
            }
            set
            {
                this.adjustedChargeCreditFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double TotalIMAdmin
        {
            get
            {
                return this.totalIMAdminField;
            }
            set
            {
                this.totalIMAdminField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotalIMAdminSpecified
        {
            get
            {
                return this.totalIMAdminFieldSpecified;
            }
            set
            {
                this.totalIMAdminFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Fee_Scale_Type
    {

        private Fee_Scale_Band_Type[] bandField;

        private int calcTypeIDField;

        private string calcTypeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Band")]
        public Fee_Scale_Band_Type[] Band
        {
            get
            {
                return this.bandField;
            }
            set
            {
                this.bandField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int CalcTypeID
        {
            get
            {
                return this.calcTypeIDField;
            }
            set
            {
                this.calcTypeIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CalcTypeName
        {
            get
            {
                return this.calcTypeNameField;
            }
            set
            {
                this.calcTypeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Fee_Scale_Band_Type
    {

        private string bandIDField;

        private decimal bandStartField;

        private decimal bandEndField;

        private float bandRateField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string BandID
        {
            get
            {
                return this.bandIDField;
            }
            set
            {
                this.bandIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal BandStart
        {
            get
            {
                return this.bandStartField;
            }
            set
            {
                this.bandStartField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal BandEnd
        {
            get
            {
                return this.bandEndField;
            }
            set
            {
                this.bandEndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public float BandRate
        {
            get
            {
                return this.bandRateField;
            }
            set
            {
                this.bandRateField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Fee_Disclosure_Type
    {

        private int bandIDField;

        private bool bandIDFieldSpecified;

        private double fee_12b1Field;

        private bool fee_12b1FieldSpecified;

        private double otherField;

        private bool otherFieldSpecified;

        private double vACField;

        private bool vACFieldSpecified;

        private double imField;

        private bool imFieldSpecified;

        private double iMFactorField;

        private bool iMFactorFieldSpecified;

        private double adminField;

        private bool adminFieldSpecified;

        private double totalIMAdminField;

        private bool totalIMAdminFieldSpecified;

        private double notRetainedField;

        private bool notRetainedFieldSpecified;

        private double retainedField;

        private bool retainedFieldSpecified;

        private double retainedTotalField;

        private bool retainedTotalFieldSpecified;

        private double underlyingExpenseRatioField;

        private bool underlyingExpenseRatioFieldSpecified;

        private double totalExpenseRatioField;

        private bool totalExpenseRatioFieldSpecified;

        private double recordkeepingCreditField;

        private bool recordkeepingCreditSpecifiedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BandID
        {
            get
            {
                return this.bandIDField;
            }
            set
            {
                this.bandIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BandIDSpecified
        {
            get
            {
                return this.bandIDFieldSpecified;
            }
            set
            {
                this.bandIDFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Fee_12b1
        {
            get
            {
                return this.fee_12b1Field;
            }
            set
            {
                this.fee_12b1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Fee_12b1Specified
        {
            get
            {
                return this.fee_12b1FieldSpecified;
            }
            set
            {
                this.fee_12b1FieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Other
        {
            get
            {
                return this.otherField;
            }
            set
            {
                this.otherField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OtherSpecified
        {
            get
            {
                return this.otherFieldSpecified;
            }
            set
            {
                this.otherFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double VAC
        {
            get
            {
                return this.vACField;
            }
            set
            {
                this.vACField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VACSpecified
        {
            get
            {
                return this.vACFieldSpecified;
            }
            set
            {
                this.vACFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double IM
        {
            get
            {
                return this.imField;
            }
            set
            {
                this.imField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IMSpecified
        {
            get
            {
                return this.imFieldSpecified;
            }
            set
            {
                this.imFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double IMFactor
        {
            get
            {
                return this.iMFactorField;
            }
            set
            {
                this.iMFactorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IMFactorSpecified
        {
            get
            {
                return this.iMFactorFieldSpecified;
            }
            set
            {
                this.iMFactorFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Admin
        {
            get
            {
                return this.adminField;
            }
            set
            {
                this.adminField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AdminSpecified
        {
            get
            {
                return this.adminFieldSpecified;
            }
            set
            {
                this.adminFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double TotalIMAdmin
        {
            get
            {
                return this.totalIMAdminField;
            }
            set
            {
                this.totalIMAdminField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotalIMAdminSpecified
        {
            get
            {
                return this.totalIMAdminFieldSpecified;
            }
            set
            {
                this.totalIMAdminFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double NotRetained
        {
            get
            {
                return this.notRetainedField;
            }
            set
            {
                this.notRetainedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NotRetainedSpecified
        {
            get
            {
                return this.notRetainedFieldSpecified;
            }
            set
            {
                this.notRetainedFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Retained
        {
            get
            {
                return this.retainedField;
            }
            set
            {
                this.retainedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RetainedSpecified
        {
            get
            {
                return this.retainedFieldSpecified;
            }
            set
            {
                this.retainedFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double RetainedTotal
        {
            get
            {
                return this.retainedTotalField;
            }
            set
            {
                this.retainedTotalField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RetainedTotalSpecified
        {
            get
            {
                return this.retainedTotalFieldSpecified;
            }
            set
            {
                this.retainedTotalFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double UnderlyingExpenseRatio
        {
            get
            {
                return this.underlyingExpenseRatioField;
            }
            set
            {
                this.underlyingExpenseRatioField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UnderlyingExpenseRatioSpecified
        {
            get
            {
                return this.underlyingExpenseRatioFieldSpecified;
            }
            set
            {
                this.underlyingExpenseRatioFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double TotalExpenseRatio
        {
            get
            {
                return this.totalExpenseRatioField;
            }
            set
            {
                this.totalExpenseRatioField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotalExpenseRatioSpecified
        {
            get
            {
                return this.totalExpenseRatioFieldSpecified;
            }
            set
            {
                this.totalExpenseRatioFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double RecordkeepingCredit
        {
            get { return this.recordkeepingCreditField; }
            set { this.recordkeepingCreditField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RecordkeepingCreditSpecified
        {
            get { return this.recordkeepingCreditSpecifiedField; }
            set { this.recordkeepingCreditSpecifiedField = value; }
        }



    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class XRef_Partner_Type
    {

        private string partnerIDField;

        private string fundCodeTypeIDField;

        private string fundIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string PartnerID
        {
            get
            {
                return this.partnerIDField;
            }
            set
            {
                this.partnerIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string FundCodeTypeID
        {
            get
            {
                return this.fundCodeTypeIDField;
            }
            set
            {
                this.fundCodeTypeIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FundID
        {
            get
            {
                return this.fundIDField;
            }
            set
            {
                this.fundIDField = value;
            }
        }
    }

    [Serializable()]
    public partial class FundID_Type
    {
        private string fundIDField;

        [XmlText]
        public string FundID
        {
            get
            {
                return fundIDField;
            }
            set
            {
                fundIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Morningstar_Type
    {

        private RiskMeasuresDetail_Type[] riskMeasuresField;

        private MPTDetail_Type[] mPTField;

        private Category_MS_Type categoryField;

        private StarRatingDetail_Type[] starRatingField;

        private TrailingReturn_Type trailingReturnField;

        private System.DateTime asOfDateField;

        private TimePeriod_Main_Type timePeriodField;

        [System.Xml.Serialization.XmlArrayItemAttribute("WaiverDataDetail")]
        public WaiverDataDetail[] WaiverData { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("SevenDayYieldDataDetail")]
        public SevenDayYieldDataDetail[] SevenDayYieldData { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("RiskMeasuresDetail", IsNullable = false)]
        public RiskMeasuresDetail_Type[] RiskMeasures
        {
            get
            {
                return this.riskMeasuresField;
            }
            set
            {
                this.riskMeasuresField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("MPTDetail", IsNullable = false)]
        public MPTDetail_Type[] MPT
        {
            get
            {
                return this.mPTField;
            }
            set
            {
                this.mPTField = value;
            }
        }

        /// <remarks/>
        public Category_MS_Type Category
        {
            get
            {
                return this.categoryField;
            }
            set
            {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("RatingDetail", IsNullable = false)]
        public StarRatingDetail_Type[] StarRating
        {
            get
            {
                return this.starRatingField;
            }
            set
            {
                this.starRatingField = value;
            }
        }

        /// <remarks/>
        public TrailingReturn_Type TrailingReturn
        {
            get
            {
                return this.trailingReturnField;
            }
            set
            {
                this.trailingReturnField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime AsOfDate
        {
            get
            {
                return this.asOfDateField;
            }
            set
            {
                this.asOfDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimePeriod_Main_Type TimePeriod
        {
            get
            {
                return this.timePeriodField;
            }
            set
            {
                this.timePeriodField = value;
            }
        }
    }

    [Serializable()]
    public partial class WaiverDataDetail
    {
        [XmlAttribute()]
        public string FeeWaiver { get; set; }

        [XmlAttribute()]
        public DateTime FeeWaiverExpirationDate { get; set; }
    }

    [Serializable()]
    public partial class SevenDayYieldDataDetail
    {
        [XmlAttribute()]
        public double SevenDayYield { get; set; }
        [XmlAttribute()]
        public DateTime SevenDayYieldEndDate { get; set; }
        [XmlAttribute()]
        public double UnsubsidizedYield { get; set; }
        [XmlAttribute()]
        public DateTime UnsubsidizedYieldDate { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RiskMeasuresDetail_Type
    {

        private TimePeriod_Detail_Type timePeriodField;

        private double sharpeRatioField;

        private bool sharpeRatioFieldSpecified;

        private double standardDeviationField;

        private bool standardDeviationFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimePeriod_Detail_Type TimePeriod
        {
            get
            {
                return this.timePeriodField;
            }
            set
            {
                this.timePeriodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double SharpeRatio
        {
            get
            {
                return this.sharpeRatioField;
            }
            set
            {
                this.sharpeRatioField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SharpeRatioSpecified
        {
            get
            {
                return this.sharpeRatioFieldSpecified;
            }
            set
            {
                this.sharpeRatioFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double StandardDeviation
        {
            get
            {
                return this.standardDeviationField;
            }
            set
            {
                this.standardDeviationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StandardDeviationSpecified
        {
            get
            {
                return this.standardDeviationFieldSpecified;
            }
            set
            {
                this.standardDeviationFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class MPTDetail_Type
    {

        private TimePeriod_Detail_Type timePeriodField;

        private double alphaField;

        private double betaField;

        private double rSquaredField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimePeriod_Detail_Type TimePeriod
        {
            get
            {
                return this.timePeriodField;
            }
            set
            {
                this.timePeriodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Alpha
        {
            get
            {
                return this.alphaField;
            }
            set
            {
                this.alphaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Beta
        {
            get
            {
                return this.betaField;
            }
            set
            {
                this.betaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double RSquared
        {
            get
            {
                return this.rSquaredField;
            }
            set
            {
                this.rSquaredField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Category_MS_Type
    {

        private string _IDField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string _ID
        {
            get
            {
                return this._IDField;
            }
            set
            {
                this._IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class StarRatingDetail_Type
    {

        private TimePeriod_Detail_Type timePeriodField;

        private int typeField;

        private int valueField;

        private string descriptionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimePeriod_Detail_Type TimePeriod
        {
            get
            {
                return this.timePeriodField;
            }
            set
            {
                this.timePeriodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Note_List_Value_Type
    {

        private string languageIDField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LanguageID
        {
            get
            {
                return this.languageIDField;
            }
            set
            {
                this.languageIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Note_List_Type
    {

        private Note_List_Value_Type[] valueField;

        private string _IDField;

        private string noteIDField;

        private string nameField;

        private string symbolField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Value")]
        public Note_List_Value_Type[] Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string _ID
        {
            get
            {
                return this._IDField;
            }
            set
            {
                this._IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string NoteID
        {
            get
            {
                return this.noteIDField;
            }
            set
            {
                this.noteIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Symbol
        {
            get
            {
                return this.symbolField;
            }
            set
            {
                this.symbolField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Morningstar_Index_Type
    {

        private RiskMeasuresDetail_Type[] riskMeasuresField;

        private TrailingReturn_Type trailingReturnField;

        private System.DateTime asOfDateField;

        private TimePeriod_Main_Type timePeriodField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("RiskMeasuresDetail", IsNullable = false)]
        public RiskMeasuresDetail_Type[] RiskMeasures
        {
            get
            {
                return this.riskMeasuresField;
            }
            set
            {
                this.riskMeasuresField = value;
            }
        }

        /// <remarks/>
        public TrailingReturn_Type TrailingReturn
        {
            get
            {
                return this.trailingReturnField;
            }
            set
            {
                this.trailingReturnField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime AsOfDate
        {
            get
            {
                return this.asOfDateField;
            }
            set
            {
                this.asOfDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimePeriod_Main_Type TimePeriod
        {
            get
            {
                return this.timePeriodField;
            }
            set
            {
                this.timePeriodField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Index_Type
    {

        private Morningstar_Index_Type[] morningstarField;

        private Note_Type[] notesField;

        private string indexIDField;

        private string indexNameField;

        private string sequenceIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Morningstar")]
        public Morningstar_Index_Type[] Morningstar
        {
            get
            {
                return this.morningstarField;
            }
            set
            {
                this.morningstarField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Note", IsNullable = false)]
        public Note_Type[] Notes
        {
            get
            {
                return this.notesField;
            }
            set
            {
                this.notesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IndexID
        {
            get
            {
                return this.indexIDField;
            }
            set
            {
                this.indexIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IndexName
        {
            get
            {
                return this.indexNameField;
            }
            set
            {
                this.indexNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SequenceID
        {
            get
            {
                return this.sequenceIDField;
            }
            set
            {
                this.sequenceIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Rule_Type
    {

        private int fundGroupRuleIDField;

        private int typeIDField;

        private bool typeIDFieldSpecified;

        private string descriptionField;

        private string messageField;

        private string queryField;

        private string requireAllField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int FundGroupRuleID
        {
            get
            {
                return this.fundGroupRuleIDField;
            }
            set
            {
                this.fundGroupRuleIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int TypeID
        {
            get
            {
                return this.typeIDField;
            }
            set
            {
                this.typeIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeIDSpecified
        {
            get
            {
                return this.typeIDFieldSpecified;
            }
            set
            {
                this.typeIDFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Query
        {
            get
            {
                return this.queryField;
            }
            set
            {
                this.queryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string RequireAll
        {
            get
            {
                return this.requireAllField;
            }
            set
            {
                this.requireAllField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class FundGroup_Type
    {

        private Rule_Type[] rulesField;

        private XRef_Partner_Type[] xRefField;

        private Note_Type[] notesField;

        private Fund_Type[] fundListField;

        private FundGroup_Type[] fundGroupsField;

        private int fundGroupIDField;

        private string fundGroupIDConcatField;

        private string nameField;

        private int orderIDField;

        private int visibleField;

        private int qDIAEligibleField;

        private bool qDIAEligibleFieldSpecified;

        private int qDIAOptionField;

        private bool qDIAOptionFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Rule", IsNullable = false)]
        public Rule_Type[] Rules
        {
            get
            {
                return this.rulesField;
            }
            set
            {
                this.rulesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Partner", IsNullable = false)]
        public XRef_Partner_Type[] XRef
        {
            get
            {
                return this.xRefField;
            }
            set
            {
                this.xRefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Note", IsNullable = false)]
        public Note_Type[] Notes
        {
            get
            {
                return this.notesField;
            }
            set
            {
                this.notesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Fund", IsNullable = false)]
        public Fund_Type[] FundList
        {
            get
            {
                return this.fundListField;
            }
            set
            {
                this.fundListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("FundGroup", IsNullable = false)]
        public FundGroup_Type[] FundGroups
        {
            get
            {
                return this.fundGroupsField;
            }
            set
            {
                this.fundGroupsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int FundGroupID
        {
            get
            {
                return this.fundGroupIDField;
            }
            set
            {
                this.fundGroupIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FundGroupIDConcat
        {
            get
            {
                return this.fundGroupIDConcatField;
            }
            set
            {
                this.fundGroupIDConcatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int OrderID
        {
            get
            {
                return this.orderIDField;
            }
            set
            {
                this.orderIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int QDIAEligible
        {
            get
            {
                return this.qDIAEligibleField;
            }
            set
            {
                this.qDIAEligibleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QDIAEligibleSpecified
        {
            get
            {
                return this.qDIAEligibleFieldSpecified;
            }
            set
            {
                this.qDIAEligibleFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int QDIAOption
        {
            get
            {
                return this.qDIAOptionField;
            }
            set
            {
                this.qDIAOptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QDIAOptionSpecified
        {
            get
            {
                return this.qDIAOptionFieldSpecified;
            }
            set
            {
                this.qDIAOptionFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Application_Type
    {

        private string applicationIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string ApplicationID
        {
            get
            {
                return this.applicationIDField;
            }
            set
            {
                this.applicationIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Session_Type
    {

        private string sessionIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SessionID
        {
            get
            {
                return this.sessionIDField;
            }
            set
            {
                this.sessionIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class User_Type
    {

        private string usrNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UsrName
        {
            get
            {
                return this.usrNameField;
            }
            set
            {
                this.usrNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class MDP_Type
    {
        private string mDP_QDIASelected;

        private int mDP_RunIDField;

        private bool mDP_RunIDFieldSpecified;

        private string partnerIDField;

        private string fiduciaryServicesProviderIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string QDIASelected
        {
            get
            {
                return this.mDP_QDIASelected;
            }
            set
            {
                this.mDP_QDIASelected = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int MDP_RunID
        {
            get
            {
                return this.mDP_RunIDField;
            }
            set
            {
                this.mDP_RunIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MDP_RunIDSpecified
        {
            get
            {
                return this.mDP_RunIDFieldSpecified;
            }
            set
            {
                this.mDP_RunIDFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string PartnerID
        {
            get
            {
                return this.partnerIDField;
            }
            set
            {
                this.partnerIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string FiduciaryServicesProviderID
        {
            get
            {
                return this.fiduciaryServicesProviderIDField;
            }
            set
            {
                this.fiduciaryServicesProviderIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int DefaultFundID;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DefaultFundIDSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int ForfeitureFundID;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ForfeitureFundIDSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string IntendedAsQDIA;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string UseSeries;

    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Contract_PortfolioExpress_Type
    {

        private bool selectedField;

        private bool selectedFieldSpecified;

        private int glidePathField;

        private bool glidePathFieldSpecified;

        private bool defaultFundField;

        private bool defaultFundFieldSpecified;

        private bool qDIAField;

        private bool qDIAFieldSpecified;

        private bool riskPreferenceField;

        private bool riskPreferenceFieldSpecified;

        private bool customField;

        private bool customFieldSpecified;

        private bool ruleMaterialChangePassField;

        private bool ruleMaterialChangePassFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Selected
        {
            get
            {
                return this.selectedField;
            }
            set
            {
                this.selectedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SelectedSpecified
        {
            get
            {
                return this.selectedFieldSpecified;
            }
            set
            {
                this.selectedFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int GlidePath
        {
            get
            {
                return this.glidePathField;
            }
            set
            {
                this.glidePathField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GlidePathSpecified
        {
            get
            {
                return this.glidePathFieldSpecified;
            }
            set
            {
                this.glidePathFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool DefaultFund
        {
            get
            {
                return this.defaultFundField;
            }
            set
            {
                this.defaultFundField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DefaultFundSpecified
        {
            get
            {
                return this.defaultFundFieldSpecified;
            }
            set
            {
                this.defaultFundFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool QDIA
        {
            get
            {
                return this.qDIAField;
            }
            set
            {
                this.qDIAField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QDIASpecified
        {
            get
            {
                return this.qDIAFieldSpecified;
            }
            set
            {
                this.qDIAFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool RiskPreference
        {
            get
            {
                return this.riskPreferenceField;
            }
            set
            {
                this.riskPreferenceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RiskPreferenceSpecified
        {
            get
            {
                return this.riskPreferenceFieldSpecified;
            }
            set
            {
                this.riskPreferenceFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Custom
        {
            get
            {
                return this.customField;
            }
            set
            {
                this.customField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CustomSpecified
        {
            get
            {
                return this.customFieldSpecified;
            }
            set
            {
                this.customFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool RuleMaterialChangePass
        {
            get
            {
                return this.ruleMaterialChangePassField;
            }
            set
            {
                this.ruleMaterialChangePassField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RuleMaterialChangePassSpecified
        {
            get
            {
                return this.ruleMaterialChangePassFieldSpecified;
            }
            set
            {
                this.ruleMaterialChangePassFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Contract_Type
    {

        private Contract_PortfolioExpress_Type portfolioExpressField;

        private string contractIDField;

        private string subIDField;

        private string defaultFundIDField;

        private QDIA_TMF_Select_Type qDIASelectField;

        private bool qDIASelectFieldSpecified;

        private System.DateTime qDIAStartDateField;

        private bool qDIAStartDateFieldSpecified;

        private QDIA_TMF_Select_Type tMFSelectField;

        private bool tMFSelectFieldSpecified;

        private string forfeitureFundIDField;

        private string partnerIDField;

        private string fiduciaryServicesProviderIDField;

        /// <remarks/>
        public Contract_PortfolioExpress_Type PortfolioExpress
        {
            get
            {
                return this.portfolioExpressField;
            }
            set
            {
                this.portfolioExpressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ContractID
        {
            get
            {
                return this.contractIDField;
            }
            set
            {
                this.contractIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SubID
        {
            get
            {
                return this.subIDField;
            }
            set
            {
                this.subIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string DefaultFundID
        {
            get
            {
                return this.defaultFundIDField;
            }
            set
            {
                this.defaultFundIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public QDIA_TMF_Select_Type QDIASelect
        {
            get
            {
                return this.qDIASelectField;
            }
            set
            {
                this.qDIASelectField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QDIASelectSpecified
        {
            get
            {
                return this.qDIASelectFieldSpecified;
            }
            set
            {
                this.qDIASelectFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime QDIAStartDate
        {
            get
            {
                return this.qDIAStartDateField;
            }
            set
            {
                this.qDIAStartDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QDIAStartDateSpecified
        {
            get
            {
                return this.qDIAStartDateFieldSpecified;
            }
            set
            {
                this.qDIAStartDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public QDIA_TMF_Select_Type TMFSelect
        {
            get
            {
                return this.tMFSelectField;
            }
            set
            {
                this.tMFSelectField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TMFSelectSpecified
        {
            get
            {
                return this.tMFSelectFieldSpecified;
            }
            set
            {
                this.tMFSelectFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string ForfeitureFundID
        {
            get
            {
                return this.forfeitureFundIDField;
            }
            set
            {
                this.forfeitureFundIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string PartnerID
        {
            get
            {
                return this.partnerIDField;
            }
            set
            {
                this.partnerIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string FiduciaryServicesProviderID
        {
            get
            {
                return this.fiduciaryServicesProviderIDField;
            }
            set
            {
                this.fiduciaryServicesProviderIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    public enum QDIA_TMF_Select_Type
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("0000")]
        Item0000,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("0001")]
        Item0001,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Product_Type
    {

        private int productIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int ProductID
        {
            get
            {
                return this.productIDField;
            }
            set
            {
                this.productIDField = value;
            }
        }
    }
}