using System.Text.Json.Serialization;

namespace AssignmentManagement.Api.Domain;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    [JsonIgnore]
    public ICollection<User> Users { get; set; } = new List<User>();
}
