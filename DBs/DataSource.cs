//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BTraderWPF.DBs
{
    using System;
    using System.Collections.Generic;
    
    public partial class DataSource
    {
        public DataSource()
        {
            this.DataSource1 = new HashSet<DataSource>();
            this.FeatureValues = new HashSet<FeatureValue>();
        }
    
        public int Id { get; set; }
        public string AggregationCode { get; set; }
        public Nullable<int> OtherDataSourceId { get; set; }
    
        public virtual ICollection<DataSource> DataSource1 { get; set; }
        public virtual DataSource DataSource2 { get; set; }
        public virtual ICollection<FeatureValue> FeatureValues { get; set; }
    }
}
