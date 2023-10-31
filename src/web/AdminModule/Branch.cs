namespace FfAdmin.AdminModule;

public readonly record struct Branch(string Name)
{
    public static implicit operator Branch(string name)
        => new(name);

    public static implicit operator string(Branch branch)
        => branch.Name;
}
