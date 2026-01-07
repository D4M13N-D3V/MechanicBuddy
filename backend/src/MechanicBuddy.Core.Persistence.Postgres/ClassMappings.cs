using System; 
using System.Data.Common; 
using MechanicBuddy.Core.Domain;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes; 

namespace MechanicBuddy.Core.Persistence.Postgres.Repositories
{
    public static class IdentityInsertExtensions
    {
        public static IdentityPart DefaultGeneratedBy<IdentityPart>(this IdentityGenerationStrategyBuilder<IdentityPart> builder)
        {
            return builder.GuidComb();
        }
    }

    public class UtcDateType : IUserType
    {
        public UtcDateType()
        {
        }

        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        public object DeepCopy(object value)
        {
            var orig = value as Nullable<DateTime>;
            if (orig == null) return null;
            var newVal = new DateTime(
                ((DateTime)orig).Ticks);
            return newVal;
        }

        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        public new bool Equals(object x, object y)
        { 
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            if (x == null)
            {
                throw new ArgumentNullException("Argument cannot be null.");
            }
            return x.GetHashCode();
        }

       
        public bool IsMutable
        {
            get { return false; }
        }

       

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var timestamp = NHibernateUtil.DateTime.NullSafeGet(rs, names[0], session, owner);
            if (timestamp != null && timestamp is DateTime)
            {
                return DateTime.SpecifyKind(((DateTime)timestamp), DateTimeKind.Local);
            }
            return null;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (value != null)
            {
                value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
            }
            NHibernateUtil.DateTime.NullSafeSet(cmd, value, index, session);
        }

    

        public Type ReturnedType
        {
            get
            {
                return typeof(Nullable<DateTime>);
            }
        }
         
