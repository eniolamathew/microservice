using System;

namespace MicroServices.Shared.API.Models.Address
{
    public class AddressResultEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string Phone { get; set; }
        public string ImportId { get; set; }
        public bool IsDeleted { get; set; }


        public override bool Equals(Object obj)
        {
            var addressToCompare = obj as AddressResultEntity;
            return addressToCompare.Id == Id &&
                   addressToCompare.ContactName == ContactName &&
                   addressToCompare.AddressLine1 == AddressLine1 &&
                   addressToCompare.AddressLine2 == AddressLine2 &&
                   addressToCompare.AddressLine3 == AddressLine3 &&
                   addressToCompare.City == City &&
                   addressToCompare.County == County &&
                   addressToCompare.PostCode == PostCode &&
                   addressToCompare.CountryId == CountryId &&
                   addressToCompare.Phone == Phone;
        }
    }
}