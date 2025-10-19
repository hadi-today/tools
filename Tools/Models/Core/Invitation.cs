namespace Tools.Models.Core;

public class Invitation
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public string Token { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}