        SqlType[] IUserType.SqlTypes => new[] { new SqlType(System.Data.DbType.DateTime) };
    }

    public class TenantRequisitesMapping : ClassMap<TenantRequisites>
    {
        public TenantRequisitesMapping()
        {
            Schema("tenant_config");
            Table("requisites");

            Id(x => x.Id)
                .Column("id")
                .GeneratedBy
                .DefaultGeneratedBy();

            Map(x => x.Name).Column("name").Access.BackingField();
            Map(x => x.Phone).Column("phone").Access.BackingField();
            Map(x => x.Address).Column("address").Access.BackingField();
            Map(x => x.Email).Column("email").Access.BackingField();
            Map(x => x.BankAccount).Column("bank_account").Access.BackingField();
            Map(x => x.RegNr).Column("reg_nr").Access.BackingField();
            Map(x => x.KMKR).Column("tax_id").Access.BackingField();
            Map(x => x.CreatedAt).Column("created_at").Access.BackingField();
            Map(x => x.UpdatedAt).Column("updated_at").Access.BackingField();
        }
    }

    public class TenantPricingMapping : ClassMap<TenantPricing>
    {
        public TenantPricingMapping()
        {
            Schema("tenant_config");
            Table("pricing");

            Id(x => x.Id)
                .Column("id")
                .GeneratedBy
                .DefaultGeneratedBy();

            Map(x => x.VatRate).Column("vat_rate").Access.BackingField();
            Map(x => x.SurCharge).Column("surcharge").Access.BackingField();
            Map(x => x.Disclaimer).Column("disclaimer").Access.BackingField();
            Map(x => x.SignatureLine).Column("signature_line").Access.BackingField();
            Map(x => x.InvoiceEmailContent).Column("invoice_email_content").Access.BackingField();
            Map(x => x.EstimateEmailContent).Column("estimate_email_content").Access.BackingField();
            Map(x => x.CreatedAt).Column("created_at").Access.BackingField();
            Map(x => x.UpdatedAt).Column("updated_at").Access.BackingField();
        }
    }

    public class ServiceOfferedMapping : SubclassMap<ServiceOffered>
    {
        public ServiceOfferedMapping()
        {
            Schema("domain"); 
            Table("serviceoffered"); 
            KeyColumn("id");

            References(Reveal.Member<ServiceOffered, Offer>("offer"), "offerid").Access.LowerCaseField(); 
        }
    }
    public class ServicePerformedMapping : SubclassMap<ServicePerformed>
    {
        public ServicePerformedMapping()
        {
            Schema("domain");
            Table("serviceperformed");
            KeyColumn("id");
            
            References(x => x.Mechanic).Column("mechanicid").Access.BackingField(); ;
            Map(x => x.Notes).Column("notes").Access.BackingField(); ;

            References(Reveal.Member<ServicePerformed, RepairJob>("job"), "repairjobid").Access.LowerCaseField();
        }
    }
    public class ProductOfferedMapping : SubclassMap<ProductOffered>
    {
        public ProductOfferedMapping()
        {
            Schema("domain");
            Table("productoffered");
            KeyColumn("id");
            Map(x => x.Jnr).Column("jnr").Access.BackingField();
            Map(x => x.Code).Column("code");
            References(x => x.Service).Column("serviceid");

            References(x => x.Offer, "offerid").Access.BackingField().LazyLoad(); 
        }
    }
    public class ProductInstalledMapping : SubclassMap<ProductInstalled>
    {
        public ProductInstalledMapping()
        {
            Schema("domain");
            Table("productinstalled");
            KeyColumn("id");

            Map(x => x.Code).Column("code").Access.BackingField();
            Map(x => x.Notes).Column("notes").Access.BackingField();
            Map(x => x.Jnr).Column("jnr").Access.BackingField();
            Map(x => x.Status).Column("status").CustomType<int>().Access.BackingField();

            References(x => x.Service).Column("serviceid").Access.BackingField();

            References(x=>x.Job, "repairjobid").Access.BackingField().LazyLoad();
        }
    }


    public class SaleableMapping : ClassMap<Saleable>
    {
        public SaleableMapping()
        {
            Schema("domain");
            Table("saleable");
         

            Id(x => x.Id)
            .Column("id")
            .GeneratedBy
             .DefaultGeneratedBy();

            Map(x => x.Name).Column("name");
            Map(x => x.Quantity).Column("quantity");
            Map(x => x.Unit).Column("unit");
            Map(x => x.Price).Column("price");
            Map(x => x.Discount).Column("discount");
           
        }
    }
     
    public class AssignmentMapping : ClassMap<Assignment>
    {
        public AssignmentMapping()
        {
            Table("assignment");
            Schema("domain");
             
            CompositeId()
                 .KeyReference(x=>x.Mechanic,c=>c.Access.BackingField(),"mechanicid")
                    .KeyReference(Reveal.Member<Assignment>("work"), c => c.Access.LowerCaseField(), "workid");
        }
    }

   
    public class OfferMapping : ClassMap<Offer>
    {
        public OfferMapping()
        {
            Schema("domain");
            Table("offer");

            Id(x => x.Id)
              .Column("id")
              .GeneratedBy
               .DefaultGeneratedBy();
             

            References(x=>x.Work, "workid").Access.LowerCaseField();
            Map(x => x.OrderNr).Column("ordernr");
            Map(x => x.Notes).Column("notes");
            Map(x => x.IsVehicleLinesOnEstimate).Column("isvehilelesonestimate");
            Map(x => x.AcceptedOn).Column("acceptedon").CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
            Map(x => x.StartedOn).Column("startedon").CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
            References(x => x.Starter).Column("starterid").Access.BackingField();
            References(x => x.Estimate).Column("estimateid").Access.BackingField().Cascade.SaveUpdate();
            References(x => x.Acceptor).Column("acceptorid").Access.BackingField();

            HasMany(x => x.Products).KeyColumn("offerid").Access.LowerCaseField().Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Services).KeyColumn("offerid").Access.LowerCaseField().Cascade.AllDeleteOrphan().Inverse();
        }
    }

    public class RepairJobMapping : ClassMap<RepairJob>
    {
        public RepairJobMapping()
        {
            Schema("domain");
            Table("repairjob");

            Id(x => x.Id)
              .Column("id")
              .GeneratedBy
               .DefaultGeneratedBy();
             
            References(x => x.Work, "workid").Access.LowerCaseField();

            Map(x => x.Notes).Column("notes");
            Map(x => x.OrderNr).Column("ordernr");
            Map(x => x.StartedOn).Column("startedon").CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
             
            References(x => x.Starter).Column("starterid").Access.BackingField();

            HasMany(x => x.Products).KeyColumn("repairjobid").Access.LowerCaseField().Cascade.AllDeleteOrphan().Inverse() ;
            HasMany(x => x.Services).KeyColumn("repairjobid").Access.LowerCaseField().Cascade.AllDeleteOrphan().Inverse() ; 
        }
    }

    public class WorkMapping : ClassMap<Work>
    {
        public WorkMapping()
        {
            Schema("domain");
            Table("work");


            Id(x => x.Id)
            .Column("id")
            .GeneratedBy
             .DefaultGeneratedBy();

            Map(x => x.Number).Column("number").Access.BackingField().Unique();
            Map(x => x.StartedOn).Column("startedon").CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
            References(x => x.Invoice).Column("invoiceid").Access.BackingField().Cascade.SaveUpdate(); 
            References(x => x.Client).Column("clientid").Access.LowerCaseField();
            References(x => x.Starter).Column("starterid").Access.BackingField();
             
            References(x => x.Vehicle).Column("vehicleid").Access.BackingField(); 
          
            HasMany(x => x.Offers).KeyColumn("workid").Access.LowerCaseField().Cascade.AllDeleteOrphan().Inverse().LazyLoad();
            HasMany(x => x.Jobs).KeyColumn("workid").Access.LowerCaseField().Cascade.AllDeleteOrphan().Inverse().LazyLoad();
            
            Map(x => x.Notes).Column("notes").Access.BackingField();
            Map(x => x.CompletedOn).Column("completedon").CustomType<UtcDateType>().Access.BackingField(); ;//.HasConversion<DateTimeUtcConverter>();
            Map(x => x.ChangedOn).Column("changedon").CustomType<UtcDateType>().Access.BackingField();
            Map(x => x.Odo).Column("odo").Access.BackingField();
            Map(x => x.UserStatus).Column("userstatus").Access.BackingField();
             
            References(x => x.Completer).Column("completerid").Access.BackingField();

            HasMany<Assignment>(Reveal.Member<Work>("assignements"))
                .Inverse()
                .Cascade
                .AllDeleteOrphan()
                .KeyColumn("workid")
                .Access
                .LowerCaseField(); 

        }
    }
    public class PricingLineMapping : ClassMap<PricingLine>
    {
        public PricingLineMapping()
        {
            Schema("domain");
            Table("pricingline");

            CompositeId()
               .KeyReference(Reveal.Member<PricingLine>("pricing"), k => k.Access.LowerCaseField(), "pricingid")
               .KeyProperty(x => x.Nr, k => k.ColumnName("nr").Access.BackingField());

             
          
            Map(x => x.Description).Column("description").Access.BackingField();
            Map(x => x.Quantity).Column("quantity").Access.BackingField();
            Map(x => x.UnitPrice).Column("unitprice").Access.BackingField();
            Map(x => x.Unit).Column("unit").Access.BackingField();
            Map(x => x.Discount).Column("discount").Access.BackingField();
            Map(x => x.Total).Column("total").Access.BackingField();
            Map(x => x.TotalWithVat).Column("totalwithvat").Access.BackingField();
        }
    }

    public class InvoiceMapping : SubclassMap<Invoice>
    {
        public InvoiceMapping()
        { 
            Schema("domain");
            Table("invoice");
            KeyColumn("id");
            Map(x => x.Number).Column("number").Access.BackingField().Unique();
            Map(x => x.PaymentType).Column("paymenttype").CustomType<int>().Access.BackingField();
            Map(x => x.DueDays).Column("duedays").Access.BackingField();
            Map(x => x.IsPaid).Column("ispaid").Access.BackingField();
            Map(x => x.IsCredited).Column("iscredited").Access.BackingField();
        }
    }
    public class EstimateMapping : SubclassMap<Estimate>
    {
        public EstimateMapping()
        {
            Schema("domain");
            Table("estimate");
            KeyColumn("id");
            Map(x => x.Number).Column("number").Access.BackingField().Unique();
        }
    }
    public class PricingMapping : ClassMap<Pricing>
    {
        public PricingMapping()
        {
            Schema("domain");
            Table("pricing");


            Id(x => x.Id)
            .Column("id")
            .GeneratedBy
             .DefaultGeneratedBy();
             
            Map(x => x.SentOn).Column("senton").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
            Map(x => x.PrintedOn).Column("printedon").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
            Map(x => x.Email).Column("email").Access.BackingField();
            Map(x => x.PartyName).Column("partyname").Access.BackingField();
            Map(x => x.PartyAddress).Column("partyaddress").Access.BackingField();
            Map(x => x.PartyCode).Column("partycode").Access.BackingField();
            Map(x => x.VehicleLine1).Column("vehicleline1").Access.BackingField();
            Map(x => x.VehicleLine2).Column("vehicleline2").Access.BackingField();
            Map(x => x.VehicleLine3).Column("vehicleline3").Access.BackingField();
            Map(x => x.VehicleLine4).Column("vehicleline4").Access.BackingField();
            Map(x => x.IssuedOn).Column("issuedon").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();

           
            References(x => x.Issuer).Column("issuerid").Access.BackingField();

           
            HasMany(x => x.Lines)
                       .KeyColumn("pricingid").Access.LowerCaseField().Inverse()
                       .Cascade.AllDeleteOrphan();


        }
    }
  
    public class VehicleMapping : ClassMap<Vehicle>
    {
        public VehicleMapping()
        {
            Schema("domain");
            Table("vehicle");


            Id(x => x.Id)
            .Column("id")
            .GeneratedBy
             .DefaultGeneratedBy();

            Map(x => x.Producer).Column("producer").Access.BackingField();
            Map(x => x.Model).Column("model").Access.BackingField();
            Map(x => x.RegNr).Column("regnr").Access.BackingField();
            Map(x => x.Vin).Column("vin").Access.BackingField();
            Map(x => x.Odo).Column("odo").Access.BackingField();
            Map(x => x.Body).Column("body").Access.BackingField();
            Map(x => x.DrivingSide).Column("drivingside").Access.BackingField();
            Map(x => x.Engine).Column("engine").Access.BackingField();
            Map(x => x.ProductionDate).Column("productiondate").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
            Map(x => x.Region).Column("region").Access.BackingField();
            Map(x => x.Series).Column("series").Access.BackingField();
            Map(x => x.Transmission).Column("transmission").Access.BackingField();
            Map(x => x.Description).Column("description").Access.BackingField();
            Map(x => x.IntroducedAt).Column("introducedat").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();


            HasMany(x => x.Registrations).KeyColumn("vehicleid").Access.LowerCaseField().Cascade.AllDeleteOrphan().Inverse();
        }
    }

    public class LegalClientMapping : SubclassMap<LegalClient>
    {
        public LegalClientMapping()
        {
            Schema("domain");
            Table("legalclient");
            KeyColumn("id");
            Map(x => x.Name).Column("name").Access.LowerCaseField();
            Map(x => x.RegNr).Column("regnr").Access.BackingField();
        }
    }
    public class PrivateClientMapping : SubclassMap<PrivateClient>
    {
        public PrivateClientMapping()
        {
            Schema("domain");
            Table("privateclient");
            KeyColumn("id");
            Map(x => x.FirstName).Column("firstname").Access.BackingField();
            Map(x => x.LastName).Column("lastname").Access.BackingField();
            Map(x => x.PersonalCode).Column("personalcode").Access.BackingField();
        }
    }
    public class ClientMapping : ClassMap<Client>
    {
        public ClientMapping()
        {
            Schema("domain");
            Table("client");


            Id(x => x.Id)
            .Column("id")
            .GeneratedBy
             .DefaultGeneratedBy();


            Component(x => x.Address,
                 c =>
                 {
                     c.Map(a => a.Street).Column("address");
                     c.Map(a => a.Country).Column("country");
                     c.Map(a => a.Region).Column("region");
                     c.Map(a => a.City).Column("city");
                     c.Map(a => a.PostalCode).Column("postalcode");
                 }
             );
              
            Map(x => x.Phone).Column("phone").Access.BackingField();
            Map(x => x.IsAsshole).Column("isasshole").Access.BackingField();
            Map(x => x.Description).Column("description").Access.BackingField();
            Map(x => x.IntroducedAt).Column("introducedat").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();


            HasMany(x => x.EmailAddresses).Access.CamelCaseField()
                       .KeyColumn("clientid").Inverse() 
                       .Cascade.AllDeleteOrphan();
             
        }
    }
    public class VehicleRegistrationMapping : ClassMap<VehicleRegistration>
    {
        public VehicleRegistrationMapping()
        {
            Schema("domain");
            Table("vehicleregistration");
            LazyLoad();


            CompositeId()
            .KeyReference(x => x.Owner, c => c.Access.BackingField(), "ownerid")
            .KeyReference(Reveal.Member<VehicleRegistration>("vehicle"), c => c.Access.LowerCaseField(), "vehicleid")
            .KeyProperty(x => x.DateTimeFrom, c => c.ColumnName("datetimefrom").Access.BackingField()).CustomType<UtcDateType>();


             Map(x => x.DateTimeTo).Column("datetimeto").Access.BackingField().CustomType<UtcDateType>();

        }
    }
    public class ClientEmailMapping : ClassMap<ClientEmail>
    {
        public ClientEmailMapping()
        {
            Schema("domain");
            Table("clientemail");
             
            CompositeId()
            .KeyProperty(x => x.Address, x=>x.ColumnName("address").Access.BackingField())
            .KeyReference(Reveal.Member<ClientEmail>("client"), c=>c.Access.LowerCaseField(),"clientid"); 
            Map(x => x.IsActive).Access.BackingField().Column("isactive");
            
        }
    }


    public class EmployeeDbMapping : ClassMap<Employee>
    {
        public EmployeeDbMapping()
        {
            Schema("domain");
            Table("employee");


            Id(x => x.Id)
            .Column("id")
            .GeneratedBy
             .DefaultGeneratedBy();
             
            Map(x => x.FirstName).Column("firstname").Access.BackingField();
            Map(x => x.LastName).Column("lastname").Access.BackingField();
            Map(x => x.Phone).Column("phone").Access.BackingField();
            Map(x => x.Email).Column("email").Access.BackingField();
            Map(x => x.Proffession).Column("proffession").Access.BackingField();
            Map(x => x.Description).Column("description").Access.BackingField();
            Map(x => x.IntroducedAt).Column("introducedat").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
        }
    }
    public class StorageDbMapping : ClassMap<Storage>
    {
        public StorageDbMapping()
        {
            Schema("domain");
            Table("storage");


            Id(x => x.Id)
            .Column("id")
            .GeneratedBy
             .DefaultGeneratedBy();

           
            Map(x => x.Name).Column("name").Access.LowerCaseField();
            Map(x => x.Address).Column("address").Access.LowerCaseField();
            Map(x => x.Description).Column("description").Access.BackingField();
            Map(x => x.IntroducedAt).Column("introducedat").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
        }
    }
    public class SparePartDbMapping : ClassMap<SparePart>
    {
        public SparePartDbMapping()
        {
            Schema("domain");
            Table("sparepart");


            Id(x => x.Id)
            .Column("id")
            .GeneratedBy
             .DefaultGeneratedBy();
            
            Map(x => x.Code).Column("code").Access.LowerCaseField();
            Map(x => x.Name).Column("name").Access.LowerCaseField();
            Map(x => x.Price).Column("price").Access.LowerCaseField();
            Map(x => x.Quantity).Column("quantity").Access.LowerCaseField();
            Map(x => x.Discount).Column("discount").Access.LowerCaseField();
            Map(x => x.Description).Column("description").Access.BackingField();
            Map(x => x.IntroducedAt).Column("introducedat").Access.BackingField().CustomType<UtcDateType>();//.HasConversion<DateTimeUtcConverter>();
            
            References(x => x.Storage).Column("storageid").Access.LowerCaseField();

            References(x => x.UmPrice).Column("umpriceid").Access.BackingField().Cascade.AllDeleteOrphan();
            
        }

    }

    public class UmPriceDbMapping : ClassMap<UmPrice>
    {
        public UmPriceDbMapping()
        {
            Schema("domain");
            Table("unitedmotorsprice");

            Id(x=>x.Id,"id").GeneratedBy.DefaultGeneratedBy();
             
            Map(x => x.Name).Column("name").Access.LowerCaseField();
            Map(x => x.Address).Column("address").Access.LowerCaseField();
            Map(x => x.Price).Column("price").Access.LowerCaseField(); 

        }
    } 

    public class ServiceRequestMapping : ClassMap<ServiceRequest>
    {
        public ServiceRequestMapping()
        {
            Schema("domain");
            Table("servicerequest");

            Id(x => x.Id)
                .Column("id")
                .GeneratedBy
                .DefaultGeneratedBy();

            Map(x => x.CustomerName).Column("customername").Access.BackingField();
            Map(x => x.Phone).Column("phone").Access.BackingField();
            Map(x => x.Email).Column("email").Access.BackingField();
            Map(x => x.VehicleInfo).Column("vehicleinfo").Access.BackingField();
            Map(x => x.ServiceType).Column("servicetype").Access.BackingField();
            Map(x => x.Message).Column("message").Access.BackingField();
            Map(x => x.Status).Column("status").Access.BackingField();
            Map(x => x.SubmittedAt).Column("submittedat").Access.BackingField().CustomType<UtcDateType>();
            Map(x => x.Notes).Column("notes").Access.BackingField();
        }
    }
}
