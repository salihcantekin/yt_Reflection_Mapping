using System.Text.Json.Serialization;

namespace Mapping.Reflection.Models;

public class UserEntity
{
    //public int Id { get; set; }

    [Custom("Email")]
    public string EmailAddress { get; set; }

    //public string FirstName { get; set; }

    //public string LastName { get; set; }

    //public List<Address> Addresses { get; set; }

    public List<string> AdditionalProperties { get; private set; }
}
