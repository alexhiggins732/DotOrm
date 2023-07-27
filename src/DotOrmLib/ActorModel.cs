using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotOrmLib
{
    public interface IAuditableEntity<T>
    {
        T? Id { get; }
        DateTime? Created { get; }
        string? CreatedBy { get; }
        DateTime? Modified { get; }
        string? ModifiedBy { get; }
    }
    public class AuditableEntityBase<TKey> : IAuditableEntity<TKey>
    {
        public TKey? Id { get; set; }

        public DateTime? Created { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        public string? ModifiedBy { get; set; }
    }
    public class ActorModelBase : AuditableEntityBase<int>
    {

    }
    public class PatientDto
    {
        public int Id { get; set; }

        //pattern is each property Name/SSN/Address/Email/Phone might initial be assumed as a scalar but are really a collection, with a type and an index.
        // The dto can flatten these with a Primary/Secondary/Tertiary/Other (with a 4 collection limit) prefix, seperated by an underscore, entity name, and another underscore and property name
        // each compound prefix (ordinal_entity_) should be suffixed with a type discriminator int as ordinal_entity_{type}
        public string Primary_Name_FirstName { get; set; }
        public string Primary_Name_MiddleName { get; set; }
        public string Primary_Name_LastName { get; set; }
        public string Primary_Name_NamePrefix { get; set; }
        public string Primary_Name_NameSuffix { get; set; }
        public NameType Primary_Name_Type { get; set; }


        public string Secondary_Name_FirstName { get; set; }
        public string Secondary_Name_MiddleName { get; set; }
        public string Secondary_Name_LastName { get; set; }
        public string Secondary_Name_NamePrefix { get; set; }
        public string Secondary_Name_NameSuffix { get; set; }
        public NameType Secondary_Name_Type { get; set; }


        public string Tertiary_Name_FirstName { get; set; }
        public string Tertiary_Name_MiddleName { get; set; }
        public string Tertiary_Name_LastName { get; set; }
        public string Tertiary_Name_NamePrefix { get; set; }
        public string Tertiary_Name_NameSuffix { get; set; }
        public NameType Tertiary_Name_Type { get; set; }


        public string Other_Name_FirstName { get; set; }
        public string Other_Name_MiddleName { get; set; }
        public string Other_Name_LastName { get; set; }
        public string Other_Name_NamePrefix { get; set; }
        public string Other_Name_NameSuffix { get; set; }
        public NameType Other_Name_Type { get; set; }


        public string Primary_Ssn { get; set; }
        // Ssn Type?//bor
        public string Secondary_Ssn { get; set; }

        public string Primary_Email_Address { get; set; }
        public EmailType Primary_Email_Type { get; set; }
        public string Secondary_Email_Address { get; set; }
        public EmailType Secondary_Email_Type { get; set; }
        public string Tertiary_Email_Address { get; set; }
        public EmailType Tertiary_Email_Type { get; set; }
        public string Other_Email_Address { get; set; }
        public EmailType Other_Email_Type { get; set; }


        public string Primary_Phone_Number { get; set; }
        public PhoneType Primary_Phone_Type { get; set; }
        public string Secondary_Phone_Number { get; set; }
        public PhoneType Secondary_Phone_Type { get; set; }
        public string Tertiary_Phone_Number { get; set; }
        public PhoneType Tertiary_Phone_Type { get; set; }
        public string Other_Phone_Number { get; set; }
        public PhoneType Other_Phone_Type { get; set; }

        public string Primary_Address_Address1 { get; set; }
        public string Primary_Address_Address2 { get; set; }
        public string Primary_Address_City { get; set; }
        public string Primary_Address_State { get; set; }
        public string Primary_Address_PostalCode { get; set; }
        public string Primary_Address_Country { get; set; }
        public AddressType Primary_Address_Type { get; set; }


        public string Secondary_Address_Address1 { get; set; }
        public string Secondary_Address_Address2 { get; set; }
        public string Secondary_Address_City { get; set; }
        public string Secondary_Address_State { get; set; }
        public string Secondary_Address_PostalCode { get; set; }
        public string Secondary_Address_Country { get; set; }
        public AddressType Secondary_Address_Type { get; set; }

        public string Tertiary_Address_Address1 { get; set; }
        public string Tertiary_Address_Address2 { get; set; }
        public string Tertiary_Address_City { get; set; }
        public string Tertiary_Address_State { get; set; }
        public string Tertiary_Address_PostalCode { get; set; }
        public string Tertiary_Address_Country { get; set; }
        public AddressType Tertiary_Address_Type { get; set; }

        public string Other_Address_Address1 { get; set; }
        public string Other_Address_Address2 { get; set; }
        public string Other_Address_City { get; set; }
        public string Other_Address_State { get; set; }
        public string Other_Address_PostalCode { get; set; }
        public string Other_Address_Country { get; set; }
        public AddressType Other_Address_Type { get; set; }

        // Various services may also be interested in previously know properties, such as previous address, previous phone, etc..
    }

    public enum NameType
    {
        Unknown = 0,
        Birth = 1,
        Married = 2,
        LegalChange = 3,
        Alias = 4,
        Other = 5,
    }
    public enum EmailType
    {
        Unknown = 0,
        PersonalPrimary = 1,
        PersonalSecondary = 2,
        PersonalTertiary = 3,
        PersonalOther = 4,
        WorkPrimary = 5,
        WorkSecondary = 6,
        WorkTertiary = 7,
        WorkOther = 8,
        Other = 9
    }
    public enum PhoneType
    {
        Unknown = 0,
        HomePrimary = 1,
        HomeSecondary = 2,
        HomeTertiary = 3,
        HomeOther = 4,
        WorkPrimary = 5,
        WorkSecondary = 6,
        WorkTertiary = 7,
        WorkOther = 8,
        PersonalCellPrimary = 9,
        PersonalCellSecondary = 10,
        PersonalCellTertiary = 11,
        PersonalCellOther = 12,
        WorkCellPrimary = 13,
        WorkCellSecondary = 14,
        WorkCellTertiary = 15,
        WorkCellOther = 16,
        PersonalFaxPrimary = 17,
        PersonalFaxSecondary = 18,
        PersonalFaxTertiary = 19,
        PersonalFaxOther = 20,
        WorkFaxPrimary = 21,
        WorkFaxSecondary = 22,
        WorkFaxTertiary = 23,
        WorkFaxOther = 24,
        FamilyContactPrimary = 25,
        FamilyContactSecondary = 26,
        FamilyContactTertiary = 27,
        FamilyContactOther = 28,
        NextOfKinPrimary = 29,
        NextOfKinSecondary = 30,
        NextOfKinTertiary = 31,
        NextOfKinOther = 32,
        WorkContactPrimary = 33,
        WorkContactSecondary = 34,
        WorkContactTertiary = 35,
        WorkContactOther = 36,
        Other = 37,

    }
    public enum AddressType
    {
        Unknown = 0,
        HomePrimary = 1,
        HomeSecondary = 2,
        HomeTertiary = 3,
        HomeOther = 4,
        PoBoxPrimary = 5,
        PoBoxSecondary = 6,
        PoBoxTertiary = 7,
        PoBoxOther = 8,
        WorkPrimary = 9,
        WorkSecondary = 10,
        WorkTertiary = 11,
        WorkOther = 12,
        ShippingPrimary = 13,
        ShippingSecondary = 14,
        ShippingTertiary = 15,
        ShippingOther = 16,

    }
}
